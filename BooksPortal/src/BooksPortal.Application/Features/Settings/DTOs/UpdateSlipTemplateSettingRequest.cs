namespace BooksPortal.Application.Features.Settings.DTOs;

public class UpdateSlipTemplateSettingRequest
{
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
