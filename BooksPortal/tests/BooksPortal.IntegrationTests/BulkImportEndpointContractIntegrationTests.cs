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

    [Fact]
    public async Task Parents_BulkValidate_AcceptsGeneratedTemplate_File()
    {
        await AuthenticateAsync();

        var templateBytes = await DownloadTemplateAsync("/api/import-templates/parents");
        using var response = await PostWorkbookAsync("/api/parents/bulk/validate", "parents-import-template.xlsx", templateBytes);

        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("data").GetProperty("entity").GetString().Should().Be("Parent");
    }

    [Theory]
    [InlineData("/api/books/bulk/validate")]
    [InlineData("/api/students/bulk/validate")]
    [InlineData("/api/teachers/bulk/validate")]
    [InlineData("/api/parents/bulk/validate")]
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

    [Fact]
    public async Task Parents_BulkValidate_ShouldFlagMissingStudent_ByStudentIndexNo()
    {
        await AuthenticateAsync();

        var workbookBytes = CreateWorkbook(
            "Parents",
            ["FullName", "NationalId", "Phone", "Relationship", "StudentIndexNo"],
            ["Bulk Linked Parent", $"P-{Guid.NewGuid():N}"[..10], "7700000", "Father", "IDX-DOES-NOT-EXIST"]);

        using var response = await PostWorkbookAsync("/api/parents/bulk/validate", "parents-student-link.xlsx", workbookBytes);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var issues = doc.RootElement.GetProperty("data").GetProperty("issues");
        issues.GetArrayLength().Should().BeGreaterThan(0);
        issues.EnumerateArray().Any(i =>
            string.Equals(i.GetProperty("field").GetString(), "StudentIndexNo", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(i.GetProperty("code").GetString(), "NotFound", StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue("parent bulk should validate student linkage by StudentIndexNo");
    }

    [Fact]
    public async Task Books_BulkCommit_AllNewRows_ShouldInsertData()
    {
        await AuthenticateAsync();

        var subjectCode = await GetAnySubjectCodeAsync();
        var academicYear = await GetActiveAcademicYearNameAsync();
        var codeA = $"BK-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000000:D6}";
        var codeB = $"BK-{(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 1) % 1000000:D6}";

        var workbookBytes = CreateWorkbook(
            "Books",
            ["Code", "Title", "SubjectCode", "Publisher", "PublishedYear", "AcademicYear", "Quantity"],
            new[]
            {
                new[] { codeA, "Bulk Insert A", subjectCode, "Other", "2026", academicYear, "3" },
                new[] { codeB, "Bulk Insert B", subjectCode, "Other", "2026", academicYear, "4" }
            });

        using var response = await PostWorkbookAsync("/api/books/bulk/commit", "books-commit-all-new.xlsx", workbookBytes);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var report = doc.RootElement.GetProperty("data");
        report.GetProperty("insertedRows").GetInt32().Should().Be(2);
        report.GetProperty("failedRows").GetInt32().Should().Be(0);
        report.GetProperty("canCommit").GetBoolean().Should().BeTrue();

        await AssertBookExistsAsync(codeA);
        await AssertBookExistsAsync(codeB);
    }

    [Fact]
    public async Task Books_BulkCommit_MixedDuplicateAndNewRows_ShouldUpsertAndInsertWithRowStatuses()
    {
        await AuthenticateAsync();

        var subjectCode = await GetAnySubjectCodeAsync();
        var activeYearId = await GetActiveAcademicYearIdAsync();
        var duplicateCode = $"BK-DUP-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000000:D6}";
        var newCode = $"BK-NEW-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000000:D6}";

        await CreateBookAsync(duplicateCode, subjectCode);

        var workbookBytes = CreateWorkbook(
            "Books",
            ["Code", "Title", "SubjectCode", "Publisher", "PublishedYear", "AcademicYearId", "Quantity"],
            new[]
            {
                new[] { duplicateCode, "Duplicate Row", subjectCode, "Other", "2026", activeYearId.ToString(), "2" },
                new[] { newCode, "Potential New Row", subjectCode, "Other", "2026", activeYearId.ToString(), "2" }
            });

        using var response = await PostWorkbookAsync("/api/books/bulk/commit", "books-commit-mixed.xlsx", workbookBytes);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var report = doc.RootElement.GetProperty("data");
        report.GetProperty("canCommit").GetBoolean().Should().BeTrue();
        report.GetProperty("insertedRows").GetInt32().Should().Be(1);
        report.GetProperty("updatedRows").GetInt32().Should().Be(1);
        report.GetProperty("failedRows").GetInt32().Should().Be(0);
        report.GetProperty("rows").EnumerateArray().Any(r =>
            string.Equals(r.GetProperty("key").GetString(), duplicateCode, StringComparison.OrdinalIgnoreCase)
            && string.Equals(r.GetProperty("status").GetString(), "Updated", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
        report.GetProperty("rows").EnumerateArray().Any(r =>
            string.Equals(r.GetProperty("key").GetString(), newCode, StringComparison.OrdinalIgnoreCase)
            && string.Equals(r.GetProperty("status").GetString(), "Inserted", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();

        await AssertBookExistsAsync(duplicateCode);
        await AssertBookExistsAsync(newCode);
    }

    [Fact]
    public async Task Books_BulkCommitAsync_ShouldExposeProgressAndDownloadableReport()
    {
        await AuthenticateAsync();

        var subjectCode = await GetAnySubjectCodeAsync();
        var academicYear = await GetActiveAcademicYearNameAsync();
        var code = $"BK-ASYNC-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000000:D6}";
        var workbookBytes = CreateWorkbook(
            "Books",
            ["Code", "Title", "SubjectCode", "Publisher", "PublishedYear", "AcademicYear", "Quantity"],
            [code, "Async Book", subjectCode, "Other", "2026", academicYear, "1"]);

        using var startResponse = await PostWorkbookAsync("/api/books/bulk/commit-async", "books-async.xlsx", workbookBytes);
        startResponse.EnsureSuccessStatusCode();

        using var startDoc = JsonDocument.Parse(await startResponse.Content.ReadAsStringAsync());
        var jobId = startDoc.RootElement.GetProperty("data").GetProperty("jobId").GetGuid();

        JsonDocument? finalStatus = null;
        for (var i = 0; i < 30; i++)
        {
            using var statusResponse = await _client.GetAsync($"/api/books/bulk/jobs/{jobId}");
            statusResponse.EnsureSuccessStatusCode();
            var statusDoc = JsonDocument.Parse(await statusResponse.Content.ReadAsStringAsync());
            var status = statusDoc.RootElement.GetProperty("data").GetProperty("status").GetString();
            if (string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                finalStatus = statusDoc;
                break;
            }

            await Task.Delay(250);
        }

        finalStatus.Should().NotBeNull();
        var reportReady = finalStatus!.RootElement.GetProperty("data").GetProperty("reportReady").GetBoolean();
        reportReady.Should().BeTrue();

        using var reportResponse = await _client.GetAsync($"/api/books/bulk/jobs/{jobId}/report");
        reportResponse.EnsureSuccessStatusCode();
        var reportBytes = await reportResponse.Content.ReadAsByteArrayAsync();
        reportBytes.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Students_BulkCommitAsync_ShouldCompleteAndProduceReport()
    {
        await AuthenticateAsync();

        var classSectionId = await EnsureClassSectionAsync();
        var indexNo = $"IDX-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000000:D6}";
        var workbookBytes = CreateWorkbook(
            "Students",
            ["FullName", "IndexNo", "NationalId", "ClassSectionId", "ParentNationalId"],
            [$"Student {indexNo}", indexNo, $"S-{Guid.NewGuid():N}"[..10], classSectionId.ToString(), ""]);

        using var startResponse = await PostWorkbookAsync("/api/students/bulk/commit-async", "students-async.xlsx", workbookBytes);
        startResponse.EnsureSuccessStatusCode();
        using var startDoc = JsonDocument.Parse(await startResponse.Content.ReadAsStringAsync());
        var jobId = startDoc.RootElement.GetProperty("data").GetProperty("jobId").GetGuid();

        var status = await WaitForJobCompletionAsync($"/api/students/bulk/jobs/{jobId}");
        status.GetProperty("reportReady").GetBoolean().Should().BeTrue();
        status.GetProperty("entity").GetString()?.ToLowerInvariant().Should().Be("student");

        using var reportResponse = await _client.GetAsync($"/api/students/bulk/jobs/{jobId}/report");
        reportResponse.EnsureSuccessStatusCode();
        (await reportResponse.Content.ReadAsByteArrayAsync()).Length.Should().BeGreaterThan(0);

        await AssertStudentExistsAsync(indexNo);
    }

    [Fact]
    public async Task Teachers_BulkCommitAsync_ShouldCompleteAndProduceReport()
    {
        await AuthenticateAsync();

        var nationalId = $"T-{Guid.NewGuid():N}"[..10];
        var workbookBytes = CreateWorkbook(
            "Teachers",
            ["FullName", "NationalId", "Email", "Phone"],
            [$"Teacher {nationalId}", nationalId, $"{nationalId.ToLowerInvariant()}@local.test", "7712345"]);

        using var startResponse = await PostWorkbookAsync("/api/teachers/bulk/commit-async", "teachers-async.xlsx", workbookBytes);
        startResponse.EnsureSuccessStatusCode();
        using var startDoc = JsonDocument.Parse(await startResponse.Content.ReadAsStringAsync());
        var jobId = startDoc.RootElement.GetProperty("data").GetProperty("jobId").GetGuid();

        var status = await WaitForJobCompletionAsync($"/api/teachers/bulk/jobs/{jobId}");
        status.GetProperty("reportReady").GetBoolean().Should().BeTrue();
        status.GetProperty("entity").GetString()?.ToLowerInvariant().Should().Be("teacher");

        using var reportResponse = await _client.GetAsync($"/api/teachers/bulk/jobs/{jobId}/report");
        reportResponse.EnsureSuccessStatusCode();
        (await reportResponse.Content.ReadAsByteArrayAsync()).Length.Should().BeGreaterThan(0);

        await AssertTeacherExistsAsync(nationalId);
    }

    [Fact]
    public async Task Parents_BulkCommitAsync_ShouldCompleteAndProduceReport()
    {
        await AuthenticateAsync();

        var nationalId = $"P-{Guid.NewGuid():N}"[..10];
        var workbookBytes = CreateWorkbook(
            "Parents",
            ["FullName", "NationalId", "Phone", "Relationship", "StudentIndexNo"],
            [$"Parent {nationalId}", nationalId, "7700099", "Father", ""]);

        using var startResponse = await PostWorkbookAsync("/api/parents/bulk/commit-async", "parents-async.xlsx", workbookBytes);
        startResponse.EnsureSuccessStatusCode();
        using var startDoc = JsonDocument.Parse(await startResponse.Content.ReadAsStringAsync());
        var jobId = startDoc.RootElement.GetProperty("data").GetProperty("jobId").GetGuid();

        var status = await WaitForJobCompletionAsync($"/api/parents/bulk/jobs/{jobId}");
        status.GetProperty("reportReady").GetBoolean().Should().BeTrue();
        status.GetProperty("entity").GetString()?.ToLowerInvariant().Should().Be("parent");

        using var reportResponse = await _client.GetAsync($"/api/parents/bulk/jobs/{jobId}/report");
        reportResponse.EnsureSuccessStatusCode();
        (await reportResponse.Content.ReadAsByteArrayAsync()).Length.Should().BeGreaterThan(0);

        await AssertParentExistsAsync(nationalId);
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
        => CreateWorkbook(worksheetName, headers, [values]);

    private static byte[] CreateWorkbook(string worksheetName, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(worksheetName);

        for (var col = 0; col < headers.Count; col++)
        {
            sheet.Cell(1, col + 1).Value = headers[col];
        }

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var rowValues = rows[rowIndex];
            for (var col = 0; col < rowValues.Length; col++)
            {
                sheet.Cell(rowIndex + 2, col + 1).Value = rowValues[col];
            }
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

    private async Task<string> GetAnySubjectCodeAsync()
    {
        var response = await _client.GetAsync("/api/subjects");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var subjects = doc.RootElement.GetProperty("data");
        subjects.GetArrayLength().Should().BeGreaterThan(0);
        return subjects[0].GetProperty("code").GetString()!;
    }

    private async Task<int> GetActiveAcademicYearIdAsync()
    {
        var response = await _client.GetAsync("/api/AcademicYears/active");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("data").GetProperty("id").GetInt32();
    }

    private async Task<string> GetActiveAcademicYearNameAsync()
    {
        var response = await _client.GetAsync("/api/AcademicYears/active");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("data").GetProperty("name").GetString()!;
    }

    private async Task CreateBookAsync(string code, string subjectCode)
    {
        var subjectId = await ResolveSubjectIdByCodeAsync(subjectCode);
        using var createResponse = await _client.PostAsJsonAsync("/api/books", new
        {
            code,
            title = $"Book {code}",
            subjectId,
            publisher = "Other",
            publishedYear = 2026
        });
        createResponse.EnsureSuccessStatusCode();
    }

    private async Task<int> ResolveSubjectIdByCodeAsync(string subjectCode)
    {
        var response = await _client.GetAsync("/api/subjects");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var subjects = doc.RootElement.GetProperty("data");
        foreach (var subject in subjects.EnumerateArray())
        {
            if (string.Equals(subject.GetProperty("code").GetString(), subjectCode, StringComparison.OrdinalIgnoreCase))
            {
                return subject.GetProperty("id").GetInt32();
            }
        }

        throw new InvalidOperationException($"Subject with code '{subjectCode}' not found.");
    }

    private async Task AssertBookExistsAsync(string code)
    {
        var response = await _client.GetAsync($"/api/books/search?q={Uri.EscapeDataString(code)}");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var results = doc.RootElement.GetProperty("data");
        results.EnumerateArray().Any(item =>
            string.Equals(item.GetProperty("code").GetString(), code, StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue($"book '{code}' should be present after commit");
    }

    private async Task AssertBookNotExistsAsync(string code)
    {
        var response = await _client.GetAsync($"/api/books/search?q={Uri.EscapeDataString(code)}");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var results = doc.RootElement.GetProperty("data");
        results.EnumerateArray().Any(item =>
            string.Equals(item.GetProperty("code").GetString(), code, StringComparison.OrdinalIgnoreCase))
            .Should().BeFalse($"book '{code}' should not be inserted when commit is rejected");
    }

    private async Task<JsonElement> WaitForJobCompletionAsync(string endpoint)
    {
        JsonDocument? finalStatus = null;
        for (var i = 0; i < 40; i++)
        {
            using var statusResponse = await _client.GetAsync(endpoint);
            statusResponse.EnsureSuccessStatusCode();
            var statusDoc = JsonDocument.Parse(await statusResponse.Content.ReadAsStringAsync());
            var status = statusDoc.RootElement.GetProperty("data").GetProperty("status").GetString();
            if (string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase))
            {
                finalStatus = statusDoc;
                break;
            }

            await Task.Delay(250);
        }

        finalStatus.Should().NotBeNull();
        finalStatus!.RootElement.GetProperty("data").GetProperty("status").GetString().Should().Be("Completed");
        var snapshot = finalStatus.RootElement.GetProperty("data").Clone();
        finalStatus.Dispose();
        return snapshot;
    }

    private async Task AssertStudentExistsAsync(string indexNo)
    {
        var response = await _client.GetAsync($"/api/students?pageNumber=1&pageSize=20&search={Uri.EscapeDataString(indexNo)}");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("data").GetProperty("items");
        items.EnumerateArray().Any(item =>
            string.Equals(item.GetProperty("indexNo").GetString(), indexNo, StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue();
    }

    private async Task AssertTeacherExistsAsync(string nationalId)
    {
        var response = await _client.GetAsync($"/api/teachers?pageNumber=1&pageSize=20&search={Uri.EscapeDataString(nationalId)}");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("data").GetProperty("items");
        items.EnumerateArray().Any(item =>
            string.Equals(item.GetProperty("nationalId").GetString(), nationalId, StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue();
    }

    private async Task AssertParentExistsAsync(string nationalId)
    {
        var response = await _client.GetAsync($"/api/parents?pageNumber=1&pageSize=20&search={Uri.EscapeDataString(nationalId)}");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("data").GetProperty("items");
        items.EnumerateArray().Any(item =>
            string.Equals(item.GetProperty("nationalId").GetString(), nationalId, StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue();
    }
}
