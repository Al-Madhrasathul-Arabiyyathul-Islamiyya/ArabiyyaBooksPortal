namespace BooksPortal.Application.Features.BulkImport.DTOs;

public class BulkImportReport
{
    public string Entity { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public int InsertedRows { get; set; }
    public int FailedRows { get; set; }
    public bool CanCommit { get; set; }
    public List<BulkImportRowResult> Rows { get; set; } = new();
    public List<BulkImportRowIssue> Issues { get; set; } = new();
}

public class BulkImportRowResult
{
    public int RowNumber { get; set; }
    public string Key { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Note { get; set; }
}

public class BulkImportRowIssue
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
