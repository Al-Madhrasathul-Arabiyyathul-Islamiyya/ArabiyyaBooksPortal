using BooksPortal.API.Services;
using BooksPortal.Application.Features.Distribution.DTOs;
using BooksPortal.Application.Features.Returns.DTOs;
using BooksPortal.Application.Features.TeacherIssues.DTOs;
using BooksPortal.Domain.Enums;
using FluentAssertions;
using QuestPDF.Infrastructure;

namespace BooksPortal.UnitTests.Features.Print;

public class PdfServiceTests
{
    private readonly PdfService _sut;

    public PdfServiceTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        _sut = new PdfService();
    }

    [Fact]
    public void GenerateDistributionSlip_ProducesValidPdf()
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
            Items = new List<DistributionSlipItemResponse>
            {
                new() { BookCode = "MAT-G1", BookTitle = "Mathematics Grade 1", Quantity = 1 },
                new() { BookCode = "ENG-G1", BookTitle = "English Grade 1", Quantity = 1 },
                new() { BookCode = "SCI-G1", BookTitle = "Science Grade 1", Quantity = 2 }
            }
        };

        var result = _sut.GenerateDistributionSlip(slip);

        result.Should().NotBeNullOrEmpty();
        result.Length.Should().BeGreaterThan(100);
        // PDF magic bytes
        result[0].Should().Be(0x25); // '%'
        result[1].Should().Be(0x50); // 'P'
        result[2].Should().Be(0x44); // 'D'
        result[3].Should().Be(0x46); // 'F'
    }

    [Fact]
    public void GenerateDistributionSlip_WithNoNotes_ProducesValidPdf()
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
            Items = new List<DistributionSlipItemResponse>
            {
                new() { BookCode = "ARA-G2", BookTitle = "Arabic Grade 2", Quantity = 1 }
            }
        };

        var result = _sut.GenerateDistributionSlip(slip);

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateReturnSlip_ProducesValidPdf()
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
            Items = new List<ReturnSlipItemResponse>
            {
                new() { BookCode = "MAT-G1", BookTitle = "Mathematics Grade 1", Quantity = 1, Condition = BookCondition.Good },
                new() { BookCode = "ENG-G1", BookTitle = "English Grade 1", Quantity = 1, Condition = BookCondition.Damaged, ConditionNotes = "Cover torn" },
                new() { BookCode = "SCI-G1", BookTitle = "Science Grade 1", Quantity = 1, Condition = BookCondition.Lost }
            }
        };

        var result = _sut.GenerateReturnSlip(slip);

        result.Should().NotBeNullOrEmpty();
        result.Length.Should().BeGreaterThan(100);
        result[0].Should().Be(0x25);
        result[1].Should().Be(0x50);
    }

    [Fact]
    public void GenerateReturnSlip_WithMixedConditions_ProducesValidPdf()
    {
        var slip = new ReturnSlipResponse
        {
            ReferenceNo = "RTN2025000002",
            AcademicYearName = "2025/2026",
            StudentName = "Omar Khalil",
            StudentIndexNo = "STU003",
            ReturnedByName = "Khalil Ibrahim",
            ReceivedAt = DateTime.UtcNow,
            Items = new List<ReturnSlipItemResponse>
            {
                new() { BookCode = "ARA-G1", BookTitle = "Arabic Grade 1", Quantity = 2, Condition = BookCondition.Fair },
                new() { BookCode = "ISL-G1", BookTitle = "Islamic Studies Grade 1", Quantity = 1, Condition = BookCondition.Poor }
            }
        };

        var result = _sut.GenerateReturnSlip(slip);

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateTeacherIssueSlip_ProducesValidPdf()
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
            Items = new List<TeacherIssueItemResponse>
            {
                new() { BookCode = "MAT-G1", BookTitle = "Mathematics Grade 1 Teacher Edition", Quantity = 5, ReturnedQuantity = 0, OutstandingQuantity = 5 },
                new() { BookCode = "MAT-G2", BookTitle = "Mathematics Grade 2 Teacher Edition", Quantity = 3, ReturnedQuantity = 0, OutstandingQuantity = 3 }
            }
        };

        var result = _sut.GenerateTeacherIssueSlip(issue);

        result.Should().NotBeNullOrEmpty();
        result.Length.Should().BeGreaterThan(100);
        result[0].Should().Be(0x25);
        result[1].Should().Be(0x50);
    }

    [Fact]
    public void GenerateTeacherIssueSlip_WithNoExpectedReturnDate_ProducesValidPdf()
    {
        var issue = new TeacherIssueResponse
        {
            ReferenceNo = "TIS2025000002",
            AcademicYearName = "2025/2026",
            TeacherName = "Mr. Ibrahim Ali",
            IssuedAt = DateTime.UtcNow,
            ExpectedReturnDate = null,
            Status = TeacherIssueStatus.Active,
            Items = new List<TeacherIssueItemResponse>
            {
                new() { BookCode = "ENG-G3", BookTitle = "English Grade 3", Quantity = 10, ReturnedQuantity = 0, OutstandingQuantity = 10 }
            }
        };

        var result = _sut.GenerateTeacherIssueSlip(issue);

        result.Should().NotBeNullOrEmpty();
    }
}
