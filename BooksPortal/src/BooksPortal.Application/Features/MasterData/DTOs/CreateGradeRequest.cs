namespace BooksPortal.Application.Features.MasterData.DTOs;

public class CreateGradeRequest
{
    public int KeystageId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
