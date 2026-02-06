using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Domain.Entities;
using BooksPortal.Domain.Enums;
using BooksPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Infrastructure.Services;

public class ReferenceNumberService : IReferenceNumberService
{
    private readonly BooksPortalDbContext _context;

    public ReferenceNumberService(BooksPortalDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateAsync(SlipType slipType, int academicYearId)
    {
        var format = await _context.ReferenceNumberFormats
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.SlipType == slipType && f.AcademicYearId == academicYearId);

        var academicYear = await _context.AcademicYears
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == academicYearId);

        var yearValue = academicYear?.Year.ToString() ?? DateTime.UtcNow.Year.ToString();

        string template;
        int paddingWidth;

        if (format != null)
        {
            template = format.FormatTemplate;
            paddingWidth = format.PaddingWidth;
        }
        else
        {
            var prefix = GetDefaultPrefix(slipType);
            template = $"{prefix}{{year}}{{autonum}}";
            paddingWidth = 6;
        }

        var counterKey = $"{slipType}_{academicYearId}";
        var sequence = await GetNextSequenceAsync(counterKey);

        return ApplyTemplate(template, yearValue, sequence, paddingWidth);
    }

    private async Task<int> GetNextSequenceAsync(string key)
    {
        var counter = await _context.ReferenceCounters
            .FromSqlRaw("SELECT * FROM ReferenceCounters WITH (UPDLOCK, ROWLOCK) WHERE [Key] = {0}", key)
            .FirstOrDefaultAsync();

        if (counter == null)
        {
            counter = new ReferenceCounter { Key = key, LastSequence = 1 };
            _context.ReferenceCounters.Add(counter);
        }
        else
        {
            counter.LastSequence++;
            _context.ReferenceCounters.Update(counter);
        }

        await _context.SaveChangesAsync();
        return counter.LastSequence;
    }

    internal static string ApplyTemplate(string template, string year, int sequence, int paddingWidth)
    {
        var result = template
            .Replace("{year}", year, StringComparison.OrdinalIgnoreCase)
            .Replace("{autonum}", sequence.ToString().PadLeft(paddingWidth, '0'), StringComparison.OrdinalIgnoreCase);

        return result;
    }

    private static string GetDefaultPrefix(SlipType slipType) => slipType switch
    {
        SlipType.Distribution => "DST",
        SlipType.Return => "RTN",
        SlipType.TeacherIssue => "TIS",
        SlipType.TeacherReturn => "TRT",
        _ => "REF"
    };
}
