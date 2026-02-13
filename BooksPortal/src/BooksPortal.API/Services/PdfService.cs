using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.Distribution.DTOs;
using BooksPortal.Application.Features.Returns.DTOs;
using BooksPortal.Application.Features.Settings.Interfaces;
using BooksPortal.Application.Features.TeacherIssues.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BooksPortal.API.Services;

public class PdfService : IPdfService
{
    private readonly ISlipTemplateSettingService _labelService;

    private const string ThaanaFont = "Faruma";
    private const int StudentTableRows = 9;
    private const int TeacherTableRows = 3;

    private readonly byte[]? _logoPng;

    public PdfService(ISlipTemplateSettingService labelService)
    {
        _labelService = labelService;

        var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.png");
        if (File.Exists(logoPath))
            _logoPng = File.ReadAllBytes(logoPath);
    }

    public async Task<byte[]> GenerateDistributionSlipAsync(DistributionSlipResponse slip)
    {
        var common = await _labelService.GetLabelsByCategoryAsync("Common");
        var labels = await _labelService.GetLabelsByCategoryAsync("Distribution");

        var logo = _logoPng;
        return GenerateDocument(page =>
        {
            page.Content().Layers(layers =>
            {
                layers.PrimaryLayer().Row(mainRow =>
                {
                    mainRow.RelativeItem().PaddingHorizontal(10).Column(col =>
                        ComposeDistributionCopy(col, slip, common, labels, logo));

                    mainRow.ConstantItem(1).Element(ComposeCenterDivider);

                    mainRow.RelativeItem().PaddingHorizontal(10).Column(col =>
                        ComposeDistributionCopy(col, slip, common, labels, logo));
                });

                if (slip.LifecycleStatus == BooksPortal.Domain.Enums.SlipLifecycleStatus.Cancelled)
                {
                    layers.Layer().Row(row =>
                    {
                        row.RelativeItem()
                            .Rotate(-20)
                            .AlignCenter()
                            .AlignMiddle()
                            .TranslateX(-100)
                            .Text("CANCELLED")
                            .Bold()
                            .FontSize(46)
                            .FontColor(QuestPDF.Helpers.Colors.Red.Lighten2);

                        row.ConstantItem(1);

                        row.RelativeItem()
                            .Rotate(-20)
                            .AlignCenter()
                            .AlignMiddle()
                            .TranslateX(-100)
                            .Text("CANCELLED")
                            .Bold()
                            .FontSize(46)
                            .FontColor(QuestPDF.Helpers.Colors.Red.Lighten2);
                    });
                }
            });
        });
    }

    public async Task<byte[]> GenerateReturnSlipAsync(ReturnSlipResponse slip)
    {
        var common = await _labelService.GetLabelsByCategoryAsync("Common");
        var labels = await _labelService.GetLabelsByCategoryAsync("Return");

        var logo = _logoPng;
        return GenerateDocument(page =>
        {
            page.Content().Row(mainRow =>
            {
                mainRow.RelativeItem().PaddingHorizontal(10).Column(col =>
                    ComposeReturnCopy(col, slip, common, labels, logo));

                mainRow.ConstantItem(1).Element(ComposeCenterDivider);

                mainRow.RelativeItem().PaddingHorizontal(10).Column(col =>
                    ComposeReturnCopy(col, slip, common, labels, logo));
            });
        });
    }

    public async Task<byte[]> GenerateTeacherIssueSlipAsync(TeacherIssueResponse issue)
    {
        var common = await _labelService.GetLabelsByCategoryAsync("Common");
        var labels = await _labelService.GetLabelsByCategoryAsync("TeacherIssue");

        var logo = _logoPng;
        return GenerateDocument(page =>
        {
            page.Content().Layers(layers =>
            {
                layers.PrimaryLayer().Row(mainRow =>
                {
                    mainRow.RelativeItem().PaddingHorizontal(10).Column(col =>
                        ComposeTeacherIssueCopy(col, issue, common, labels, logo));

                    mainRow.ConstantItem(1).Element(ComposeCenterDivider);

                    mainRow.RelativeItem().PaddingHorizontal(10).Column(col =>
                        ComposeTeacherIssueCopy(col, issue, common, labels, logo));
                });

                if (issue.LifecycleStatus == BooksPortal.Domain.Enums.SlipLifecycleStatus.Cancelled)
                {
                    layers.Layer().Row(row =>
                    {
                        row.RelativeItem()
                            .Rotate(-20)
                            .PaddingRight(150)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text("CANCELLED")
                            .Bold()
                            .FontSize(46)
                            .FontColor(QuestPDF.Helpers.Colors.Red.Lighten2);

                        row.ConstantItem(1);

                        row.RelativeItem()
                            .Rotate(-20)
                            .PaddingRight(150)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text("CANCELLED")
                            .Bold()
                            .FontSize(46)
                            .FontColor(QuestPDF.Helpers.Colors.Red.Lighten2);
                    });
                }
            });
        });
    }

