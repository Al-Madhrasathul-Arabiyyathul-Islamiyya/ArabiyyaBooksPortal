using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.Distribution.DTOs;

namespace BooksPortal.Application.Features.Distribution.Interfaces;

public interface IDistributionService
{
    Task<PaginatedList<DistributionSlipResponse>> GetPagedAsync(int pageNumber, int pageSize, int? academicYearId = null, int? studentId = null);
    Task<DistributionSlipResponse> GetByIdAsync(int id);
    Task<DistributionSlipResponse> GetByReferenceAsync(string referenceNo);
    Task<DistributionSlipResponse> CreateAsync(CreateDistributionSlipRequest request, int userId);
    Task CancelAsync(int id);
}
