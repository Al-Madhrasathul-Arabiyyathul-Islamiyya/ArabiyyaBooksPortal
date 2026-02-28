using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.Setup.DTOs;
using BooksPortal.Application.Features.Setup.Interfaces;
using BooksPortal.Application.Features.Settings.Services;
using BooksPortal.Domain.Entities;
using BooksPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.Setup.Services;

public class SetupReadinessService : ISetupReadinessService
{
    private static readonly (SlipType SlipType, string Template)[] DefaultReferenceTemplates =
    [
        (SlipType.Distribution, "DST{year}{autonum}"),
        (SlipType.Return, "RTN{year}{autonum}"),
        (SlipType.TeacherIssue, "TIS{year}{autonum}"),
        (SlipType.TeacherReturn, "TRT{year}{autonum}")
    ];

    private readonly IRepository<SystemSetupState> _setupRepository;
    private readonly IRepository<AcademicYear> _academicYearRepository;
    private readonly IRepository<Keystage> _keystageRepository;
    private readonly IRepository<Grade> _gradeRepository;
    private readonly IRepository<ClassSection> _classSectionRepository;
    private readonly IRepository<ReferenceNumberFormat> _referenceFormatRepository;
    private readonly IRepository<SlipTemplateSetting> _slipTemplateRepository;
    private readonly ISetupIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;

    public SetupReadinessService(
        IRepository<SystemSetupState> setupRepository,
        IRepository<AcademicYear> academicYearRepository,
        IRepository<Keystage> keystageRepository,
        IRepository<Grade> gradeRepository,
        IRepository<ClassSection> classSectionRepository,
        IRepository<ReferenceNumberFormat> referenceFormatRepository,
        IRepository<SlipTemplateSetting> slipTemplateRepository,
        ISetupIdentityService identityService,
        IUnitOfWork unitOfWork)
    {
        _setupRepository = setupRepository;
        _academicYearRepository = academicYearRepository;
        _keystageRepository = keystageRepository;
        _gradeRepository = gradeRepository;
        _classSectionRepository = classSectionRepository;
        _referenceFormatRepository = referenceFormatRepository;
        _slipTemplateRepository = slipTemplateRepository;
        _identityService = identityService;
        _unitOfWork = unitOfWork;
    }

