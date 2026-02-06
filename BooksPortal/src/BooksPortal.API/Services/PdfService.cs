using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.Distribution.DTOs;
using BooksPortal.Application.Features.Returns.DTOs;
using BooksPortal.Application.Features.TeacherIssues.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BooksPortal.API.Services;

public class PdfService : IPdfService
{
    private const string SchoolName = "Arabiyya International School";
    private const string SchoolAddress = "Book Distribution Department";

    public byte[] GenerateDistributionSlip(DistributionSlipResponse slip)
    {
        var document = Document.Create(container =>
        {
            // Two copies: School Copy + Parent Copy
            foreach (var copyLabel in new[] { "School Copy", "Parent Copy" })
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(header => ComposeHeader(header, "BOOK DISTRIBUTION SLIP", copyLabel));

                    page.Content().Column(col =>
                    {
                        col.Spacing(8);

                        // Slip details
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Reference: {slip.ReferenceNo}").Bold();
                                c.Item().Text($"Date: {slip.IssuedAt:dd/MM/yyyy}");
                                c.Item().Text($"Academic Year: {slip.AcademicYearName}");
                                c.Item().Text($"Term: {slip.Term}");
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Student: {slip.StudentName}").Bold();
                                c.Item().Text($"Index No: {slip.StudentIndexNo}");
                                c.Item().Text($"Parent: {slip.ParentName}");
                            });
                        });

                        // Items table
                        col.Item().Element(e => ComposeDistributionTable(e, slip.Items));

                        // Notes
                        if (!string.IsNullOrWhiteSpace(slip.Notes))
                            col.Item().Text($"Notes: {slip.Notes}").FontSize(8).Italic();

                        // Signatures
                        col.Item().PaddingTop(15).Element(ComposeSignatures);
                    });

                    page.Footer().Element(ComposeFooter);
                });
            }
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateReturnSlip(ReturnSlipResponse slip)
    {
        var document = Document.Create(container =>
        {
            foreach (var copyLabel in new[] { "School Copy", "Parent Copy" })
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(header => ComposeHeader(header, "BOOK RETURN SLIP", copyLabel));

                    page.Content().Column(col =>
                    {
                        col.Spacing(8);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Reference: {slip.ReferenceNo}").Bold();
                                c.Item().Text($"Date: {slip.ReceivedAt:dd/MM/yyyy}");
                                c.Item().Text($"Academic Year: {slip.AcademicYearName}");
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Student: {slip.StudentName}").Bold();
                                c.Item().Text($"Index No: {slip.StudentIndexNo}");
                                c.Item().Text($"Returned By: {slip.ReturnedByName}");
                            });
                        });

                        col.Item().Element(e => ComposeReturnTable(e, slip.Items));

                        if (!string.IsNullOrWhiteSpace(slip.Notes))
                            col.Item().Text($"Notes: {slip.Notes}").FontSize(8).Italic();

                        col.Item().PaddingTop(15).Element(ComposeSignatures);
                    });

                    page.Footer().Element(ComposeFooter);
                });
            }
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateTeacherIssueSlip(TeacherIssueResponse issue)
    {
        var document = Document.Create(container =>
        {
            foreach (var copyLabel in new[] { "School Copy", "Teacher Copy" })
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(header => ComposeHeader(header, "TEACHER BOOK ISSUE SLIP", copyLabel));

                    page.Content().Column(col =>
                    {
                        col.Spacing(8);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Reference: {issue.ReferenceNo}").Bold();
                                c.Item().Text($"Date: {issue.IssuedAt:dd/MM/yyyy}");
                                c.Item().Text($"Academic Year: {issue.AcademicYearName}");
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Teacher: {issue.TeacherName}").Bold();
                                c.Item().Text($"Status: {issue.Status}");
                                if (issue.ExpectedReturnDate.HasValue)
                                    c.Item().Text($"Expected Return: {issue.ExpectedReturnDate:dd/MM/yyyy}");
                            });
                        });

                        col.Item().Element(e => ComposeTeacherIssueTable(e, issue.Items));

                        if (!string.IsNullOrWhiteSpace(issue.Notes))
                            col.Item().Text($"Notes: {issue.Notes}").FontSize(8).Italic();

                        col.Item().PaddingTop(15).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().PaddingTop(25).LineHorizontal(0.5f);
                                c.Item().Text("Issued By").FontSize(8).AlignCenter();
                            });
                            row.ConstantItem(30);
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().PaddingTop(25).LineHorizontal(0.5f);
                                c.Item().Text("Teacher Signature").FontSize(8).AlignCenter();
                            });
                        });
                    });

                    page.Footer().Element(ComposeFooter);
                });
            }
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, string title, string copyLabel)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(SchoolName).FontSize(12).Bold();
                    c.Item().Text(SchoolAddress).FontSize(8);
                });
                row.ConstantItem(80).AlignRight().Text(copyLabel).FontSize(8).Bold().Italic();
            });
            col.Item().PaddingVertical(5).LineHorizontal(1);
            col.Item().AlignCenter().Text(title).FontSize(11).Bold();
            col.Item().PaddingBottom(5);
        });
    }

    private static void ComposeDistributionTable(IContainer container, List<DistributionSlipItemResponse> items)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(25);  // #
                columns.ConstantColumn(60);  // Code
                columns.RelativeColumn();    // Title
                columns.ConstantColumn(40);  // Qty
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("#");
                header.Cell().Element(HeaderCell).Text("Code");
                header.Cell().Element(HeaderCell).Text("Book Title");
                header.Cell().Element(HeaderCell).AlignCenter().Text("Qty");
            });

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                table.Cell().Element(DataCell).Text($"{i + 1}");
                table.Cell().Element(DataCell).Text(item.BookCode);
                table.Cell().Element(DataCell).Text(item.BookTitle);
                table.Cell().Element(DataCell).AlignCenter().Text($"{item.Quantity}");
            }

            // Total row
            table.Cell().ColumnSpan(3).Element(TotalCell).AlignRight().Text("Total Books:");
            table.Cell().Element(TotalCell).AlignCenter().Text($"{items.Sum(i => i.Quantity)}");
        });
    }

    private static void ComposeReturnTable(IContainer container, List<ReturnSlipItemResponse> items)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(25);  // #
                columns.ConstantColumn(55);  // Code
                columns.RelativeColumn();    // Title
                columns.ConstantColumn(35);  // Qty
                columns.ConstantColumn(55);  // Condition
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("#");
                header.Cell().Element(HeaderCell).Text("Code");
                header.Cell().Element(HeaderCell).Text("Book Title");
                header.Cell().Element(HeaderCell).AlignCenter().Text("Qty");
                header.Cell().Element(HeaderCell).Text("Condition");
            });

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                table.Cell().Element(DataCell).Text($"{i + 1}");
                table.Cell().Element(DataCell).Text(item.BookCode);
                table.Cell().Element(DataCell).Text(item.BookTitle);
                table.Cell().Element(DataCell).AlignCenter().Text($"{item.Quantity}");
                table.Cell().Element(DataCell).Text(item.Condition.ToString());
            }

            table.Cell().ColumnSpan(4).Element(TotalCell).AlignRight().Text("Total Books:");
            table.Cell().Element(TotalCell).AlignCenter().Text($"{items.Sum(i => i.Quantity)}");
        });
    }

    private static void ComposeTeacherIssueTable(IContainer container, List<TeacherIssueItemResponse> items)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(25);  // #
                columns.ConstantColumn(55);  // Code
                columns.RelativeColumn();    // Title
                columns.ConstantColumn(35);  // Qty
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("#");
                header.Cell().Element(HeaderCell).Text("Code");
                header.Cell().Element(HeaderCell).Text("Book Title");
                header.Cell().Element(HeaderCell).AlignCenter().Text("Qty");
            });

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                table.Cell().Element(DataCell).Text($"{i + 1}");
                table.Cell().Element(DataCell).Text(item.BookCode);
                table.Cell().Element(DataCell).Text(item.BookTitle);
                table.Cell().Element(DataCell).AlignCenter().Text($"{item.Quantity}");
            }

            table.Cell().ColumnSpan(3).Element(TotalCell).AlignRight().Text("Total Books:");
            table.Cell().Element(TotalCell).AlignCenter().Text($"{items.Sum(i => i.Quantity)}");
        });
    }

    private static void ComposeSignatures(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().PaddingTop(25).LineHorizontal(0.5f);
                c.Item().Text("Issued By").FontSize(8).AlignCenter();
            });
            row.ConstantItem(30);
            row.RelativeItem().Column(c =>
            {
                c.Item().PaddingTop(25).LineHorizontal(0.5f);
                c.Item().Text("Received By (Parent/Guardian)").FontSize(8).AlignCenter();
            });
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Generated on ").FontSize(7);
            text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(7);
        });
    }

    private static IContainer HeaderCell(IContainer container) =>
        container.Background(Colors.Grey.Lighten3)
            .Border(0.5f)
            .BorderColor(Colors.Grey.Darken1)
            .Padding(3)
            .DefaultTextStyle(x => x.FontSize(8).Bold());

    private static IContainer DataCell(IContainer container) =>
        container.Border(0.5f)
            .BorderColor(Colors.Grey.Medium)
            .Padding(3)
            .DefaultTextStyle(x => x.FontSize(8));

    private static IContainer TotalCell(IContainer container) =>
        container.Background(Colors.Grey.Lighten4)
            .Border(0.5f)
            .BorderColor(Colors.Grey.Darken1)
            .Padding(3)
            .DefaultTextStyle(x => x.FontSize(8).Bold());
}
