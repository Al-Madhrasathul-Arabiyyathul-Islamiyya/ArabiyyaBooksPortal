using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.AuditLogs.DTOs;
using BooksPortal.Application.Features.AuditLogs.Interfaces;
using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.AuditLogs.Services;

public class AuditLogService : IAuditLogService
{
    private readonly DbContext _context;

    public AuditLogService(DbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AuditLogResponse>> GetPagedAsync(
        int pageNumber, int pageSize,
        string? entityType = null, string? action = null,
        int? userId = null, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Set<AuditLog>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        var projected = query.OrderByDescending(a => a.Timestamp).Select(a => new AuditLogResponse
        {
            Id = a.Id,
            Action = a.Action,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            OldValues = a.OldValues,
            NewValues = a.NewValues,
            UserId = a.UserId,
            UserName = a.UserName,
            Timestamp = a.Timestamp
        });

        return await PaginatedList<AuditLogResponse>.CreateAsync(projected, pageNumber, pageSize);
    }
}
