using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.Settings.DTOs;
using BooksPortal.Application.Features.Settings.Interfaces;
using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.Settings.Services;

public class SlipTemplateSettingService : ISlipTemplateSettingService
{
    private readonly IRepository<SlipTemplateSetting> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SlipTemplateSettingService(IRepository<SlipTemplateSetting> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<SlipTemplateSettingResponse>> GetAllAsync(string? category = null)
    {
        var query = _repository.Query().AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(s => s.Category == category);

        var items = await query.OrderBy(s => s.Category).ThenBy(s => s.SortOrder).ToListAsync();
        return items.Select(MapToResponse).ToList();
    }

    public async Task<SlipTemplateSettingResponse> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(SlipTemplateSetting), id);
        return MapToResponse(entity);
    }

    public async Task<SlipTemplateSettingResponse> UpdateAsync(int id, UpdateSlipTemplateSettingRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(SlipTemplateSetting), id);

        entity.Value = request.Value;
        entity.SortOrder = request.SortOrder;

        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(entity);
    }

    public async Task ResetToDefaultsAsync()
    {
        var existing = await _repository.GetAllAsync();
        foreach (var item in existing)
            _repository.SoftDelete(item);

        foreach (var setting in GetDefaultLabels())
            _repository.Add(setting);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Dictionary<string, string>> GetLabelsByCategoryAsync(string category)
    {
        var items = await _repository.Query()
            .Where(s => s.Category == category)
            .ToListAsync();

        return items.ToDictionary(s => s.Key, s => s.Value);
    }

    public static List<SlipTemplateSetting> GetDefaultLabels()
    {
        var labels = new List<SlipTemplateSetting>();
        var sort = 0;

        // Common labels shared across slip types
        AddLabel(labels, "Common", "SchoolName", "މާލެ އިންޓަނޭޝަނަލް ސްކੂލް", ref sort);
        AddLabel(labels, "Common", "SchoolSubtitle", "ފޮތް ބެހެއްٽެރި ޕੋرਟَل", ref sort);
        AddLabel(labels, "Common", "SignatureIssuedBy", "ދੂکری ފަރާތް", ref sort);
        AddLabel(labels, "Common", "SignatureReceivedBy", "ލިބ였 ފަرާتް", ref sort);
        AddLabel(labels, "Common", "LabelName", "ނަން:", ref sort);
        AddLabel(labels, "Common", "LabelIdNo", "އައي.ডي ނަންބަر:", ref sort);
        AddLabel(labels, "Common", "LabelPhone", "ފੋނ:", ref sort);
        AddLabel(labels, "Common", "LabelSignature", "ސައي:", ref sort);
        AddLabel(labels, "Common", "LabelDate", "ތާرީخ:", ref sort);
        AddLabel(labels, "Common", "LabelTime", "ގަޑi:", ref sort);
        AddLabel(labels, "Common", "LabelRefNo", "Ref No:", ref sort);

        // Distribution
        sort = 0;
        AddLabel(labels, "Distribution", "Title", "ދަރިވަرުންނަށް ފੋތް ބੈ ސްލިޕ", ref sort);
        AddLabel(labels, "Distribution", "ColTerm", "ٹَرމ", ref sort);
        AddLabel(labels, "Distribution", "ColPublisher", "ޕަބްلިޝަر", ref sort);
        AddLabel(labels, "Distribution", "ColAcademicYear", "އެکَޑَمިک އަހَر", ref sort);
        AddLabel(labels, "Distribution", "ColSubjectCode", "ساބްجެکٹ کوڈ", ref sort);
        AddLabel(labels, "Distribution", "ColBookTitle", "ފੋتް ނަން", ref sort);

        // Return
        sort = 0;
        AddLabel(labels, "Return", "Title", "ދَرiވَرuންގެ ފੋتް އަނބuރާ ސްلiپ", ref sort);
        AddLabel(labels, "Return", "ColCondition", "ހާلَތu", ref sort);
        AddLabel(labels, "Return", "ColBookTitle", "ފੋتް ނަން", ref sort);
        AddLabel(labels, "Return", "ColSubjectCode", "ساބްجެکٹ کوڈ", ref sort);
        AddLabel(labels, "Return", "ColQuantity", "އަދَދu", ref sort);

        // TeacherIssue
        sort = 0;
        AddLabel(labels, "TeacherIssue", "Title", "ٹީچَرަށް ފੋتް ދiނ ސްلiپ", ref sort);
        AddLabel(labels, "TeacherIssue", "ColBookTitle", "ފੋتް ނަން", ref sort);
        AddLabel(labels, "TeacherIssue", "ColSubjectCode", "ساބްجެکٹ کوڈ", ref sort);
        AddLabel(labels, "TeacherIssue", "ColQuantity", "އަދَدu", ref sort);
        AddLabel(labels, "TeacherIssue", "ColAcademicYear", "އެکَޑَمiک އَހَر", ref sort);

        // TeacherReturn
        sort = 0;
        AddLabel(labels, "TeacherReturn", "Title", "ٹީچَرގެ ފੋتް އަނبuރާ ސްلiޕ", ref sort);
        AddLabel(labels, "TeacherReturn", "ColBookTitle", "ފੋتް ނަން", ref sort);
        AddLabel(labels, "TeacherReturn", "ColSubjectCode", "ساބްجެکٹ ކوޑ", ref sort);
        AddLabel(labels, "TeacherReturn", "ColQuantity", "އަދَދu", ref sort);

        return labels;
    }

    private static void AddLabel(List<SlipTemplateSetting> labels, string category, string key, string value, ref int sort)
    {
        labels.Add(new SlipTemplateSetting
        {
            Category = category,
            Key = key,
            Value = value,
            SortOrder = sort++
        });
    }

    private static SlipTemplateSettingResponse MapToResponse(SlipTemplateSetting entity) => new()
    {
        Id = entity.Id,
        Category = entity.Category,
        Key = entity.Key,
        Value = entity.Value,
        SortOrder = entity.SortOrder
    };
}
