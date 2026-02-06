using BooksPortal.Application.Features.Settings.DTOs;
using BooksPortal.Domain.Enums;

namespace BooksPortal.Application.Features.Settings.Interfaces;

public interface IReferenceNumberFormatService
{
    Task<List<ReferenceNumberFormatResponse>> GetAllAsync(SlipType? slipType = null, int? academicYearId = null);
    Task<ReferenceNumberFormatResponse> GetByIdAsync(int id);
    Task<ReferenceNumberFormatResponse> CreateAsync(CreateReferenceNumberFormatRequest request);
    Task<ReferenceNumberFormatResponse> UpdateAsync(int id, CreateReferenceNumberFormatRequest request);
    Task DeleteAsync(int id);
}
