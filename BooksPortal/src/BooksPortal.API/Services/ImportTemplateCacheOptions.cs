namespace BooksPortal.API.Services;

public sealed class ImportTemplateCacheOptions
{
    public const string SectionName = "ImportTemplateCache";
    public string StoragePath { get; set; } = "App_Data/import-templates";
}
