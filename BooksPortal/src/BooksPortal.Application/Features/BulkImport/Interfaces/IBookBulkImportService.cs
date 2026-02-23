using BooksPortal.Application.Features.BulkImport.DTOs;

namespace BooksPortal.Application.Features.BulkImport.Interfaces;

public interface IBookBulkImportService
{
    Task<BulkImportReport> ValidateAsync(Stream stream, CancellationToken cancellationToken = default);
    Task<BulkImportReport> CommitAsync(
        Stream stream,
        int userId,
        IProgress<int>? processedRowsProgress = null,
        CancellationToken cancellationToken = default);
}
