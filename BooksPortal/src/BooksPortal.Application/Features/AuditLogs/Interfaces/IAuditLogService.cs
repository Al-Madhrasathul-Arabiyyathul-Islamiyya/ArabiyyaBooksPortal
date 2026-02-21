using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.AuditLogs.DTOs;

namespace BooksPortal.Application.Features.AuditLogs.Interfaces;

public interface IAuditLogService
{
    Task<PaginatedList<AuditLogResponse>> GetPagedAsync(
        int pageNumber, int pageSize,
        string? entityType = null, string? action = null,
        int? userId = null, DateTime? from = null, DateTime? to = null);
}
