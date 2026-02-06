namespace BooksPortal.Application.Features.Settings.DTOs;

public class SlipTemplateSettingResponse
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
