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
        AddLabel(labels, "Common", "SchoolName", "އަލް މަދްރަސަތުލް ޢަރަބިއްޔަތުލް އިސްލާމިއްޔާ", ref sort);
        AddLabel(labels, "Common", "SchoolSubtitle", "މާލެ", ref sort);
        AddLabel(labels, "Common", "LabelName", "ނަން:", ref sort);
        AddLabel(labels, "Common", "LabelIdCard", "އައިޑީ ކާޑު:", ref sort);
        AddLabel(labels, "Common", "LabelPhone", "ފޯން:", ref sort);
        AddLabel(labels, "Common", "LabelSignature", "ސޮއި:", ref sort);
        AddLabel(labels, "Common", "LabelDate", "ތާރީހު:", ref sort);
        AddLabel(labels, "Common", "LabelTime", "ގަޑި:", ref sort);
        AddLabel(labels, "Common", "LabelRefNo", "Ref No:", ref sort);
        AddLabel(labels, "Common", "LabelClass", "ކުލާސް:", ref sort);
        AddLabel(labels, "Common", "LabelIndex", "އިންޑެކްސް:", ref sort);
        AddLabel(labels, "Common", "LabelId", "އައިޑީ:", ref sort);
        AddLabel(labels, "Common", "LabelPosition", "މަގާމު:", ref sort);

        // Distribution
        sort = 0;
        AddLabel(labels, "Distribution", "Title", "ފޮތް ދޫކުރާ ސުލިޕް", ref sort);
        AddLabel(labels, "Distribution", "StudentInfoTitle", "ދަރިވަރުގެ މައުލޫމާތު", ref sort);
        AddLabel(labels, "Distribution", "ColBookTitle", "ނަން", ref sort);
        AddLabel(labels, "Distribution", "ColSubjectCode", "މާއްދާ ކޯޑް", ref sort);
        AddLabel(labels, "Distribution", "ColAcademicYear", "އަހަރު", ref sort);
        AddLabel(labels, "Distribution", "ColPublisher", "ޕަބްލިޝަރ", ref sort);
        AddLabel(labels, "Distribution", "ColTerm", "ޓާރމް", ref sort);
        AddLabel(labels, "Distribution", "SignatureReceiver", "ހަވާލުވި ފަރާތް", ref sort);
        AddLabel(labels, "Distribution", "SignatureStaff", "ހަވާލުކުރި މުވައްޒަފު", ref sort);

        // Return
        sort = 0;
        AddLabel(labels, "Return", "Title", "ދަރިވަރުންގެ ފޮތް ހަވާލުކުރާ ސުލިޕް", ref sort);
        AddLabel(labels, "Return", "StudentInfoTitle", "ދަރިވަރުގެ މައުލޫމާތު", ref sort);
        AddLabel(labels, "Return", "ColBookTitle", "ނަން", ref sort);
        AddLabel(labels, "Return", "ColSubjectCode", "މާއްދާ ކޯޑް", ref sort);
        AddLabel(labels, "Return", "ColAcademicYear", "އަހަރު", ref sort);
        AddLabel(labels, "Return", "ColPublisher", "ޕަބްލިޝަރ", ref sort);
        AddLabel(labels, "Return", "ColTerm", "ޓާރމް", ref sort);
        AddLabel(labels, "Return", "SignatureParent", "ހަވާލުކުރި ފަރާތް", ref sort);
        AddLabel(labels, "Return", "SignatureStaff", "ހަވާލުވި މުވައްޒަފު", ref sort);

        // TeacherIssue
        sort = 0;
        AddLabel(labels, "TeacherIssue", "Title", "މުދައްރިސުންނަށް ފޮތްދޫކުރާ ސުލިޕް", ref sort);
        AddLabel(labels, "TeacherIssue", "ColBookTitle", "ނަން", ref sort);
        AddLabel(labels, "TeacherIssue", "ColSubjectCode", "މާއްދާ ކޯޑް", ref sort);
        AddLabel(labels, "TeacherIssue", "ColAcademicYear", "އަހަރު", ref sort);
        AddLabel(labels, "TeacherIssue", "ColPublisher", "ޕަބްލިޝަރ", ref sort);
        AddLabel(labels, "TeacherIssue", "SignatureTeacher", "ޓީޗަރު", ref sort);
        AddLabel(labels, "TeacherIssue", "SignatureStaff", "ހަވާލުކުރި މުވައްޒަފު", ref sort);

        // TeacherReturn
        sort = 0;
        AddLabel(labels, "TeacherReturn", "Title", "ޓީޗަރުން ފޮތް ހަވާލުކުރާ ސުލިޕް", ref sort);
        AddLabel(labels, "TeacherReturn", "ColBookTitle", "ނަން", ref sort);
        AddLabel(labels, "TeacherReturn", "ColSubjectCode", "މާއްދާ ކޯޑް", ref sort);
        AddLabel(labels, "TeacherReturn", "ColAcademicYear", "އަހަރު", ref sort);
        AddLabel(labels, "TeacherReturn", "ColPublisher", "ޕަބްލިޝަރ", ref sort);
        AddLabel(labels, "TeacherReturn", "SignatureTeacher", "ޓީޗަރު", ref sort);
        AddLabel(labels, "TeacherReturn", "SignatureStaff", "ހަވާލުވި މުވައްޒަފު", ref sort);

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
