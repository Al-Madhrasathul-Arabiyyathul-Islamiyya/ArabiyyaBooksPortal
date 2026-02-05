namespace BooksPortal.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<string> Roles { get; }
}
