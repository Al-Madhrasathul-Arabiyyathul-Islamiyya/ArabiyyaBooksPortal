namespace BooksPortal.Application.Common.Interfaces;

public interface IReferenceNumberService
{
    Task<string> GenerateAsync(string prefix);
}
