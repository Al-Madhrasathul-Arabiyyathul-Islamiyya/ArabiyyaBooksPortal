namespace BooksPortal.Application.Features.MasterData.DTOs;

public class GradeResponse
{
    public int Id { get; set; }
    public int KeystageId { get; set; }
    public string KeystageName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
