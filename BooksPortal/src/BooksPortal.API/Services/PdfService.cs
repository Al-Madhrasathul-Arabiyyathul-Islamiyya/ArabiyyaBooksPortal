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

    public PdfService(ISlipTemplateSettingService labelService)
    {
        _labelService = labelService;
    }

    public async Task<byte[]> GenerateDistributionSlipAsync(DistributionSlipResponse slip)
    {
        var common = await _labelService.GetLabelsByCategoryAsync("Common");
        var labels = await _labelService.GetLabelsByCategoryAsync("Distribution");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginVertical(12);
                page.MarginHorizontal(15);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Content().Row(mainRow =>
                {
                    mainRow.RelativeItem().Element(c =>
                        ComposeDistributionCopy(c, slip, common, labels));

                    mainRow.ConstantItem(16).Element(ComposeSeparator);

                    mainRow.RelativeItem().Element(c =>
                        ComposeDistributionCopy(c, slip, common, labels));
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateReturnSlipAsync(ReturnSlipResponse slip)
    {
        var common = await _labelService.GetLabelsByCategoryAsync("Common");
        var labels = await _labelService.GetLabelsByCategoryAsync("Return");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginVertical(12);
                page.MarginHorizontal(15);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Content().Row(mainRow =>
                {
                    mainRow.RelativeItem().Element(c =>
                        ComposeReturnCopy(c, slip, common, labels));

                    mainRow.ConstantItem(16).Element(ComposeSeparator);

                    mainRow.RelativeItem().Element(c =>
                        ComposeReturnCopy(c, slip, common, labels));
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateTeacherIssueSlipAsync(TeacherIssueResponse issue)
    {
        var common = await _labelService.GetLabelsByCategoryAsync("Common");
        var labels = await _labelService.GetLabelsByCategoryAsync("TeacherIssue");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginVertical(12);
                page.MarginHorizontal(15);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Content().Row(mainRow =>
                {
                    mainRow.RelativeItem().Element(c =>
                        ComposeTeacherIssueCopy(c, issue, common, labels));

                    mainRow.ConstantItem(16).Element(ComposeSeparator);

                    mainRow.RelativeItem().Element(c =>
                        ComposeTeacherIssueCopy(c, issue, common, labels));
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateTeacherReturnSlipAsync(TeacherReturnSlipResponse slip)
    {
        var common = await _labelService.GetLabelsByCategoryAsync("Common");
        var labels = await _labelService.GetLabelsByCategoryAsync("TeacherReturn");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginVertical(12);
                page.MarginHorizontal(15);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Content().Row(mainRow =>
                {
                    mainRow.RelativeItem().Element(c =>
                        ComposeTeacherReturnCopy(c, slip, common, labels));

                    mainRow.ConstantItem(16).Element(ComposeSeparator);

                    mainRow.RelativeItem().Element(c =>
                        ComposeTeacherReturnCopy(c, slip, common, labels));
                });
            });
        });

        return document.GeneratePdf();
    }

    // ── Per-slip-type copy composition ──────────────────────────────

    private static void ComposeDistributionCopy(
        IContainer container,
        DistributionSlipResponse slip,
        Dictionary<string, string> common,
        Dictionary<string, string> labels)
    {
        container.Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(8).Column(col =>
        {
            col.Spacing(6);

            col.Item().Element(c => ComposeHeader(c, common, L(labels, "Title")));

            col.Item().Element(c => ComposeRefAndDate(c, slip.ReferenceNo, slip.IssuedAt, common));

            // Info row: Name, IndexNo, AcademicYear, Term
            col.Item().ContentFromRightToLeft().Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span(L(common, "LabelName")).FontFamily(ThaanaFont).FontSize(8);
                    text.Span($" {slip.StudentName}").FontSize(8);
                });
                row.RelativeItem().Text(text =>
                {
                    text.Span(L(common, "LabelIdNo")).FontFamily(ThaanaFont).FontSize(8);
                    text.Span($" {slip.StudentIndexNo}").FontSize(8);
                });
            });

            // Distribution table
            col.Item().Element(c => ComposeDistributionTable(c, slip, labels));

            col.Item().Element(c => ComposeSignatures(c, common,
                L(common, "SignatureIssuedBy"), L(common, "SignatureReceivedBy")));
        });
    }

    private static void ComposeReturnCopy(
        IContainer container,
        ReturnSlipResponse slip,
        Dictionary<string, string> common,
        Dictionary<string, string> labels)
    {
        container.Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(8).Column(col =>
        {
            col.Spacing(6);

            col.Item().Element(c => ComposeHeader(c, common, L(labels, "Title")));

            col.Item().Element(c => ComposeRefAndDate(c, slip.ReferenceNo, slip.ReceivedAt, common));

            col.Item().ContentFromRightToLeft().Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span(L(common, "LabelName")).FontFamily(ThaanaFont).FontSize(8);
                    text.Span($" {slip.StudentName}").FontSize(8);
                });
                row.RelativeItem().Text(text =>
                {
                    text.Span(L(common, "LabelIdNo")).FontFamily(ThaanaFont).FontSize(8);
                    text.Span($" {slip.StudentIndexNo}").FontSize(8);
                });
            });

            col.Item().Element(c => ComposeReturnTable(c, slip, labels));

            col.Item().Element(c => ComposeSignatures(c, common,
                L(common, "SignatureIssuedBy"), L(common, "SignatureReceivedBy")));
        });
    }

    private static void ComposeTeacherIssueCopy(
        IContainer container,
        TeacherIssueResponse issue,
        Dictionary<string, string> common,
        Dictionary<string, string> labels)
    {
        container.Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(8).Column(col =>
        {
            col.Spacing(6);

            col.Item().Element(c => ComposeHeader(c, common, L(labels, "Title")));

            col.Item().Element(c => ComposeRefAndDate(c, issue.ReferenceNo, issue.IssuedAt, common));

            col.Item().ContentFromRightToLeft().Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span(L(common, "LabelName")).FontFamily(ThaanaFont).FontSize(8);
                    text.Span($" {issue.TeacherName}").FontSize(8);
                });
            });

            col.Item().Element(c => ComposeTeacherIssueTable(c, issue, labels));

            col.Item().Element(c => ComposeSignatures(c, common,
                L(common, "SignatureIssuedBy"), L(common, "SignatureReceivedBy")));
        });
    }

    private static void ComposeTeacherReturnCopy(
        IContainer container,
        TeacherReturnSlipResponse slip,
        Dictionary<string, string> common,
        Dictionary<string, string> labels)
    {
        container.Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(8).Column(col =>
        {
            col.Spacing(6);

            col.Item().Element(c => ComposeHeader(c, common, L(labels, "Title")));

            col.Item().Element(c => ComposeRefAndDate(c, slip.ReferenceNo, slip.ReceivedAt, common));

            col.Item().ContentFromRightToLeft().Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span(L(common, "LabelName")).FontFamily(ThaanaFont).FontSize(8);
                    text.Span($" {slip.TeacherName}").FontSize(8);
                });
            });

            col.Item().Element(c => ComposeTeacherReturnTable(c, slip, labels));

            col.Item().Element(c => ComposeSignatures(c, common,
                L(common, "SignatureIssuedBy"), L(common, "SignatureReceivedBy")));
        });
    }

    // ── Shared layout components ────────────────────────────────────

    private static void ComposeHeader(IContainer container, Dictionary<string, string> common, string title)
    {
        container.Column(col =>
        {
            // School name and subtitle (RTL, centered)
            col.Item().ContentFromRightToLeft().AlignCenter().Text(text =>
            {
                text.Span(L(common, "SchoolName")).FontFamily(ThaanaFont).FontSize(13).Bold();
            });
            col.Item().ContentFromRightToLeft().AlignCenter().Text(text =>
            {
                text.Span(L(common, "SchoolSubtitle")).FontFamily(ThaanaFont).FontSize(9);
            });

            col.Item().PaddingVertical(4).LineHorizontal(1);

            // Slip title
            col.Item().ContentFromRightToLeft().AlignCenter().Text(text =>
            {
                text.Span(title).FontFamily(ThaanaFont).FontSize(11).Bold();
            });
        });
    }

    private static void ComposeRefAndDate(IContainer container, string refNo, DateTime dateTime, Dictionary<string, string> common)
    {
        container.Row(row =>
        {
            // Ref No (English, left side)
            row.RelativeItem().Text(text =>
            {
                text.Span($"{L(common, "LabelRefNo", "Ref No:")} ").FontSize(9);
                text.Span(refNo).FontSize(9).Bold();
            });

            // Date (right side, RTL)
            row.RelativeItem().ContentFromRightToLeft().AlignRight().Text(text =>
            {
                text.Span(L(common, "LabelDate")).FontFamily(ThaanaFont).FontSize(8);
                text.Span($" {dateTime:dd/MM/yyyy}  ").FontSize(8);
                text.Span(L(common, "LabelTime")).FontFamily(ThaanaFont).FontSize(8);
                text.Span($" {dateTime:HH:mm}").FontSize(8);
            });
        });
    }

    private static void ComposeSeparator(IContainer container)
    {
        container.AlignCenter().PaddingVertical(15).Column(col =>
        {
            col.Item().Width(1).ExtendVertical().Background(Colors.Grey.Lighten1);
        });
    }

    // ── Tables ──────────────────────────────────────────────────────

    private static void ComposeDistributionTable(IContainer container, DistributionSlipResponse slip, Dictionary<string, string> labels)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);   // Book Title (widest)
                columns.RelativeColumn(1.2f); // Subject Code
                columns.RelativeColumn(1.2f); // Academic Year
                columns.RelativeColumn(1.2f); // Publisher
                columns.RelativeColumn(1);    // Term
            });

            // Thaana headers (RTL order: rightmost column first in visual, but table is LTR, so we reverse)
            table.Header(header =>
            {
                header.Cell().Element(HeaderCellRtl).Text(L(labels, "ColBookTitle")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCellRtl).Text(L(labels, "ColSubjectCode")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCellRtl).Text(L(labels, "ColAcademicYear")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCellRtl).Text(L(labels, "ColPublisher")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCellRtl).Text(L(labels, "ColTerm")).FontFamily(ThaanaFont);
            });

            foreach (var item in slip.Items)
            {
                table.Cell().Element(DataCellLtr).Text(item.BookTitle);
                table.Cell().Element(DataCellLtr).Text(item.BookCode);
                table.Cell().Element(DataCellLtr).Text(slip.AcademicYearName);
                table.Cell().Element(DataCellLtr).Text("-"); // Publisher not in DTO
                table.Cell().Element(DataCellLtr).Text(slip.Term.ToString());
            }

            // Empty rows to fill the table area (match the design's grid)
            for (var i = slip.Items.Count; i < 12; i++)
            {
                table.Cell().Element(EmptyCell);
                table.Cell().Element(EmptyCell);
                table.Cell().Element(EmptyCell);
                table.Cell().Element(EmptyCell);
                table.Cell().Element(EmptyCell);
            }
        });
    }

    private static void ComposeReturnTable(IContainer container, ReturnSlipResponse slip, Dictionary<string, string> labels)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);   // Book Title
                columns.RelativeColumn(1.2f); // Subject Code
                columns.RelativeColumn(1);    // Quantity
                columns.RelativeColumn(1.2f); // Condition
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCellRtl).Text(L(labels, "ColBookTitle")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCellRtl).Text(L(labels, "ColSubjectCode")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCellRtl).Text(L(labels, "ColQuantity")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCellRtl).Text(L(labels, "ColCondition")).FontFamily(ThaanaFont);
            });

            foreach (var item in slip.Items)
            {
                table.Cell().Element(DataCellLtr).Text(item.BookTitle);
                table.Cell().Element(DataCellLtr).Text(item.BookCode);
                table.Cell().Element(DataCellLtr).AlignCenter().Text($"{item.Quantity}");
                table.Cell().Element(DataCellLtr).Text(item.Condition.ToString());
            }

            for (var i = slip.Items.Count; i < 12; i++)
            {
                table.Cell().Element(EmptyCell);
                table.Cell().Element(EmptyCell);
                table.Cell().Element(EmptyCell);
                table.Cell().Element(EmptyCell);
            }
        });
    }

    private static void ComposeTeacherIssueTable(IContainer container, TeacherIssueResponse issue, Dictionary<string, string> labels)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);   // Book Title
                columns.RelativeColumn(1.2f); // Subject Code
                columns.RelativeColumn(1.2f); // Academic Year
                columns.RelativeColumn(1);    // Quantity
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCellRtl).Text(L(labels, "ColBookTitle")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCellRtl).Text(L(labels, "ColSubjectCode")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCellRtl).Text(L(labels, "ColAcademicYear")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCellRtl).Text(L(labels, "ColQuantity")).FontFamily(ThaanaFont);
            });

            foreach (var item in issue.Items)
            {
                table.Cell().Element(DataCellLtr).Text(item.BookTitle);
                table.Cell().Element(DataCellLtr).Text(item.BookCode);
                table.Cell().Element(DataCellLtr).Text(issue.AcademicYearName);
                table.Cell().Element(DataCellLtr).AlignCenter().Text($"{item.Quantity}");
            }

            for (var i = issue.Items.Count; i < 12; i++)
            {
                table.Cell().Element(EmptyCell);
                table.Cell().Element(EmptyCell);
                table.Cell().Element(EmptyCell);
                table.Cell().Element(EmptyCell);
            }
        });
    }

    private static void ComposeTeacherReturnTable(IContainer container, TeacherReturnSlipResponse slip, Dictionary<string, string> labels)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);   // Book Title
                columns.RelativeColumn(1.2f); // Subject Code
                columns.RelativeColumn(1);    // Quantity
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCellRtl).Text(L(labels, "ColBookTitle")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCellRtl).Text(L(labels, "ColSubjectCode")).FontFamily(ThaanaFont);
                header.Cell().Element(HeaderCellRtl).Text(L(labels, "ColQuantity")).FontFamily(ThaanaFont);
            });

            foreach (var item in slip.Items)
            {
                table.Cell().Element(DataCellLtr).Text(item.BookTitle);
                table.Cell().Element(DataCellLtr).Text(item.BookCode);
                table.Cell().Element(DataCellLtr).AlignCenter().Text($"{item.Quantity}");
            }

            for (var i = slip.Items.Count; i < 12; i++)
            {
                table.Cell().Element(EmptyCell);
                table.Cell().Element(EmptyCell);
                table.Cell().Element(EmptyCell);
            }
        });
    }

    // ── Signatures ──────────────────────────────────────────────────

    private static void ComposeSignatures(IContainer container, Dictionary<string, string> common, string leftTitle, string rightTitle)
    {
        container.PaddingTop(8).Row(row =>
        {
            row.RelativeItem().Element(c =>
                ComposeOneSignatureBlock(c, common, leftTitle));
            row.ConstantItem(20);
            row.RelativeItem().Element(c =>
                ComposeOneSignatureBlock(c, common, rightTitle));
        });
    }

    private static void ComposeOneSignatureBlock(IContainer container, Dictionary<string, string> common, string title)
    {
        container.ContentFromRightToLeft().Column(col =>
        {
            col.Item().Text(title).FontFamily(ThaanaFont).FontSize(8).Bold();
            col.Item().PaddingTop(2).Element(c => SignatureField(c, L(common, "LabelName")));
            col.Item().Element(c => SignatureField(c, L(common, "LabelIdNo")));
            col.Item().Element(c => SignatureField(c, L(common, "LabelPhone")));
            col.Item().Element(c => SignatureField(c, L(common, "LabelSignature")));
            col.Item().Row(dateTimeRow =>
            {
                dateTimeRow.RelativeItem().Element(c => SignatureField(c, L(common, "LabelDate")));
                dateTimeRow.ConstantItem(10);
                dateTimeRow.RelativeItem().Element(c => SignatureField(c, L(common, "LabelTime")));
            });
        });
    }

    private static void SignatureField(IContainer container, string label)
    {
        container.PaddingTop(3).Row(row =>
        {
            row.AutoItem().Text(label).FontFamily(ThaanaFont).FontSize(7);
            row.RelativeItem().PaddingBottom(1).AlignBottom()
                .LineHorizontal(0.3f).LineColor(Colors.Grey.Medium);
        });
    }

    // ── Cell styles ─────────────────────────────────────────────────

    private static IContainer HeaderCellRtl(IContainer container) =>
        container.ContentFromRightToLeft()
            .Background(Colors.Grey.Lighten3)
            .Border(0.5f).BorderColor(Colors.Grey.Darken1)
            .PaddingVertical(3).PaddingHorizontal(4)
            .DefaultTextStyle(x => x.FontSize(7).Bold());

    private static IContainer DataCellLtr(IContainer container) =>
        container.ContentFromLeftToRight()
            .Border(0.5f).BorderColor(Colors.Grey.Medium)
            .PaddingVertical(2).PaddingHorizontal(4)
            .DefaultTextStyle(x => x.FontSize(7));

    private static IContainer EmptyCell(IContainer container) =>
        container.Border(0.5f).BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(2).PaddingHorizontal(4)
            .MinHeight(14);

    // ── Helpers ─────────────────────────────────────────────────────

    private static string L(Dictionary<string, string> labels, string key, string fallback = "")
        => labels.TryGetValue(key, out var value) ? value : fallback;
}
