using BooksPortal.Application.Features.BulkImport.DTOs;

namespace BooksPortal.Application.Features.BulkImport.Interfaces;

public interface IStudentBulkImportService
{
    Task<BulkImportReport> ValidateAsync(Stream stream, CancellationToken cancellationToken = default);
    Task<BulkImportReport> CommitAsync(
        Stream stream,
        IProgress<int>? processedRowsProgress = null,
        CancellationToken cancellationToken = default);
}
