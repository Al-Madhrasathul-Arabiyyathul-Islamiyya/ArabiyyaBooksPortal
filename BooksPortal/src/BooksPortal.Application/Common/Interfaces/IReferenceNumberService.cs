using BooksPortal.Domain.Enums;

namespace BooksPortal.Application.Common.Interfaces;

public interface IReferenceNumberService
{
    Task<string> GenerateAsync(SlipType slipType, int academicYearId);
}
