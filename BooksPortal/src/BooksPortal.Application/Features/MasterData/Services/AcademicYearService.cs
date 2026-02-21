using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.MasterData.Services;

public class AcademicYearService : IAcademicYearService
{
    private readonly IRepository<AcademicYear> _repository;
    private readonly IRepository<ClassSection> _classSectionRepository;
    private readonly IRepository<StockEntry> _stockEntryRepository;
    private readonly IRepository<Book> _bookRepository;
    private readonly IRepository<DistributionSlip> _distributionRepository;
    private readonly IRepository<ReturnSlip> _returnRepository;
    private readonly IRepository<TeacherIssue> _teacherIssueRepository;
    private readonly IRepository<ReferenceNumberFormat> _referenceFormatRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AcademicYearService(
        IRepository<AcademicYear> repository,
        IRepository<ClassSection> classSectionRepository,
        IRepository<StockEntry> stockEntryRepository,
        IRepository<Book> bookRepository,
        IRepository<DistributionSlip> distributionRepository,
        IRepository<ReturnSlip> returnRepository,
        IRepository<TeacherIssue> teacherIssueRepository,
        IRepository<ReferenceNumberFormat> referenceFormatRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _classSectionRepository = classSectionRepository;
        _stockEntryRepository = stockEntryRepository;
        _bookRepository = bookRepository;
        _distributionRepository = distributionRepository;
        _returnRepository = returnRepository;
        _teacherIssueRepository = teacherIssueRepository;
        _referenceFormatRepository = referenceFormatRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AcademicYearResponse>> GetAllAsync()
    {
        var items = await _repository.Query().OrderByDescending(a => a.Year).ToListAsync();
        return items.Adapt<List<AcademicYearResponse>>();
    }

    public async Task<AcademicYearResponse?> GetActiveAsync()
    {
        var entity = await _repository.Query().FirstOrDefaultAsync(a => a.IsActive);
        return entity?.Adapt<AcademicYearResponse>();
    }

    public async Task<AcademicYearResponse> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(AcademicYear), id);
        return entity.Adapt<AcademicYearResponse>();
    }

    public async Task<AcademicYearResponse> CreateAsync(CreateAcademicYearRequest request)
    {
        if (await _repository.AnyAsync(a => a.Year == request.Year))
            throw new BusinessRuleException($"Academic year {request.Year} already exists.");

        var entity = request.Adapt<AcademicYear>();
        _repository.Add(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Adapt<AcademicYearResponse>();
    }

    public async Task<AcademicYearResponse> UpdateAsync(int id, UpdateAcademicYearRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(AcademicYear), id);

        if (await _repository.AnyAsync(a => a.Year == request.Year && a.Id != id))
            throw new BusinessRuleException($"Academic year {request.Year} already exists.");

        request.Adapt(entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Adapt<AcademicYearResponse>();
    }

    public async Task ActivateAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(AcademicYear), id);

        var allYears = await _repository.GetAllAsync();
        foreach (var year in allYears)
        {
            year.IsActive = year.Id == id;
            _repository.Update(year);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(AcademicYear), id);

        var hasClassSections = await _classSectionRepository.AnyAsync(c => c.AcademicYearId == id);
        var hasStockEntries = await _stockEntryRepository.AnyAsync(s => s.AcademicYearId == id);
        var hasStockMovements = await _bookRepository.Query()
            .AnyAsync(b => b.StockMovements.Any(m => m.AcademicYearId == id));
        var hasDistributions = await _distributionRepository.AnyAsync(d => d.AcademicYearId == id);
        var hasReturns = await _returnRepository.AnyAsync(r => r.AcademicYearId == id);
        var hasTeacherIssues = await _teacherIssueRepository.AnyAsync(t => t.AcademicYearId == id);
        var hasReferenceFormats = await _referenceFormatRepository.AnyAsync(f => f.AcademicYearId == id);

        if (hasClassSections || hasStockEntries || hasStockMovements || hasDistributions ||
            hasReturns || hasTeacherIssues || hasReferenceFormats)
        {
            throw new BusinessRuleException("Cannot delete academic year because it is referenced by existing records.");
        }

        _repository.SoftDelete(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
