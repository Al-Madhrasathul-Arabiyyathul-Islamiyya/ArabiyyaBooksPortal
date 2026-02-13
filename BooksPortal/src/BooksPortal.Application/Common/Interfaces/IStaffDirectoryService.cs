namespace BooksPortal.Application.Common.Interfaces;

public record StaffDirectoryEntry(
    int Id,
    string DisplayName,
    string? Designation);

public interface IStaffDirectoryService
{
    Task<StaffDirectoryEntry?> GetByIdAsync(int id);
    Task<Dictionary<int, StaffDirectoryEntry>> GetByIdsAsync(IEnumerable<int> ids);
}
