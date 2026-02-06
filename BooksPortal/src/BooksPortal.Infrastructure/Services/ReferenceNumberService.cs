using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Domain.Entities;
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

    public async Task<string> GenerateAsync(string prefix)
    {
        var year = DateTime.UtcNow.Year;
        var key = $"{prefix}{year}";

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

        return $"{prefix}{year}{counter.LastSequence:D6}";
    }
}
