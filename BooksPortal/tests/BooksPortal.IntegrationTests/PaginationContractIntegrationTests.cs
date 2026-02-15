using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BooksPortal.IntegrationTests;

[Collection("Integration API")]
public class PaginationContractIntegrationTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PaginationContractIntegrationTests(IntegrationTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Users_List_ReturnsPaginatedEnvelope()
    {
        await AuthenticateAsync();

        var response = await _client.GetAsync("/api/users?pageNumber=1&pageSize=1&search=admin");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = doc.RootElement.GetProperty("data");

        data.GetProperty("pageNumber").GetInt32().Should().Be(1);
        data.GetProperty("pageSize").GetInt32().Should().Be(1);
        data.GetProperty("items").GetArrayLength().Should().BeLessThanOrEqualTo(1);
        data.GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ClassSections_List_ReturnsPaginatedEnvelope()
    {
        await AuthenticateAsync();

        var activeYearId = await GetActiveAcademicYearIdAsync();
        await EnsureClassSectionAsync(activeYearId);

        var response = await _client.GetAsync($"/api/ClassSections?pageNumber=1&pageSize=1&academicYearId={activeYearId}");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = doc.RootElement.GetProperty("data");

        data.GetProperty("pageNumber").GetInt32().Should().Be(1);
        data.GetProperty("pageSize").GetInt32().Should().Be(1);
        data.GetProperty("items").GetArrayLength().Should().BeLessThanOrEqualTo(1);
        data.GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Report_StockSummary_ReturnsPaginatedEnvelope()
    {
        await AuthenticateAsync();

        var activeYearId = await GetActiveAcademicYearIdAsync();
        await EnsureOperationalBookAsync(activeYearId);

        var response = await _client.GetAsync("/api/reports/stock-summary?pageNumber=1&pageSize=1");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = doc.RootElement.GetProperty("data");

        data.GetProperty("pageNumber").GetInt32().Should().Be(1);
        data.GetProperty("pageSize").GetInt32().Should().Be(1);
        data.GetProperty("items").GetArrayLength().Should().BeLessThanOrEqualTo(1);
        data.GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Book_StockHistory_ReturnsPaginatedEnvelope()
    {
        await AuthenticateAsync();

        var activeYearId = await GetActiveAcademicYearIdAsync();
        var bookId = await EnsureOperationalBookAsync(activeYearId);

        var entriesResponse = await _client.GetAsync($"/api/Books/{bookId}/stock-entries?pageNumber=1&pageSize=1");
        entriesResponse.EnsureSuccessStatusCode();

        using var entriesDoc = JsonDocument.Parse(await entriesResponse.Content.ReadAsStringAsync());
        var entriesData = entriesDoc.RootElement.GetProperty("data");
        entriesData.GetProperty("pageNumber").GetInt32().Should().Be(1);
        entriesData.GetProperty("pageSize").GetInt32().Should().Be(1);
        entriesData.GetProperty("items").GetArrayLength().Should().BeLessThanOrEqualTo(1);
        entriesData.GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0);

        var movementsResponse = await _client.GetAsync($"/api/Books/{bookId}/stock-movements?pageNumber=1&pageSize=1");
        movementsResponse.EnsureSuccessStatusCode();

        using var movementsDoc = JsonDocument.Parse(await movementsResponse.Content.ReadAsStringAsync());
        var movementsData = movementsDoc.RootElement.GetProperty("data");
        movementsData.GetProperty("pageNumber").GetInt32().Should().Be(1);
        movementsData.GetProperty("pageSize").GetInt32().Should().Be(1);
        movementsData.GetProperty("items").GetArrayLength().Should().BeLessThanOrEqualTo(1);
        movementsData.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(0);
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

    private async Task<int> GetActiveAcademicYearIdAsync()
    {
        var response = await _client.GetAsync("/api/AcademicYears/active");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("data").GetProperty("id").GetInt32();
    }

    private async Task<int> EnsureOperationalBookAsync(int academicYearId)
    {
        var response = await _client.GetAsync($"/api/lookups/operations/books?academicYearId={academicYearId}&take=1");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var books = doc.RootElement.GetProperty("data");
        if (books.GetArrayLength() > 0)
            return books[0].GetProperty("id").GetInt32();

        var subjectId = await EnsureSubjectAsync();
        var unique = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var createBookResponse = await _client.PostAsJsonAsync("/api/Books", new
        {
            code = $"IT-BK-{unique}",
            title = $"Integration Book {unique}",
            subjectId,
            publisher = "Other",
            publishedYear = 2026
        });
        createBookResponse.EnsureSuccessStatusCode();

        using var createDoc = JsonDocument.Parse(await createBookResponse.Content.ReadAsStringAsync());
        var bookId = createDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32();

        var stockEntryResponse = await _client.PostAsJsonAsync($"/api/Books/{bookId}/stock-entry", new
        {
            academicYearId,
            quantity = 10,
            source = "Integration Test",
            notes = "Seeded for pagination integration tests"
        });
        stockEntryResponse.EnsureSuccessStatusCode();

        return bookId;
    }

    private async Task<int> EnsureSubjectAsync()
    {
        var response = await _client.GetAsync("/api/lookups/subjects");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var subjects = doc.RootElement.GetProperty("data");
        if (subjects.GetArrayLength() > 0)
            return subjects[0].GetProperty("id").GetInt32();

        var unique = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 10000;
        var createResponse = await _client.PostAsJsonAsync("/api/subjects", new
        {
            code = $"S{unique:D4}",
            name = $"Integration Subject {unique:D4}"
        });
        createResponse.EnsureSuccessStatusCode();

        using var createDoc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        return createDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32();
    }

    private async Task<int> EnsureClassSectionAsync(int academicYearId)
    {
        var existingResponse = await _client.GetAsync($"/api/ClassSections?pageNumber=1&pageSize=1&academicYearId={academicYearId}");
        existingResponse.EnsureSuccessStatusCode();
        using (var existingDoc = JsonDocument.Parse(await existingResponse.Content.ReadAsStringAsync()))
        {
            var items = existingDoc.RootElement.GetProperty("data").GetProperty("items");
            if (items.GetArrayLength() > 0)
            {
                return items[0].GetProperty("id").GetInt32();
            }
        }

        var keystageResponse = await _client.GetAsync("/api/keystages");
        keystageResponse.EnsureSuccessStatusCode();
        using var keystageDoc = JsonDocument.Parse(await keystageResponse.Content.ReadAsStringAsync());
        var keystageId = keystageDoc.RootElement.GetProperty("data")[0].GetProperty("id").GetInt32();

        var gradesResponse = await _client.GetAsync($"/api/lookups/grades?keystageId={keystageId}");
        gradesResponse.EnsureSuccessStatusCode();
        using var gradesDoc = JsonDocument.Parse(await gradesResponse.Content.ReadAsStringAsync());
        var gradeId = gradesDoc.RootElement.GetProperty("data")[0].GetProperty("id").GetInt32();

        var uniqueSection = $"P{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000:D3}";
        var createResponse = await _client.PostAsJsonAsync("/api/ClassSections", new
        {
            academicYearId,
            keystageId,
            gradeId,
            section = uniqueSection
        });
        createResponse.EnsureSuccessStatusCode();

        using var createDoc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        return createDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32();
    }
}
