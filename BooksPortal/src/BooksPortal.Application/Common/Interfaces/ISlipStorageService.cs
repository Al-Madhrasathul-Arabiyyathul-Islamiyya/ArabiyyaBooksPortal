namespace BooksPortal.Application.Common.Interfaces;

public interface ISlipStorageService
{
    Task<string> SaveAsync(string slipType, string academicYearName, string referenceNo, byte[] content);
    Task<byte[]?> LoadAsync(string? filePath);
}
