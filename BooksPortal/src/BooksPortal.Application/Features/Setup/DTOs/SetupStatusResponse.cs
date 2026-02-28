using BooksPortal.Domain.Enums;

namespace BooksPortal.Application.Features.Setup.DTOs;

public class SetupStatusResponse
{
    public SetupStatus Status { get; set; }
    public bool IsReady { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? LastEvaluatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public List<SetupStepStatusResponse> Steps { get; set; } = [];
    public List<SetupReadinessIssueResponse> Issues { get; set; } = [];
}

public class SetupStepStatusResponse
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public bool IsBlocking { get; set; } = true;
    public string? Hint { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}

public class SetupReadinessIssueResponse
{
    public string Key { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Hint { get; set; }
}

public static class SetupReadinessStepKeys
{
    public const string SuperAdmin = "super-admin";
    public const string SlipTemplates = "slip-templates";
    public const string ActiveAcademicYear = "active-academic-year";
    public const string Hierarchy = "hierarchy";
    public const string ReferenceFormats = "reference-formats";
}
