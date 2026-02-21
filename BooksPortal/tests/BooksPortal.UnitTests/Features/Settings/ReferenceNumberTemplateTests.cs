using BooksPortal.Infrastructure.Services;
using FluentAssertions;

namespace BooksPortal.UnitTests.Features.Settings;

public class ReferenceNumberTemplateTests
{
    [Fact]
    public void ApplyTemplate_DefaultFormat_ProducesExpectedOutput()
    {
        var result = ReferenceNumberService.ApplyTemplate("DST{year}{autonum}", "2026", 1, 6);
        result.Should().Be("DST2026000001");
    }

    [Fact]
    public void ApplyTemplate_CustomFormat_SubstitutesTokens()
    {
        var result = ReferenceNumberService.ApplyTemplate("MAI/{year}/bdp/{autonum}", "2025", 42, 6);
        result.Should().Be("MAI/2025/bdp/000042");
    }

    [Fact]
    public void ApplyTemplate_DifferentPadding_PadsCorrectly()
    {
        var result = ReferenceNumberService.ApplyTemplate("REF-{autonum}", "2026", 7, 4);
        result.Should().Be("REF-0007");
    }

    [Fact]
    public void ApplyTemplate_NoPadding_SingleDigit()
    {
        var result = ReferenceNumberService.ApplyTemplate("{autonum}", "2026", 5, 1);
        result.Should().Be("5");
    }

    [Fact]
    public void ApplyTemplate_LargeSequence_ExceedsPadding()
    {
        var result = ReferenceNumberService.ApplyTemplate("X{autonum}", "2026", 1234567, 4);
        result.Should().Be("X1234567");
    }

    [Fact]
    public void ApplyTemplate_NoTokens_ReturnsLiterals()
    {
        var result = ReferenceNumberService.ApplyTemplate("STATIC-REF", "2026", 1, 6);
        result.Should().Be("STATIC-REF");
    }

    [Fact]
    public void ApplyTemplate_CaseInsensitive_ReplacesTokens()
    {
        var result = ReferenceNumberService.ApplyTemplate("{YEAR}-{AUTONUM}", "2026", 1, 3);
        result.Should().Be("2026-001");
    }

    [Fact]
    public void ApplyTemplate_ComplexTemplate_AllTokensReplaced()
    {
        var result = ReferenceNumberService.ApplyTemplate("MAI/TRB/{year}/prefix-{autonum}", "2025", 99, 4);
        result.Should().Be("MAI/TRB/2025/prefix-0099");
    }

    [Fact]
    public void ApplyTemplate_DefaultFallback_MatchesOldFormat()
    {
        // Simulates the default fallback for Distribution: "DST{year}{autonum}" with padding 6
        var result = ReferenceNumberService.ApplyTemplate("DST{year}{autonum}", "2026", 1, 6);
        result.Should().Be("DST2026000001");
    }

    [Fact]
    public void ApplyTemplate_TeacherReturn_DefaultFormat()
    {
        var result = ReferenceNumberService.ApplyTemplate("TRT{year}{autonum}", "2026", 15, 6);
        result.Should().Be("TRT2026000015");
    }
}
