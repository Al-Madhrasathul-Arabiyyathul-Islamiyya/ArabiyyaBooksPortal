using BooksPortal.API.Services;
using BooksPortal.Application.Features.Distribution.DTOs;
using BooksPortal.Application.Features.Returns.DTOs;
using BooksPortal.Application.Features.Settings.Interfaces;
using BooksPortal.Application.Features.Settings.Services;
using BooksPortal.Application.Features.TeacherIssues.DTOs;
using BooksPortal.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using QuestPDF.Infrastructure;

namespace BooksPortal.UnitTests.Features.Print;

public class PdfServiceTests
{
    private readonly PdfService _sut;

    // Set to a directory path to save generated PDFs for visual inspection, e.g. @"D:\temp\pdfs"
    private static readonly string? SavePdfsTo = null;

    public PdfServiceTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var defaults = SlipTemplateSettingService.GetDefaultLabels();
        var labelService = Substitute.For<ISlipTemplateSettingService>();
        labelService.GetLabelsByCategoryAsync(Arg.Any<string>())
            .Returns(callInfo =>
            {
                var category = callInfo.Arg<string>();
                return defaults
                    .Where(l => l.Category == category)
                    .ToDictionary(l => l.Key, l => l.Value);
            });

        _sut = new PdfService(labelService);
    }

    [Fact]
    public async Task GenerateDistributionSlip_ProducesValidPdf()
    {
        var slip = new DistributionSlipResponse
        {
            Id = 1,
            ReferenceNo = "DST2025000001",
            AcademicYearName = "2025/2026",
            Term = Term.Term1,
            StudentName = "Ahmed Hassan",
            StudentIndexNo = "STU001",
            ParentName = "Mohamed Hassan",
            IssuedAt = new DateTime(2025, 9, 15),
            Notes = "First term distribution",
            Items =
            [
                new() { BookCode = "MAT-G1", BookTitle = "Mathematics Grade 1", Quantity = 1 },
                new() { BookCode = "ENG-G1", BookTitle = "English Grade 1", Quantity = 1 },
                new() { BookCode = "SCI-G1", BookTitle = "Science Grade 1", Quantity = 2 }
            ]
        };

        var result = await _sut.GenerateDistributionSlipAsync(slip);

        AssertValidPdf(result);
        MaybeSave(result, "distribution-slip.pdf");
    }

    [Fact]
    public async Task GenerateDistributionSlip_WithNoNotes_ProducesValidPdf()
    {
        var slip = new DistributionSlipResponse
        {
            ReferenceNo = "DST2025000002",
            AcademicYearName = "2025/2026",
            Term = Term.Term2,
            StudentName = "Sara Ali",
            StudentIndexNo = "STU002",
            ParentName = "Ali Mohamed",
            IssuedAt = DateTime.UtcNow,
            Items = [new() { BookCode = "ARA-G2", BookTitle = "Arabic Grade 2", Quantity = 1 }]
        };

        var result = await _sut.GenerateDistributionSlipAsync(slip);

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateReturnSlip_ProducesValidPdf()
    {
        var slip = new ReturnSlipResponse
        {
            Id = 1,
            ReferenceNo = "RTN2025000001",
            AcademicYearName = "2025/2026",
            StudentName = "Ahmed Hassan",
            StudentIndexNo = "STU001",
            ReturnedByName = "Mohamed Hassan",
            ReceivedAt = new DateTime(2025, 12, 20),
            Notes = "End of term return",
            Items =
            [
                new() { BookCode = "MAT-G1", BookTitle = "Mathematics Grade 1", Quantity = 1, Condition = BookCondition.Good },
                new() { BookCode = "ENG-G1", BookTitle = "English Grade 1", Quantity = 1, Condition = BookCondition.Damaged, ConditionNotes = "Cover torn" },
                new() { BookCode = "SCI-G1", BookTitle = "Science Grade 1", Quantity = 1, Condition = BookCondition.Lost }
            ]
        };

        var result = await _sut.GenerateReturnSlipAsync(slip);

        AssertValidPdf(result);
        MaybeSave(result, "return-slip.pdf");
    }

    [Fact]
    public async Task GenerateReturnSlip_WithMixedConditions_ProducesValidPdf()
    {
        var slip = new ReturnSlipResponse
        {
            ReferenceNo = "RTN2025000002",
            AcademicYearName = "2025/2026",
            StudentName = "Omar Khalil",
            StudentIndexNo = "STU003",
            ReturnedByName = "Khalil Ibrahim",
            ReceivedAt = DateTime.UtcNow,
            Items =
            [
                new() { BookCode = "ARA-G1", BookTitle = "Arabic Grade 1", Quantity = 2, Condition = BookCondition.Fair },
                new() { BookCode = "ISL-G1", BookTitle = "Islamic Studies Grade 1", Quantity = 1, Condition = BookCondition.Poor }
            ]
        };

        var result = await _sut.GenerateReturnSlipAsync(slip);

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateTeacherIssueSlip_ProducesValidPdf()
    {
        var issue = new TeacherIssueResponse
        {
            Id = 1,
            ReferenceNo = "TIS2025000001",
            AcademicYearName = "2025/2026",
            TeacherName = "Ms. Fatima Ahmed",
            IssuedAt = new DateTime(2025, 9, 1),
            ExpectedReturnDate = new DateTime(2025, 12, 15),
            Status = TeacherIssueStatus.Active,
            Notes = "Term 1 teacher copies",
            Items =
            [
                new() { BookCode = "MAT-G1", BookTitle = "Mathematics Grade 1 Teacher Edition", Quantity = 5, ReturnedQuantity = 0, OutstandingQuantity = 5 },
                new() { BookCode = "MAT-G2", BookTitle = "Mathematics Grade 2 Teacher Edition", Quantity = 3, ReturnedQuantity = 0, OutstandingQuantity = 3 }
            ]
        };

        var result = await _sut.GenerateTeacherIssueSlipAsync(issue);

        AssertValidPdf(result);
        MaybeSave(result, "teacher-issue-slip.pdf");
    }

    [Fact]
    public async Task GenerateTeacherIssueSlip_WithNoExpectedReturnDate_ProducesValidPdf()
    {
        var issue = new TeacherIssueResponse
        {
            ReferenceNo = "TIS2025000002",
            AcademicYearName = "2025/2026",
            TeacherName = "Mr. Ibrahim Ali",
            IssuedAt = DateTime.UtcNow,
            ExpectedReturnDate = null,
            Status = TeacherIssueStatus.Active,
            Items = [new() { BookCode = "ENG-G3", BookTitle = "English Grade 3", Quantity = 10, ReturnedQuantity = 0, OutstandingQuantity = 10 }]
        };

        var result = await _sut.GenerateTeacherIssueSlipAsync(issue);

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateTeacherReturnSlip_ProducesValidPdf()
    {
        var slip = new TeacherReturnSlipResponse
        {
            Id = 1,
            ReferenceNo = "TRT2025000001",
            TeacherIssueId = 1,
            TeacherName = "Ms. Fatima Ahmed",
            AcademicYearName = "2025/2026",
            ReceivedAt = new DateTime(2025, 12, 15),
            Notes = "End of term return",
            Items =
            [
                new() { BookCode = "MAT-G1", BookTitle = "Mathematics Grade 1 Teacher Edition", Quantity = 5 },
                new() { BookCode = "MAT-G2", BookTitle = "Mathematics Grade 2 Teacher Edition", Quantity = 3 }
            ]
        };

        var result = await _sut.GenerateTeacherReturnSlipAsync(slip);

        AssertValidPdf(result);
        MaybeSave(result, "teacher-return-slip.pdf");
    }

    private static void AssertValidPdf(byte[] pdf)
    {
        pdf.Should().NotBeNullOrEmpty();
        pdf.Length.Should().BeGreaterThan(100);
        // PDF magic bytes: %PDF
        pdf[0].Should().Be(0x25);
        pdf[1].Should().Be(0x50);
        pdf[2].Should().Be(0x44);
        pdf[3].Should().Be(0x46);
    }

    private static void MaybeSave(byte[] pdf, string filename)
    {
        if (SavePdfsTo is null) return;
        Directory.CreateDirectory(SavePdfsTo);
        File.WriteAllBytes(Path.Combine(SavePdfsTo, filename), pdf);
    }
}
