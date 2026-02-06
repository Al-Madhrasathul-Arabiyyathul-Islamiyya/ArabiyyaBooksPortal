using BooksPortal.Application.Features.Settings.DTOs;

namespace BooksPortal.Application.Features.Settings.Interfaces;

public interface ISlipTemplateSettingService
{
    Task<List<SlipTemplateSettingResponse>> GetAllAsync(string? category = null);
    Task<SlipTemplateSettingResponse> GetByIdAsync(int id);
    Task<SlipTemplateSettingResponse> UpdateAsync(int id, UpdateSlipTemplateSettingRequest request);
    Task ResetToDefaultsAsync();
    Task<Dictionary<string, string>> GetLabelsByCategoryAsync(string category);
}
