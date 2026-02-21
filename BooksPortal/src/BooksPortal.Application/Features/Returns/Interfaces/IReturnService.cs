using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.Returns.DTOs;

namespace BooksPortal.Application.Features.Returns.Interfaces;

public interface IReturnService
{
    Task<PaginatedList<ReturnSlipResponse>> GetPagedAsync(int pageNumber, int pageSize, int? academicYearId = null, int? studentId = null, bool includeCancelled = false);
    Task<ReturnSlipResponse> GetByIdAsync(int id);
    Task<ReturnSlipResponse> GetByReferenceAsync(string referenceNo);
    Task<ReturnSlipResponse> CreateAsync(CreateReturnSlipRequest request, int userId);
    Task FinalizeAsync(int id, int userId);
    Task CancelAsync(int id, int userId);
}
