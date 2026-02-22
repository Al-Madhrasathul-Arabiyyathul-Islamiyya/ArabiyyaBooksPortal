using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ClosedXML.Excel;
using FluentAssertions;

namespace BooksPortal.IntegrationTests;

[Collection("Integration API")]
public class BulkImportEndpointContractIntegrationTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BulkImportEndpointContractIntegrationTests(IntegrationTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Books_BulkValidate_AcceptsGeneratedTemplate_File()
    {
        await AuthenticateAsync();

        var templateBytes = await DownloadTemplateAsync("/api/import-templates/books");
        using var response = await PostWorkbookAsync("/api/books/bulk/validate", "books-import-template.xlsx", templateBytes);

        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("data").GetProperty("entity").GetString().Should().Be("Book");
    }

    [Fact]
    public async Task Students_BulkValidate_AcceptsGeneratedTemplate_File()
    {
        await AuthenticateAsync();

        var templateBytes = await DownloadTemplateAsync("/api/import-templates/students");
        using var response = await PostWorkbookAsync("/api/students/bulk/validate", "students-import-template.xlsx", templateBytes);

        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("data").GetProperty("entity").GetString().Should().Be("Student");
    }

    [Fact]
    public async Task Teachers_BulkValidate_AcceptsGeneratedTemplate_File()
    {
        await AuthenticateAsync();

        var templateBytes = await DownloadTemplateAsync("/api/import-templates/teachers");
        using var response = await PostWorkbookAsync("/api/teachers/bulk/validate", "teachers-import-template.xlsx", templateBytes);

        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("data").GetProperty("entity").GetString().Should().Be("Teacher");
    }

    [Theory]
    [InlineData("/api/books/bulk/validate")]
    [InlineData("/api/students/bulk/validate")]
    [InlineData("/api/teachers/bulk/validate")]
    public async Task BulkValidate_CorruptedWorkbook_ShouldReturnClientError(string endpoint)
    {
        await AuthenticateAsync();

        var corruptedBytes = System.Text.Encoding.UTF8.GetBytes("this-is-not-a-valid-xlsx");
        using var response = await PostWorkbookAsync(endpoint, "corrupted.xlsx", corruptedBytes);

        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Parents_BulkValidate_Endpoint_ShouldExist()
    {
        await AuthenticateAsync();

        var workbookBytes = CreateWorkbook(
            "Parents",
            ["FullName", "NationalId", "Phone", "StudentIndexNo"],
            ["Integration Parent", "P-INT-0001", "7700000", "IDX-0001"]);

        using var response = await PostWorkbookAsync("/api/parents/bulk/validate", "parents-import-template.xlsx", workbookBytes);

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Parents_BulkCommit_Endpoint_ShouldExist()
    {
        await AuthenticateAsync();

        var workbookBytes = CreateWorkbook(
            "Parents",
            ["FullName", "NationalId", "Phone", "StudentIndexNo"],
            ["Integration Parent", "P-INT-0002", "7700000", "IDX-0002"]);

        using var response = await PostWorkbookAsync("/api/parents/bulk/commit", "parents-import-template.xlsx", workbookBytes);

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Students_BulkValidate_ShouldFlagMissingParent_ByParentNationalId()
    {
        await AuthenticateAsync();

        var classSectionId = await EnsureClassSectionAsync();
        var workbookBytes = CreateWorkbook(
            "Students",
            ["FullName", "IndexNo", "NationalId", "ClassSectionId", "ParentNationalId"],
            ["Bulk Linked Student", $"IDX-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000000:D6}", $"S-{Guid.NewGuid():N}"[..10], classSectionId.ToString(), "P-DOES-NOT-EXIST"]);

        using var response = await PostWorkbookAsync("/api/students/bulk/validate", "students-parent-link.xlsx", workbookBytes);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var issues = doc.RootElement.GetProperty("data").GetProperty("issues");
        issues.GetArrayLength().Should().BeGreaterThan(0);
        issues.EnumerateArray().Any(i =>
            string.Equals(i.GetProperty("field").GetString(), "ParentNationalId", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(i.GetProperty("code").GetString(), "NotFound", StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue("student bulk should validate parent linkage by ParentNationalId");
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

    private async Task<byte[]> DownloadTemplateAsync(string endpoint)
    {
        using var response = await _client.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    private async Task<HttpResponseMessage> PostWorkbookAsync(string endpoint, string fileName, byte[] fileBytes)
    {
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        form.Add(fileContent, "file", fileName);

        return await _client.PostAsync(endpoint, form);
    }

    private static byte[] CreateWorkbook(string worksheetName, IReadOnlyList<string> headers, params string[] values)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(worksheetName);

        for (var col = 0; col < headers.Count; col++)
        {
            sheet.Cell(1, col + 1).Value = headers[col];
        }

        for (var col = 0; col < values.Length; col++)
        {
            sheet.Cell(2, col + 1).Value = values[col];
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private async Task<int> EnsureClassSectionAsync()
    {
        var existingResponse = await _client.GetAsync("/api/ClassSections?pageNumber=1&pageSize=1");
        existingResponse.EnsureSuccessStatusCode();
        using (var existingDoc = JsonDocument.Parse(await existingResponse.Content.ReadAsStringAsync()))
        {
            var items = existingDoc.RootElement.GetProperty("data").GetProperty("items");
            if (items.GetArrayLength() > 0)
            {
                return items[0].GetProperty("id").GetInt32();
            }
        }

        var activeYearResponse = await _client.GetAsync("/api/AcademicYears/active");
        activeYearResponse.EnsureSuccessStatusCode();
        using var activeYearDoc = JsonDocument.Parse(await activeYearResponse.Content.ReadAsStringAsync());
        var academicYearId = activeYearDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32();

        var keystageResponse = await _client.GetAsync("/api/keystages");
        keystageResponse.EnsureSuccessStatusCode();
        using var keystageDoc = JsonDocument.Parse(await keystageResponse.Content.ReadAsStringAsync());
        var keystageId = keystageDoc.RootElement.GetProperty("data")[0].GetProperty("id").GetInt32();

        var gradesResponse = await _client.GetAsync($"/api/lookups/grades?keystageId={keystageId}");
        gradesResponse.EnsureSuccessStatusCode();
        using var gradesDoc = JsonDocument.Parse(await gradesResponse.Content.ReadAsStringAsync());
        var gradeId = gradesDoc.RootElement.GetProperty("data")[0].GetProperty("id").GetInt32();

        var uniqueSection = $"BI{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000:D3}";
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
