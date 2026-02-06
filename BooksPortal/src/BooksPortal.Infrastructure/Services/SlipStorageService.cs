using BooksPortal.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BooksPortal.Infrastructure.Services;

public class SlipStorageService : ISlipStorageService
{
    private readonly string _basePath;

    public SlipStorageService(IConfiguration configuration)
    {
        _basePath = configuration["SlipStorage:BasePath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "SlipStorage");
    }

    public async Task<string> SaveAsync(string slipType, string academicYearName, string referenceNo, byte[] content)
    {
        var fileName = SanitizeFileName(referenceNo) + ".pdf";
        var directory = Path.Combine(_basePath, slipType, SanitizeFileName(academicYearName));
        Directory.CreateDirectory(directory);

        var filePath = Path.Combine(directory, fileName);
        await File.WriteAllBytesAsync(filePath, content);
        return filePath;
    }

    public async Task<byte[]?> LoadAsync(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return null;
        return await File.ReadAllBytesAsync(filePath);
    }

    internal static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '-');
        return name;
    }
}