    public async Task<SetupStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var state = await GetOrCreateStateAsync(cancellationToken);
        return await EvaluateAndPersistAsync(state, cancellationToken);
    }

    public Task<SetupStatusResponse> GetBootstrapStatusAsync(CancellationToken cancellationToken = default)
        => GetStatusAsync(cancellationToken);

    public async Task<SetupStatusResponse> BootstrapSuperAdminAsync(
        BootstrapSuperAdminRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _identityService.HasAnySuperAdminAsync(cancellationToken))
            throw new BusinessRuleException("System bootstrap is already completed for SuperAdmin account.");

        await _identityService.CreateSuperAdminAsync(
            request.UserName,
            request.Email,
            request.Password,
            request.FullName,
            request.NationalId,
            request.Designation,
            cancellationToken);

        var state = await GetOrCreateStateAsync(cancellationToken);
        state.StartedAtUtc ??= DateTime.UtcNow;
        state.SuperAdminConfirmedAtUtc ??= DateTime.UtcNow;
        state.Status = SetupStatus.InProgress;
        _setupRepository.Update(state);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await EvaluateAndPersistAsync(state, cancellationToken);
    }

    public async Task<SetupStatusResponse> StartAsync(CancellationToken cancellationToken = default)
    {
        var state = await GetOrCreateStateAsync(cancellationToken);
        state.StartedAtUtc ??= DateTime.UtcNow;
        state.Status = SetupStatus.InProgress;
        _setupRepository.Update(state);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await EvaluateAndPersistAsync(state, cancellationToken);
    }

    public async Task<SetupStatusResponse> ConfirmSuperAdminAsync(CancellationToken cancellationToken = default)
    {
        var state = await GetOrCreateStateAsync(cancellationToken);
        if (!await _identityService.HasActiveSuperAdminAsync(cancellationToken))
            throw new BusinessRuleException("No active SuperAdmin account is available.");

        state.SuperAdminConfirmedAtUtc ??= DateTime.UtcNow;
        state.StartedAtUtc ??= DateTime.UtcNow;
        state.Status = SetupStatus.InProgress;
        _setupRepository.Update(state);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await EvaluateAndPersistAsync(state, cancellationToken);
    }

    public async Task<SetupStatusResponse> ConfirmSlipTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var state = await GetOrCreateStateAsync(cancellationToken);

        await EnsureSlipTemplateDefaultsAsync(cancellationToken);
        state.SlipTemplatesConfirmedAtUtc ??= DateTime.UtcNow;
        state.StartedAtUtc ??= DateTime.UtcNow;
        state.Status = SetupStatus.InProgress;
        _setupRepository.Update(state);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await EvaluateAndPersistAsync(state, cancellationToken);
    }

    public async Task<SetupStatusResponse> InitializeHierarchyAsync(CancellationToken cancellationToken = default)
    {
        var state = await GetOrCreateStateAsync(cancellationToken);

        await EnsureDefaultHierarchyAsync(cancellationToken);
        state.HierarchyInitializedAtUtc ??= DateTime.UtcNow;
        state.StartedAtUtc ??= DateTime.UtcNow;
        state.Status = SetupStatus.InProgress;
        _setupRepository.Update(state);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await EvaluateAndPersistAsync(state, cancellationToken);
    }

    public async Task<SetupStatusResponse> InitializeReferenceFormatsAsync(CancellationToken cancellationToken = default)
    {
        var state = await GetOrCreateStateAsync(cancellationToken);
        await EnsureReferenceFormatsForActiveAcademicYearAsync(cancellationToken);

        state.ReferenceFormatsInitializedAtUtc ??= DateTime.UtcNow;
        state.StartedAtUtc ??= DateTime.UtcNow;
        state.Status = SetupStatus.InProgress;
        _setupRepository.Update(state);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await EvaluateAndPersistAsync(state, cancellationToken);
    }

    public async Task<SetupStatusResponse> CompleteAsync(CancellationToken cancellationToken = default)
    {
        var state = await GetOrCreateStateAsync(cancellationToken);
        var response = await EvaluateAndPersistAsync(state, cancellationToken);
        if (!response.IsReady)
        {
            throw BuildIncompleteException(response);
        }

        return response;
    }

    public async Task<SetupStatusResponse> EnsureBackfillStateAsync(CancellationToken cancellationToken = default)
    {
        var state = await GetOrCreateStateAsync(cancellationToken);
        return await EvaluateAndPersistAsync(state, cancellationToken);
    }

    public async Task EnsureReadyOrThrowAsync(CancellationToken cancellationToken = default)
    {
        var status = await GetStatusAsync(cancellationToken);
        if (!status.IsReady)
            throw BuildIncompleteException(status);
    }

    private static SetupIncompleteException BuildIncompleteException(SetupStatusResponse status)
    {
        var missing = status.Issues.Select(x => x.Key).Distinct().ToList();
        var hints = status.Issues
            .Select(x => x.Hint)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .Cast<string>()
            .ToList();

        return new SetupIncompleteException(
            "System setup is incomplete. Complete the required setup checklist before processing operations.",
            missing,
            hints);
    }

    private async Task<SystemSetupState> GetOrCreateStateAsync(CancellationToken cancellationToken)
    {
        var state = await _setupRepository.QueryIgnoringFilters()
            .FirstOrDefaultAsync(x => x.Id == 1, cancellationToken);

        if (state is not null)
            return state;

        state = new SystemSetupState
        {
            Id = 1,
            Status = SetupStatus.NotStarted,
            SchemaVersion = "1"
        };

        _setupRepository.Add(state);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return state;
    }

    private async Task<SetupStatusResponse> EvaluateAndPersistAsync(SystemSetupState state, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var issues = new List<SetupReadinessIssueResponse>();
        var steps = new List<SetupStepStatusResponse>();

        var hasActiveSuperAdmin = await _identityService.HasActiveSuperAdminAsync(cancellationToken);
        if (hasActiveSuperAdmin && state.SuperAdminConfirmedAtUtc is null)
            state.SuperAdminConfirmedAtUtc = now;
        AddStep(
            steps,
            issues,
            SetupReadinessStepKeys.SuperAdmin,
            "SuperAdmin Account",
            hasActiveSuperAdmin,
            state.SuperAdminConfirmedAtUtc,
            "Create or activate a SuperAdmin account.");

        var hasSlipTemplates = await HasRequiredSlipTemplatesAsync(cancellationToken);
        if (hasSlipTemplates && state.SlipTemplatesConfirmedAtUtc is null)
            state.SlipTemplatesConfirmedAtUtc = now;
        AddStep(
            steps,
            issues,
            SetupReadinessStepKeys.SlipTemplates,
            "Slip Templates",
            hasSlipTemplates,
            state.SlipTemplatesConfirmedAtUtc,
            "Confirm slip template defaults or configure template values.");

        var academicYearCount = await _academicYearRepository.Query().CountAsync(cancellationToken);
        var activeAcademicYearIds = await _academicYearRepository.Query()
            .Where(x => x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        var hasExactlyOneActiveAcademicYear = academicYearCount > 0 && activeAcademicYearIds.Count == 1;
        if (hasExactlyOneActiveAcademicYear && state.ActiveAcademicYearValidatedAtUtc is null)
            state.ActiveAcademicYearValidatedAtUtc = now;
        AddStep(
            steps,
            issues,
            SetupReadinessStepKeys.ActiveAcademicYear,
            "Academic Year Activation",
            hasExactlyOneActiveAcademicYear,
            state.ActiveAcademicYearValidatedAtUtc,
            "Ensure academic years exist and exactly one year is active.");

        var hasHierarchy = await _keystageRepository.Query().AnyAsync(cancellationToken)
            && await _gradeRepository.Query().AnyAsync(cancellationToken)
            && await _classSectionRepository.Query().AnyAsync(cancellationToken);
        if (hasHierarchy && state.HierarchyInitializedAtUtc is null)
            state.HierarchyInitializedAtUtc = now;
        AddStep(
            steps,
            issues,
            SetupReadinessStepKeys.Hierarchy,
            "Keystages, Grades, and Classes",
            hasHierarchy,
            state.HierarchyInitializedAtUtc,
            "Initialize curriculum hierarchy and class sections.");

        var hasReferenceFormats = false;
        if (activeAcademicYearIds.Count == 1)
        {
            var activeAcademicYearId = activeAcademicYearIds[0];
            var requiredTypes = DefaultReferenceTemplates.Select(x => x.SlipType).Distinct().ToList();
            var existingTypes = await _referenceFormatRepository.Query()
                .Where(x => x.AcademicYearId == activeAcademicYearId)
                .Select(x => x.SlipType)
                .Distinct()
                .ToListAsync(cancellationToken);
            hasReferenceFormats = requiredTypes.All(existingTypes.Contains);
        }

        if (hasReferenceFormats && state.ReferenceFormatsInitializedAtUtc is null)
            state.ReferenceFormatsInitializedAtUtc = now;
        AddStep(
            steps,
            issues,
            SetupReadinessStepKeys.ReferenceFormats,
            "Reference Number Formats",
            hasReferenceFormats,
            state.ReferenceFormatsInitializedAtUtc,
            "Initialize reference number formats for the active academic year.");

        var isReady = issues.Count == 0;
        state.StartedAtUtc ??= (steps.Any(x => x.IsComplete) ? now : null);
        state.LastEvaluatedAtUtc = now;
        state.Status = isReady
            ? SetupStatus.Completed
            : state.StartedAtUtc is null
                ? SetupStatus.NotStarted
                : SetupStatus.InProgress;
        state.CompletedAtUtc = isReady ? (state.CompletedAtUtc ?? now) : null;

        _setupRepository.Update(state);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SetupStatusResponse
        {
            Status = state.Status,
            IsReady = isReady,
            StartedAtUtc = state.StartedAtUtc,
            LastEvaluatedAtUtc = state.LastEvaluatedAtUtc,
            CompletedAtUtc = state.CompletedAtUtc,
            Steps = steps,
            Issues = issues
        };
    }

    private static void AddStep(
        ICollection<SetupStepStatusResponse> steps,
        ICollection<SetupReadinessIssueResponse> issues,
        string key,
        string title,
        bool isComplete,
        DateTime? completedAtUtc,
        string hint)
    {
        steps.Add(new SetupStepStatusResponse
        {
            Key = key,
            Title = title,
            IsComplete = isComplete,
            CompletedAtUtc = completedAtUtc,
            Hint = hint
        });

        if (isComplete)
            return;

        issues.Add(new SetupReadinessIssueResponse
        {
            Key = key,
            Message = $"{title} is incomplete.",
            Hint = hint
        });
    }

    private async Task<bool> HasRequiredSlipTemplatesAsync(CancellationToken cancellationToken)
    {
        var existing = await _slipTemplateRepository.Query()
            .Select(x => new { x.Category, x.Key })
            .ToListAsync(cancellationToken);
        if (existing.Count == 0)
            return false;

        var existingSet = existing
            .Select(x => $"{x.Category}::{x.Key}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var defaults = SlipTemplateSettingService.GetDefaultLabels();

        return defaults.All(x => existingSet.Contains($"{x.Category}::{x.Key}"));
    }

    private async Task EnsureSlipTemplateDefaultsAsync(CancellationToken cancellationToken)
    {
        var existing = await _slipTemplateRepository.Query()
            .ToListAsync(cancellationToken);

        var existingMap = existing.ToDictionary(x => $"{x.Category}::{x.Key}", StringComparer.OrdinalIgnoreCase);
        foreach (var defaultLabel in SlipTemplateSettingService.GetDefaultLabels())
        {
            var composite = $"{defaultLabel.Category}::{defaultLabel.Key}";
            if (existingMap.ContainsKey(composite))
                continue;

            _slipTemplateRepository.Add(defaultLabel);
        }
    }

    private async Task EnsureDefaultHierarchyAsync(CancellationToken cancellationToken)
    {
        var activeAcademicYear = await _academicYearRepository.Query()
            .OrderByDescending(x => x.Year)
            .FirstOrDefaultAsync(x => x.IsActive, cancellationToken);

        if (activeAcademicYear is null)
        {
            var now = DateTime.UtcNow;
            activeAcademicYear = new AcademicYear
            {
                Name = $"AY {now.Year}",
                Year = now.Year,
                StartDate = new DateTime(now.Year, 1, 1),
                EndDate = new DateTime(now.Year, 12, 31),
                IsActive = true
            };
            _academicYearRepository.Add(activeAcademicYear);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var activeYears = await _academicYearRepository.Query()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Year)
            .ToListAsync(cancellationToken);
        if (activeYears.Count > 1)
        {
            var keeperId = activeYears[0].Id;
            foreach (var year in activeYears)
            {
                year.IsActive = year.Id == keeperId;
                _academicYearRepository.Update(year);
            }
        }

        var curriculum = new[]
        {
            new { Code = "KS1", Name = "Key stage 1", Sort = 1, Grades = new[] { ("G1", "Grade 1", 1), ("G2", "Grade 2", 2), ("G3", "Grade 3", 3) } },
            new { Code = "KS2", Name = "Key stage 2", Sort = 2, Grades = new[] { ("G4", "Grade 4", 4), ("G5", "Grade 5", 5), ("G6", "Grade 6", 6) } },
            new { Code = "KS3", Name = "Key stage 3", Sort = 3, Grades = new[] { ("G7", "Grade 7", 7), ("G8", "Grade 8", 8) } },
            new { Code = "KS4", Name = "Key stage 4", Sort = 4, Grades = new[] { ("G9", "Grade 9", 9), ("G10", "Grade 10", 10) } },
            new { Code = "KS5", Name = "Key stage 5", Sort = 5, Grades = new[] { ("G11", "Grade 11", 11), ("G12", "Grade 12", 12) } }
        };

        foreach (var ks in curriculum)
        {
            var keystage = await _keystageRepository.Query()
                .FirstOrDefaultAsync(x => x.Code == ks.Code, cancellationToken);
            if (keystage is null)
            {
                keystage = new Keystage
                {
                    Code = ks.Code,
                    Name = ks.Name,
                    SortOrder = ks.Sort
                };
                _keystageRepository.Add(keystage);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                keystage.Name = ks.Name;
                keystage.SortOrder = ks.Sort;
                _keystageRepository.Update(keystage);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            foreach (var (gradeCode, gradeName, gradeSort) in ks.Grades)
            {
                var grade = await _gradeRepository.Query()
                    .FirstOrDefaultAsync(x => x.KeystageId == keystage.Id && x.Code == gradeCode, cancellationToken);
                if (grade is null)
                {
                    _gradeRepository.Add(new Grade
                    {
                        KeystageId = keystage.Id,
                        Code = gradeCode,
                        Name = gradeName,
                        SortOrder = gradeSort
                    });
                }
                else
                {
                    grade.Name = gradeName;
                    grade.SortOrder = gradeSort;
                    _gradeRepository.Update(grade);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var activeYearId = (await _academicYearRepository.Query()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Year)
            .Select(x => x.Id)
            .FirstAsync(cancellationToken));

        var gradeIds = await _gradeRepository.Query()
            .OrderBy(x => x.SortOrder)
            .Select(x => new { x.Id, x.KeystageId })
            .ToListAsync(cancellationToken);
        var sections = new[] { "A", "B", "C", "D" };

        foreach (var grade in gradeIds)
        {
            foreach (var section in sections)
            {
                var exists = await _classSectionRepository.Query().AnyAsync(x =>
                    x.AcademicYearId == activeYearId
                    && x.GradeId == grade.Id
                    && x.Section == section, cancellationToken);

                if (exists)
                    continue;

                _classSectionRepository.Add(new ClassSection
                {
                    AcademicYearId = activeYearId,
                    KeystageId = grade.KeystageId,
                    GradeId = grade.Id,
                    Section = section
                });
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureReferenceFormatsForActiveAcademicYearAsync(CancellationToken cancellationToken)
    {
        var activeYear = await _academicYearRepository.Query()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Year)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeYear is null)
            throw new BusinessRuleException("Reference formats require an active academic year.");

        var existingFormats = await _referenceFormatRepository.QueryIgnoringFilters()
            .Where(x => x.AcademicYearId == activeYear.Id)
            .ToListAsync(cancellationToken);

        foreach (var (slipType, template) in DefaultReferenceTemplates)
        {
            var existing = existingFormats.FirstOrDefault(x => x.SlipType == slipType);
            if (existing is null)
            {
                _referenceFormatRepository.Add(new ReferenceNumberFormat
                {
                    AcademicYearId = activeYear.Id,
                    SlipType = slipType,
                    FormatTemplate = template,
                    PaddingWidth = 6
                });
                continue;
            }

            if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                existing.DeletedAt = null;
            }

            if (!string.Equals(existing.FormatTemplate, template, StringComparison.Ordinal)
                || existing.PaddingWidth != 6)
            {
                existing.FormatTemplate = template;
                existing.PaddingWidth = 6;
            }

            _referenceFormatRepository.Update(existing);
        }
    }
}