    public async Task<byte[]> GenerateTeacherReturnSlipAsync(TeacherReturnSlipResponse slip)
    {
        var common = await _labelService.GetLabelsByCategoryAsync("Common");
        var labels = await _labelService.GetLabelsByCategoryAsync("TeacherReturn");

        var logo = _logoPng;
        return GenerateDocument(page =>
        {
            page.Content().Row(mainRow =>
            {
                mainRow.RelativeItem().PaddingHorizontal(10).Column(col =>
                    ComposeTeacherReturnCopy(col, slip, common, labels, logo));

                mainRow.ConstantItem(1).Element(ComposeCenterDivider);

                mainRow.RelativeItem().PaddingHorizontal(10).Column(col =>
                    ComposeTeacherReturnCopy(col, slip, common, labels, logo));
            });
        });
    }

    private static byte[] GenerateDocument(Action<PageDescriptor> configurePage)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginVertical(10, Unit.Millimetre);
                page.MarginHorizontal(5, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                configurePage(page);
            });
        });

        return document.GeneratePdf();
    }

    // ── Per-slip-type copy composition ──────────────────────────────

    private static void ComposeDistributionCopy(
        ColumnDescriptor col,
        DistributionSlipResponse slip,
        Dictionary<string, string> common,
        Dictionary<string, string> labels,
        byte[]? logo)
    {
        col.Item().Element(c => ComposeHeader(c, common, L(labels, "Title"), logo));

        col.Item().PaddingTop(8).Element(c => ComposeRefRow(c, slip.ReferenceNo,
            L(labels, "StudentInfoTitle")));

        col.Item().PaddingTop(4).Element(c => ComposeStudentDetails(c, common,
            slip.StudentName, slip.StudentClassName, slip.StudentIndexNo, slip.StudentNationalId));

        col.Item().Element(c => ComposeStudentTable(c, labels,
            slip.Items.Select(i => new TableRow(i.BookTitle, i.BookCode, slip.AcademicYearName, "-", slip.Term.ToString())).ToList()));

        col.Item().ExtendVertical().AlignBottom()
            .PaddingTop(8).Element(c => ComposeDistributionSignatures(c, common, labels, slip));
    }

    private static void ComposeReturnCopy(
        ColumnDescriptor col,
        ReturnSlipResponse slip,
        Dictionary<string, string> common,
        Dictionary<string, string> labels,
        byte[]? logo)
    {
        col.Item().Element(c => ComposeHeader(c, common, L(labels, "Title"), logo));

        col.Item().PaddingTop(8).Element(c => ComposeRefRow(c, slip.ReferenceNo,
            L(labels, "StudentInfoTitle")));

        col.Item().PaddingTop(4).Element(c => ComposeStudentDetails(c, common,
            slip.StudentName, slip.StudentClassName, slip.StudentIndexNo, slip.StudentNationalId));

        col.Item().Element(c => ComposeStudentTable(c, labels,
            slip.Items.Select(i => new TableRow(i.BookTitle, i.BookCode, slip.AcademicYearName, "-", "-")).ToList()));

        col.Item().ExtendVertical().AlignBottom()
            .PaddingTop(8).Element(c => ComposeReturnSignatures(c, common, labels, slip));
    }

    private static void ComposeTeacherIssueCopy(
        ColumnDescriptor col,
        TeacherIssueResponse issue,
        Dictionary<string, string> common,
        Dictionary<string, string> labels,
        byte[]? logo)
    {
        col.Item().Element(c => ComposeHeader(c, common, L(labels, "Title"), logo));

        col.Item().PaddingTop(8).Element(c => ComposeRefRow(c, issue.ReferenceNo, null));

        col.Item().PaddingTop(4).Element(c => ComposeTeacherTable(c, labels,
            issue.Items.Select(i => new TableRow(i.BookTitle, i.BookCode, issue.AcademicYearName, "-", null)).ToList()));

        col.Item().ExtendVertical().AlignBottom()
            .PaddingTop(8).Element(c => ComposeTeacherIssueSignatures(c, common, labels, issue));
    }

    private static void ComposeTeacherReturnCopy(
        ColumnDescriptor col,
        TeacherReturnSlipResponse slip,
        Dictionary<string, string> common,
        Dictionary<string, string> labels,
        byte[]? logo)
    {
        col.Item().Element(c => ComposeHeader(c, common, L(labels, "Title"), logo));

        col.Item().PaddingTop(8).Element(c => ComposeRefRow(c, slip.ReferenceNo, null));

        col.Item().PaddingTop(4).Element(c => ComposeTeacherTable(c, labels,
            slip.Items.Select(i => new TableRow(i.BookTitle, i.BookCode, slip.AcademicYearName, "-", null)).ToList()));

        col.Item().ExtendVertical().AlignBottom()
            .PaddingTop(8).Element(c => ComposeTeacherReturnSignatures(c, common, labels, slip));
    }

    // ── Header ──────────────────────────────────────────────────────

    private static void ComposeHeader(IContainer container, Dictionary<string, string> common, string title, byte[]? logo = null)
    {
        container.ContentFromRightToLeft().Row(row =>
        {
            if (logo != null)
            {
                row.ConstantItem(40).Image(logo);
                row.ConstantItem(10);
            }

            row.RelativeItem().Column(col =>
            {
                col.Item().AlignRight().Text(L(common, "SchoolName"))
                    .FontFamily(ThaanaFont).FontSize(11);

                col.Item().AlignRight().Text(L(common, "SchoolSubtitle"))
                    .FontFamily(ThaanaFont).FontSize(10);

                col.Item().PaddingTop(6).AlignCenter().Text(title)
                    .FontFamily(ThaanaFont).FontSize(12).Bold();
            });
        });
    }

    // ── Ref Row ─────────────────────────────────────────────────────

    private static void ComposeRefRow(IContainer container, string refNo, string? studentInfoTitle)
    {
        container.Row(row =>
        {
            row.AutoItem().ContentFromLeftToRight().AlignLeft().Text($"Ref No: {refNo}")
                .FontSize(9);

            if (!string.IsNullOrEmpty(studentInfoTitle))
            {
                row.RelativeItem().ContentFromRightToLeft().AlignRight().Text(studentInfoTitle)
                    .FontFamily(ThaanaFont).FontSize(10).Bold();
            }
            else
            {
                row.RelativeItem();
            }
        });
    }

    // ── Student Details Grid ────────────────────────────────────────

    private static void ComposeStudentDetails(IContainer container, Dictionary<string, string> common,
        string name, string className, string indexNo, string? nationalId)
    {
        container.ContentFromRightToLeft().PaddingBottom(8).Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(1.5f);
                cols.RelativeColumn(1);
                cols.RelativeColumn(1);
                cols.RelativeColumn(1);
            });

            table.Cell().Element(c => DetailField(c, L(common, "LabelName"), name));
            table.Cell().Element(c => DetailField(c, L(common, "LabelClass"), className));
            table.Cell().Element(c => DetailField(c, L(common, "LabelIndex"), indexNo));
            table.Cell().Element(c => DetailField(c, L(common, "LabelId"), nationalId ?? ""));
        });
    }

    private static void DetailField(IContainer container, string label, string value)
    {
        container.Row(row =>
        {
            row.AutoItem().Text(label).FontFamily(ThaanaFont).FontSize(9);
            row.RelativeItem().PaddingRight(4).Column(col =>
            {
                col.Item().Text(value).FontSize(9);
                col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
            });
        });
    }

    // ── Tables ──────────────────────────────────────────────────────

    private record TableRow(string BookTitle, string SubjectCode, string AcademicYear, string Publisher, string? Term);

    private static void ComposeStudentTable(IContainer container, Dictionary<string, string> labels, List<TableRow> rows)
    {
        container.Table(table =>
        {
            // RTL order: Term(12) | Publisher(15) | AcademicYear(15) | SubjectCode(15) | BookTitle(43)
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(12);
                columns.RelativeColumn(15);
                columns.RelativeColumn(15);
                columns.RelativeColumn(15);
                columns.RelativeColumn(43);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text(L(labels, "ColTerm")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCell).Text(L(labels, "ColPublisher")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCell).Text(L(labels, "ColAcademicYear")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCell).Text(L(labels, "ColSubjectCode")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCell).Text(L(labels, "ColBookTitle")).FontFamily(ThaanaFont);
            });

            foreach (var item in rows)
            {
                table.Cell().Element(DataCell).Text(item.Term ?? "");
                table.Cell().Element(DataCell).Text(item.Publisher);
                table.Cell().Element(DataCell).Text(item.AcademicYear);
                table.Cell().Element(DataCell).Text(item.SubjectCode);
                table.Cell().Element(DataCellRight).Text(item.BookTitle);
            }

            for (var i = rows.Count; i < StudentTableRows; i++)
            {
                for (var c = 0; c < 5; c++)
                    table.Cell().Element(EmptyCell);
            }
        });
    }

    private static void ComposeTeacherTable(IContainer container, Dictionary<string, string> labels, List<TableRow> rows)
    {
        container.Table(table =>
        {
            // RTL order: Publisher(15) | AcademicYear(15) | SubjectCode(20) | BookTitle(50)
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(15);
                columns.RelativeColumn(15);
                columns.RelativeColumn(20);
                columns.RelativeColumn(50);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text(L(labels, "ColPublisher")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCell).Text(L(labels, "ColAcademicYear")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCell).Text(L(labels, "ColSubjectCode")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCell).Text(L(labels, "ColBookTitle")).FontFamily(ThaanaFont);
            });

            foreach (var item in rows)
            {
                table.Cell().Element(DataCell).Text(item.Publisher);
                table.Cell().Element(DataCell).Text(item.AcademicYear);
                table.Cell().Element(DataCell).Text(item.SubjectCode);
                table.Cell().Element(DataCellRight).Text(item.BookTitle);
            }

            for (var i = rows.Count; i < TeacherTableRows; i++)
            {
                for (var c = 0; c < 4; c++)
                    table.Cell().Element(EmptyCell);
            }
        });
    }

    // ── Signature Blocks (per-slip-type) ────────────────────────────

    private static void ComposeDistributionSignatures(
        IContainer container,
        Dictionary<string, string> common,
        Dictionary<string, string> labels,
        DistributionSlipResponse slip)
    {
        container.Row(row =>
        {
            row.RelativeItem().Element(c =>
                ComposeReceiverBlock(
                    c,
                    common,
                    L(labels, "SignatureReceiver"),
                    includeDateTime: true,
                    nameValue: slip.ParentName,
                    idCardValue: slip.ParentNationalId,
                    phoneValue: slip.ParentPhone,
                    dateValue: FormatDate(slip.IssuedAt),
                    timeValue: FormatTime(slip.IssuedAt)));
            row.ConstantItem(15);
            row.RelativeItem().Element(c =>
                ComposeStaffBlock(c, common, L(labels, "SignatureStaff"), slip.IssuedByName, slip.IssuedByDesignation));
        });
    }

    private static void ComposeReturnSignatures(
        IContainer container,
        Dictionary<string, string> common,
        Dictionary<string, string> labels,
        ReturnSlipResponse slip)
    {
        container.Row(row =>
        {
            row.RelativeItem().Element(c =>
                ComposeReceiverBlock(
                    c,
                    common,
                    L(labels, "SignatureParent"),
                    includeDateTime: true,
                    nameValue: slip.ReturnedByName,
                    idCardValue: slip.ReturnedByNationalId,
                    phoneValue: slip.ReturnedByPhone,
                    dateValue: FormatDate(slip.ReceivedAt),
                    timeValue: FormatTime(slip.ReceivedAt)));
            row.ConstantItem(15);
            row.RelativeItem().Element(c =>
                ComposeStaffBlock(c, common, L(labels, "SignatureStaff"), slip.ReceivedByName, slip.ReceivedByDesignation));
        });
    }

    private static void ComposeTeacherIssueSignatures(
        IContainer container,
        Dictionary<string, string> common,
        Dictionary<string, string> labels,
        TeacherIssueResponse issue)
    {
        container.Row(row =>
        {
            row.RelativeItem().Element(c =>
                ComposeReceiverBlock(
                    c,
                    common,
                    L(labels, "SignatureTeacher"),
                    includeDateTime: false,
                    nameValue: issue.TeacherName,
                    idCardValue: issue.TeacherNationalId));
            row.ConstantItem(15);
            row.RelativeItem().Element(c =>
                ComposeStaffBlock(c, common, L(labels, "SignatureStaff"), issue.IssuedByName, issue.IssuedByDesignation));
        });
    }

    private static void ComposeTeacherReturnSignatures(
        IContainer container,
        Dictionary<string, string> common,
        Dictionary<string, string> labels,
        TeacherReturnSlipResponse slip)
    {
        container.Row(row =>
        {
            row.RelativeItem().Element(c =>
                ComposeReceiverBlock(
                    c,
                    common,
                    L(labels, "SignatureTeacher"),
                    includeDateTime: true,
                    nameValue: slip.TeacherName,
                    idCardValue: slip.TeacherNationalId,
                    dateValue: FormatDate(slip.ReceivedAt),
                    timeValue: FormatTime(slip.ReceivedAt)));
            row.ConstantItem(15);
            row.RelativeItem().Element(c =>
                ComposeStaffBlock(c, common, L(labels, "SignatureStaff"), slip.ReceivedByName, slip.ReceivedByDesignation));
        });
    }

    private static void ComposeReceiverBlock(
        IContainer container,
        Dictionary<string, string> common,
        string title,
        bool includeDateTime,
        string? nameValue = null,
        string? idCardValue = null,
        string? phoneValue = null,
        string? dateValue = null,
        string? timeValue = null)
    {
        container.ContentFromRightToLeft().Column(col =>
        {
            col.Item().AlignRight().Text(title).FontFamily(ThaanaFont).FontSize(10).Bold();
            col.Item().PaddingTop(6).Element(c => SignatureField(c, L(common, "LabelName"), nameValue));
            col.Item().Element(c => SignatureField(c, L(common, "LabelIdCard"), idCardValue));
            col.Item().Element(c => SignatureField(c, L(common, "LabelPhone"), phoneValue));
            col.Item().Element(c => SignatureField(c, L(common, "LabelSignature")));

            if (includeDateTime)
            {
                col.Item().Row(dateTimeRow =>
                {
                    dateTimeRow.RelativeItem().Element(c => SignatureField(c, L(common, "LabelDate"), dateValue));
                    dateTimeRow.ConstantItem(10);
                    dateTimeRow.RelativeItem().Element(c => SignatureField(c, L(common, "LabelTime"), timeValue));
                });
            }
        });
    }

    private static void ComposeStaffBlock(
        IContainer container,
        Dictionary<string, string> common,
        string title,
        string? nameValue = null,
        string? positionValue = null)
    {
        container.ContentFromRightToLeft().Column(col =>
        {
            col.Item().AlignRight().Text(title).FontFamily(ThaanaFont).FontSize(10).Bold();
            col.Item().PaddingTop(6).Element(c => SignatureField(c, L(common, "LabelName"), nameValue));
            col.Item().Element(c => SignatureField(c, L(common, "LabelPosition"), positionValue));
            col.Item().Element(c => SignatureField(c, L(common, "LabelSignature")));
        });
    }

    private static void SignatureField(IContainer container, string label, string? value = null)
    {
        container.PaddingTop(6).Row(row =>
        {
            row.AutoItem().Text(label).FontFamily(ThaanaFont).FontSize(9);
            row.RelativeItem().PaddingRight(4).Column(col =>
            {
                col.Item().AlignLeft().Text(value ?? string.Empty).FontSize(8.5f);
                col.Item().PaddingBottom(1).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
            });
        });
    }

    // ── Center Divider ──────────────────────────────────────────────

    private static void ComposeCenterDivider(IContainer container)
    {
        container.AlignCenter().Width(1).Background(Colors.Grey.Lighten1);
    }

    // ── Cell styles ─────────────────────────────────────────────────

    private static IContainer HeaderCell(IContainer container) =>
        container.ContentFromRightToLeft()
            .Background(Colors.Grey.Lighten3)
            .Border(0.5f).BorderColor(Colors.Black)
            .PaddingVertical(3).PaddingHorizontal(2)
            .AlignCenter()
            .DefaultTextStyle(x => x.FontSize(8.5f).Bold());

    private static IContainer DataCell(IContainer container) =>
        container.ContentFromLeftToRight()
            .Border(0.5f).BorderColor(Colors.Black)
            .PaddingVertical(2).PaddingHorizontal(2)
            .MinHeight(12).AlignCenter()
            .DefaultTextStyle(x => x.FontSize(8.5f));

    private static IContainer DataCellRight(IContainer container) =>
        container.ContentFromLeftToRight()
            .Border(0.5f).BorderColor(Colors.Black)
            .PaddingVertical(2).PaddingHorizontal(2)
            .MinHeight(12).AlignRight()
            .DefaultTextStyle(x => x.FontSize(8.5f));

    private static IContainer EmptyCell(IContainer container) =>
        container.Border(0.5f).BorderColor(Colors.Black)
            .PaddingVertical(2).PaddingHorizontal(2)
            .MinHeight(12);

    // ── Helpers ─────────────────────────────────────────────────────

    private static string L(Dictionary<string, string> labels, string key, string fallback = "")
        => labels.TryGetValue(key, out var value) ? value : fallback;

    private static string FormatDate(DateTime value) => value.ToString("dd-MM-yyyy");
    private static string FormatTime(DateTime value) => value.ToString("HH:mm");
}
