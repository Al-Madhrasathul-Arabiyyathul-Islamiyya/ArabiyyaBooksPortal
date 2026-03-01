using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BooksPortal.IntegrationTests;

[Collection("Integration API")]
public class ReferenceNumberFormatIntegrationTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReferenceNumberFormatIntegrationTests(IntegrationTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Delete_Recreate_SameCombination_RestoresSoftDeletedRecord()
    {
        await AuthenticateAsync();
        var academicYearId = await CreateAcademicYearAsync();

        var created = await CreateReferenceFormatAsync(academicYearId, "Return", "RTN{year}{autonum}", 6);
        var createdId = created.GetProperty("id").GetInt32();

        var deleteResponse = await _client.DeleteAsync($"/api/ReferenceNumberFormats/{createdId}");
        deleteResponse.EnsureSuccessStatusCode();

        var recreated = await CreateReferenceFormatAsync(academicYearId, "Return", "RTN-{year}-{autonum}", 8);
        recreated.GetProperty("id").GetInt32().Should().Be(createdId);
        recreated.GetProperty("formatTemplate").GetString().Should().Be("RTN-{year}-{autonum}");
        recreated.GetProperty("paddingWidth").GetInt32().Should().Be(8);
    }

    [Fact]
    public async Task Create_DuplicateActiveCombination_ReturnsConflict()
    {
        await AuthenticateAsync();
        var academicYearId = await CreateAcademicYearAsync();

        await CreateReferenceFormatAsync(academicYearId, "Distribution", "DST{year}{autonum}", 6);

        var duplicateResponse = await _client.PostAsJsonAsync("/api/ReferenceNumberFormats", new
        {
            slipType = "Distribution",
            academicYearId,
            formatTemplate = "DST-{year}-{autonum}",
            paddingWidth = 6
        });

        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        using var duplicateDoc = JsonDocument.Parse(await duplicateResponse.Content.ReadAsStringAsync());
        duplicateDoc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        duplicateDoc.RootElement.GetProperty("message").GetString()
            .Should().Contain("already exists");
    }

    private async Task AuthenticateAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@booksportal.local",
            password = "Admin@123456"
        });
        loginResponse.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
        token.Should().NotBeNullOrWhiteSpace();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<int> CreateAcademicYearAsync()
    {
        var suffix = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var year = 9000 + (int)(suffix % 500);

        var response = await _client.PostAsJsonAsync("/api/AcademicYears", new
        {
            name = $"AY {year} Integration",
            year,
            startDate = new DateTime(year, 1, 1),
            endDate = new DateTime(year, 12, 31)
        });
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("data").GetProperty("id").GetInt32();
    }

    private async Task<JsonElement> CreateReferenceFormatAsync(int academicYearId, string slipType, string template, int paddingWidth)
    {
        var response = await _client.PostAsJsonAsync("/api/ReferenceNumberFormats", new
        {
            slipType,
            academicYearId,
            formatTemplate = template,
            paddingWidth
        });
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("data").Clone();
    }
}
