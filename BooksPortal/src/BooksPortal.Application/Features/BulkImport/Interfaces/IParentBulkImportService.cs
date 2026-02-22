using BooksPortal.Application.Features.BulkImport.DTOs;

namespace BooksPortal.Application.Features.BulkImport.Interfaces;

public interface IParentBulkImportService
{
    Task<BulkImportReport> ValidateAsync(Stream stream, CancellationToken cancellationToken = default);
    Task<BulkImportReport> CommitAsync(Stream stream, CancellationToken cancellationToken = default);
}
