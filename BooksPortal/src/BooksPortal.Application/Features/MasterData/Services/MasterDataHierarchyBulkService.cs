using System.Text.RegularExpressions;
using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.MasterData.Services;

public sealed class MasterDataHierarchyBulkService : IMasterDataHierarchyBulkService
{
    private readonly IAcademicYearService _academicYearService;
    private readonly IRepository<AcademicYear> _academicYearRepository;
    private readonly IRepository<Keystage> _keystageRepository;
    private readonly IRepository<Grade> _gradeRepository;
    private readonly IRepository<ClassSection> _classSectionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MasterDataHierarchyBulkService(
        IAcademicYearService academicYearService,
        IRepository<AcademicYear> academicYearRepository,
        IRepository<Keystage> keystageRepository,
        IRepository<Grade> gradeRepository,
        IRepository<ClassSection> classSectionRepository,
        IUnitOfWork unitOfWork)
    {
        _academicYearService = academicYearService;
        _academicYearRepository = academicYearRepository;
        _keystageRepository = keystageRepository;
        _gradeRepository = gradeRepository;
        _classSectionRepository = classSectionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<HierarchyBulkUpsertResponse> UpsertAsync(HierarchyBulkUpsertRequest request)
    {
        if (request.AcademicYears.Count == 0)
            throw new BusinessRuleException("At least one academic year must be provided.");

        var requestedYears = request.AcademicYears
            .Select(x => x.Year)
            .Distinct()
            .ToList();
        var existingRequestedYears = await _academicYearRepository.Query()
            .Where(a => requestedYears.Contains(a.Year))
            .Select(a => a.Year)
            .ToListAsync();
        if (existingRequestedYears.Count > 0)
            throw new BusinessRuleException(
                $"Hierarchy upload rejected. Academic year(s) already exist: {string.Join(", ", existingRequestedYears.OrderBy(x => x))}.");

        var requestedActiveCount = request.AcademicYears.Count(x => x.IsActive == true);
        if (requestedActiveCount > 1)
            throw new BusinessRuleException("Hierarchy upload rejected. Only one academic year can be marked active.");

        var response = new HierarchyBulkUpsertResponse();

        for (var yearIndex = 0; yearIndex < request.AcademicYears.Count; yearIndex++)
        {
            var yearNode = request.AcademicYears[yearIndex];
            var yearPath = $"academicYears[{yearIndex}]";

            try
            {
                ValidateAcademicYearNode(yearNode, yearPath);
                var academicYear = await UpsertAcademicYearAsync(yearNode, yearPath, response);

                for (var keyIndex = 0; keyIndex < yearNode.Keystages.Count; keyIndex++)
                {
                    var keystageNode = yearNode.Keystages[keyIndex];
                    var keystagePath = $"{yearPath}.keystages[{keyIndex}]";

                    try
                    {
                        ValidateKeystageNode(keystageNode, keystagePath);
                        var keystage = await UpsertKeystageAsync(keystageNode, keyIndex, keystagePath, response);

                        for (var gradeIndex = 0; gradeIndex < keystageNode.Grades.Count; gradeIndex++)
                        {
                            var gradeNode = keystageNode.Grades[gradeIndex];
                            var gradePath = $"{keystagePath}.grades[{gradeIndex}]";

                            try
                            {
                                ValidateGradeNode(gradeNode, gradePath);
                                var grade = await UpsertGradeAsync(keystage, gradeNode, gradeIndex, gradePath, response);

                                for (var classIndex = 0; classIndex < gradeNode.Classes.Count; classIndex++)
                                {
                                    var classNode = gradeNode.Classes[classIndex];
                                    var classPath = $"{gradePath}.classes[{classIndex}]";

                                    try
                                    {
                                        ValidateClassNode(classNode, classPath);
                                        await UpsertClassSectionAsync(academicYear, keystage, grade, classNode, classPath, response);
                                    }
                                    catch (Exception ex)
                                    {
                                        AddFailure(response, classPath, "ClassSection", ex.Message);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                AddFailure(response, gradePath, "Grade", ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        AddFailure(response, keystagePath, "Keystage", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                AddFailure(response, yearPath, "AcademicYear", ex.Message);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return response;
    }

    private async Task<AcademicYear> UpsertAcademicYearAsync(
        AcademicYearHierarchyNode node,
        string path,
        HierarchyBulkUpsertResponse response)
    {
        var existing = await _academicYearRepository.Query()
            .FirstOrDefaultAsync(a => a.Year == node.Year);

        if (existing is null)
        {
            var created = new AcademicYear
            {
                Name = node.Name.Trim(),
                Year = node.Year,
                StartDate = node.StartDate?.Date ?? new DateTime(node.Year, 1, 1),
                EndDate = node.EndDate?.Date ?? new DateTime(node.Year, 12, 31),
                IsActive = node.IsActive ?? false
            };

            _academicYearRepository.Add(created);
            await _unitOfWork.SaveChangesAsync();

            if (created.IsActive)
            {
                await _academicYearService.ActivateAsync(created.Id);
            }

            response.CreatedCount++;
            response.Results.Add(new HierarchyBulkUpsertResultRow
            {
                Path = path,
                EntityType = nameof(AcademicYear),
                Operation = "created",
                Message = $"Created academic year {created.Name}."
            });
            return created;
        }

        var changed = false;
        if (!string.Equals(existing.Name, node.Name.Trim(), StringComparison.Ordinal))
        {
            existing.Name = node.Name.Trim();
            changed = true;
        }

        var startDate = node.StartDate?.Date ?? new DateTime(node.Year, 1, 1);
        var endDate = node.EndDate?.Date ?? new DateTime(node.Year, 12, 31);

        if (existing.StartDate != startDate)
        {
            existing.StartDate = startDate;
            changed = true;
        }

        if (existing.EndDate != endDate)
        {
            existing.EndDate = endDate;
            changed = true;
        }

        if (node.IsActive.HasValue && existing.IsActive != node.IsActive.Value)
        {
            existing.IsActive = node.IsActive.Value;
            changed = true;
        }

        if (changed)
        {
            _academicYearRepository.Update(existing);
            response.UpdatedCount++;
            response.Results.Add(new HierarchyBulkUpsertResultRow
            {
                Path = path,
                EntityType = nameof(AcademicYear),
                Operation = "updated",
                Message = $"Updated academic year {existing.Name}."
            });
        }
        else
        {
            response.SkippedCount++;
            response.Results.Add(new HierarchyBulkUpsertResultRow
            {
                Path = path,
                EntityType = nameof(AcademicYear),
                Operation = "skipped",
                Message = "No changes detected."
            });
        }

        return existing;
    }

    private async Task<Keystage> UpsertKeystageAsync(
        KeystageHierarchyNode node,
        int index,
        string path,
        HierarchyBulkUpsertResponse response)
    {
        var normalizedName = node.Name.Trim();
        var existing = await _keystageRepository.Query()
            .FirstOrDefaultAsync(k => k.Name.ToLower() == normalizedName.ToLower());

        if (existing is null)
        {
            var generatedCode = await ResolveKeystageCodeAsync(node.Code, normalizedName, index);
            var created = new Keystage
            {
                Name = normalizedName,
                Code = generatedCode,
                SortOrder = node.SortOrder ?? (index + 1)
            };
            _keystageRepository.Add(created);
            await _unitOfWork.SaveChangesAsync();
            response.CreatedCount++;
            response.Results.Add(new HierarchyBulkUpsertResultRow
            {
                Path = path,
                EntityType = nameof(Keystage),
                Operation = "created",
                Message = $"Created keystage {created.Name}."
            });
            return created;
        }

        var changed = false;
        var targetSort = node.SortOrder ?? existing.SortOrder;
        if (existing.SortOrder != targetSort)
        {
            existing.SortOrder = targetSort;
            changed = true;
        }

        if (changed)
        {
            _keystageRepository.Update(existing);
            response.UpdatedCount++;
            response.Results.Add(new HierarchyBulkUpsertResultRow
            {
                Path = path,
                EntityType = nameof(Keystage),
                Operation = "updated",
                Message = $"Updated keystage {existing.Name}."
            });
        }
        else
        {
            response.SkippedCount++;
            response.Results.Add(new HierarchyBulkUpsertResultRow
            {
                Path = path,
                EntityType = nameof(Keystage),
                Operation = "skipped",
                Message = "No changes detected."
            });
        }

        return existing;
    }

    private async Task<Grade> UpsertGradeAsync(
        Keystage keystage,
        GradeHierarchyNode node,
        int index,
        string path,
        HierarchyBulkUpsertResponse response)
    {
        var normalizedName = node.Name.Trim();
        var existing = await _gradeRepository.Query()
            .FirstOrDefaultAsync(g => g.KeystageId == keystage.Id && g.Name.ToLower() == normalizedName.ToLower());

        if (existing is null)
        {
            var generatedCode = await ResolveGradeCodeAsync(keystage.Id, node.Code, normalizedName, index);
            var created = new Grade
            {
                KeystageId = keystage.Id,
                Name = normalizedName,
                Code = generatedCode,
                SortOrder = node.SortOrder ?? (index + 1)
            };
            _gradeRepository.Add(created);
            await _unitOfWork.SaveChangesAsync();
            response.CreatedCount++;
            response.Results.Add(new HierarchyBulkUpsertResultRow
            {
                Path = path,
                EntityType = nameof(Grade),
                Operation = "created",
                Message = $"Created grade {created.Name}."
            });
            return created;
        }

        var changed = false;
        var targetSort = node.SortOrder ?? existing.SortOrder;
        if (existing.SortOrder != targetSort)
        {
            existing.SortOrder = targetSort;
            changed = true;
        }

        if (changed)
        {
            _gradeRepository.Update(existing);
            response.UpdatedCount++;
            response.Results.Add(new HierarchyBulkUpsertResultRow
            {
                Path = path,
                EntityType = nameof(Grade),
                Operation = "updated",
                Message = $"Updated grade {existing.Name}."
            });
        }
        else
        {
            response.SkippedCount++;
            response.Results.Add(new HierarchyBulkUpsertResultRow
            {
                Path = path,
                EntityType = nameof(Grade),
                Operation = "skipped",
                Message = "No changes detected."
            });
        }

        return existing;
    }

    private async Task UpsertClassSectionAsync(
        AcademicYear academicYear,
        Keystage keystage,
        Grade grade,
        ClassSectionHierarchyNode node,
        string path,
        HierarchyBulkUpsertResponse response)
    {
        var section = node.Section.Trim();
        var existing = await _classSectionRepository.Query()
            .FirstOrDefaultAsync(c =>
                c.AcademicYearId == academicYear.Id &&
                c.GradeId == grade.Id &&
                c.Section.ToLower() == section.ToLower());

        if (existing is null)
        {
            var created = new ClassSection
            {
                AcademicYearId = academicYear.Id,
                KeystageId = keystage.Id,
                GradeId = grade.Id,
                Section = section
            };
            _classSectionRepository.Add(created);
            response.CreatedCount++;
            response.Results.Add(new HierarchyBulkUpsertResultRow
            {
                Path = path,
                EntityType = nameof(ClassSection),
                Operation = "created",
                Message = $"Created class section {grade.Name} {section}."
            });
            return;
        }

        var changed = false;
        if (existing.KeystageId != keystage.Id)
        {
            existing.KeystageId = keystage.Id;
            changed = true;
        }

        if (changed)
        {
            _classSectionRepository.Update(existing);
            response.UpdatedCount++;
            response.Results.Add(new HierarchyBulkUpsertResultRow
            {
                Path = path,
                EntityType = nameof(ClassSection),
                Operation = "updated",
                Message = $"Updated class section {grade.Name} {section}."
            });
        }
        else
        {
            response.SkippedCount++;
            response.Results.Add(new HierarchyBulkUpsertResultRow
            {
                Path = path,
                EntityType = nameof(ClassSection),
                Operation = "skipped",
                Message = "No changes detected."
            });
        }
    }

    private static void ValidateAcademicYearNode(AcademicYearHierarchyNode node, string path)
    {
        if (string.IsNullOrWhiteSpace(node.Name))
            throw new BusinessRuleException($"{path}.name is required.");
        if (node.Year < 1900 || node.Year > 9999)
            throw new BusinessRuleException($"{path}.year is invalid.");
    }

    private static void ValidateKeystageNode(KeystageHierarchyNode node, string path)
    {
        if (string.IsNullOrWhiteSpace(node.Name))
            throw new BusinessRuleException($"{path}.name is required.");
    }

    private static void ValidateGradeNode(GradeHierarchyNode node, string path)
    {
        if (string.IsNullOrWhiteSpace(node.Name))
            throw new BusinessRuleException($"{path}.name is required.");
    }

    private static void ValidateClassNode(ClassSectionHierarchyNode node, string path)
    {
        if (string.IsNullOrWhiteSpace(node.Section))
            throw new BusinessRuleException($"{path}.section is required.");
    }

    private static void AddFailure(HierarchyBulkUpsertResponse response, string path, string entity, string message)
    {
        response.FailedCount++;
        response.Results.Add(new HierarchyBulkUpsertResultRow
        {
            Path = path,
            EntityType = entity,
            Operation = "failed",
            Message = message
        });
    }

    private async Task<string> ResolveKeystageCodeAsync(string? requestedCode, string name, int index)
    {
        var baseCode = string.IsNullOrWhiteSpace(requestedCode)
            ? BuildCodeFromName(name, $"KS{index + 1}")
            : requestedCode.Trim().ToUpperInvariant();

        var code = baseCode;
        var suffix = 1;
        while (await _keystageRepository.AnyAsync(k => k.Code == code))
        {
            code = $"{baseCode}{suffix++}";
        }
        return code;
    }

    private async Task<string> ResolveGradeCodeAsync(int keystageId, string? requestedCode, string name, int index)
    {
        var baseCode = string.IsNullOrWhiteSpace(requestedCode)
            ? BuildCodeFromName(name, $"G{index + 1}")
            : requestedCode.Trim().ToUpperInvariant();

        var code = baseCode;
        var suffix = 1;
        while (await _gradeRepository.AnyAsync(g => g.KeystageId == keystageId && g.Code == code))
        {
            code = $"{baseCode}{suffix++}";
        }
        return code;
    }

    private static string BuildCodeFromName(string name, string fallback)
    {
        var normalized = Regex.Replace(name.ToUpperInvariant(), @"[^A-Z0-9]+", " ").Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return fallback;

        var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
            return parts[0].Length > 10 ? parts[0][..10] : parts[0];

        var code = string.Concat(parts.Select(p => p[0]));
        return code.Length > 10 ? code[..10] : code;
    }
}
