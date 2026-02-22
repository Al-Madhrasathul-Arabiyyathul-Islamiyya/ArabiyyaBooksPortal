using BooksPortal.Application.Features.BulkImport.Interfaces;
using Microsoft.Extensions.Options;

namespace BooksPortal.API.Services;

public sealed class ImportTemplateCacheService
{
    private const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IImportTemplateService _templateService;
    private readonly IWebHostEnvironment _environment;
    private readonly ImportTemplateCacheOptions _options;

    private static readonly IReadOnlyDictionary<string, CachedTemplate> Templates =
        new Dictionary<string, CachedTemplate>(StringComparer.OrdinalIgnoreCase)
        {
            ["books"] = new("books-import-template.xlsx", service => service.CreateBooksTemplate()),
            ["teachers"] = new("teachers-import-template.xlsx", service => service.CreateTeachersTemplate()),
            ["students"] = new("students-import-template.xlsx", service => service.CreateStudentsTemplate()),
            ["parents"] = new("parents-import-template.xlsx", service => service.CreateParentsTemplate())
        };

    public ImportTemplateCacheService(
        IImportTemplateService templateService,
        IWebHostEnvironment environment,
        IOptions<ImportTemplateCacheOptions> options)
    {
        _templateService = templateService;
        _environment = environment;
        _options = options.Value;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var storagePath = ResolveStoragePath();

        if (Directory.Exists(storagePath))
        {
            foreach (var existingFile in Directory.GetFiles(storagePath, "*.xlsx", SearchOption.TopDirectoryOnly))
                File.Delete(existingFile);
        }

        Directory.CreateDirectory(storagePath);

        foreach (var template in Templates.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytes = template.Factory(_templateService);
            var targetPath = Path.Combine(storagePath, template.FileName);
            await File.WriteAllBytesAsync(targetPath, bytes, cancellationToken);
        }
    }

    public async Task<(byte[] Bytes, string ContentType, string FileName)?> GetTemplateAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!Templates.TryGetValue(key, out var template))
            return null;

        var path = Path.Combine(ResolveStoragePath(), template.FileName);
        if (!File.Exists(path))
            return null;

        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
        return (bytes, ContentType, template.FileName);
    }

    private string ResolveStoragePath()
    {
        var configured = string.IsNullOrWhiteSpace(_options.StoragePath)
            ? "App_Data/import-templates"
            : _options.StoragePath;

        if (Path.IsPathRooted(configured))
            return configured;

        return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, configured));
    }

    private sealed record CachedTemplate(string FileName, Func<IImportTemplateService, byte[]> Factory);
}
