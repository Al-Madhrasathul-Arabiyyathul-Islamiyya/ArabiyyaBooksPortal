namespace BooksPortal.Application.Features.MasterData.DTOs;

public class HierarchyBulkUpsertRequest
{
    public List<AcademicYearHierarchyNode> AcademicYears { get; set; } = new();
}

public class AcademicYearHierarchyNode
{
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<KeystageHierarchyNode> Keystages { get; set; } = new();
}

public class KeystageHierarchyNode
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int? SortOrder { get; set; }
    public List<GradeHierarchyNode> Grades { get; set; } = new();
}

public class GradeHierarchyNode
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int? SortOrder { get; set; }
    public List<ClassSectionHierarchyNode> Classes { get; set; } = new();
}

public class ClassSectionHierarchyNode
{
    public string Section { get; set; } = string.Empty;
}

public class HierarchyBulkUpsertResponse
{
    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int SkippedCount { get; set; }
    public int FailedCount { get; set; }
    public List<HierarchyBulkUpsertResultRow> Results { get; set; } = new();
}

public class HierarchyBulkUpsertResultRow
{
    public string Path { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string? Message { get; set; }
}
