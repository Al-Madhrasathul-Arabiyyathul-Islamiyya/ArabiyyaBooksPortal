using BooksPortal.Application.Features.BulkImport.Interfaces;
using BooksPortal.Application.Features.MasterData.DTOs;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BooksPortal.API.Services;

public sealed class ImportTemplateCacheService
{
    private const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string JsonContentType = "application/json";

    private readonly IImportTemplateService _templateService;
    private readonly IWebHostEnvironment _environment;
    private readonly ImportTemplateCacheOptions _options;

    private static readonly IReadOnlyDictionary<string, CachedTemplate> Templates =
        new Dictionary<string, CachedTemplate>(StringComparer.OrdinalIgnoreCase)
        {
            ["books"] = new("books-import-template.xlsx", ContentType, service => service.CreateBooksTemplate()),
            ["teachers"] = new("teachers-import-template.xlsx", ContentType, service => service.CreateTeachersTemplate()),
            ["students"] = new("students-import-template.xlsx", ContentType, service => service.CreateStudentsTemplate()),
            ["parents"] = new("parents-import-template.xlsx", ContentType, service => service.CreateParentsTemplate()),
            ["master-data-hierarchy"] = new("master-data-hierarchy-template.json", JsonContentType, _ => CreateMasterDataHierarchyTemplate())
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
            foreach (var existingFile in Directory.GetFiles(storagePath, "*.*", SearchOption.TopDirectoryOnly))
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
        return (bytes, template.ContentType, template.FileName);
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

    private static byte[] CreateMasterDataHierarchyTemplate()
    {
        var sample = new HierarchyBulkUpsertRequest
        {
            AcademicYears =
            [
                new AcademicYearHierarchyNode
                {
                    Name = "AY 2026",
                    Year = 2026,
                    IsActive = true,
                    StartDate = new DateTime(2026, 1, 1),
                    EndDate = new DateTime(2026, 12, 31),
                    Keystages =
                    [
                        new KeystageHierarchyNode
                        {
                            Name = "Key Stage 1",
                            Code = "KS1",
                            SortOrder = 1,
                            Grades =
                            [
                                new GradeHierarchyNode
                                {
                                    Name = "Grade 1",
                                    Code = "G1",
                                    SortOrder = 1,
                                    Classes =
                                    [
                                        new ClassSectionHierarchyNode { Section = "A" },
                                        new ClassSectionHierarchyNode { Section = "B" }
                                    ]
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        var json = JsonSerializer.Serialize(sample, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    private sealed record CachedTemplate(string FileName, string ContentType, Func<IImportTemplateService, byte[]> Factory);
}
