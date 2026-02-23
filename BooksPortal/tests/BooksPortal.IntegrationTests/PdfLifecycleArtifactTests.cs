using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using BooksPortal.Domain.Enums;

namespace BooksPortal.IntegrationTests;

[Collection("Integration API")]
public class PdfLifecycleArtifactTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly string ArtifactDirectory = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestArtifacts", "PdfSamples"));

    public PdfLifecycleArtifactTests(IntegrationTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Generate_Finalized_And_Cancelled_Pdf_Samples()
    {
        ResetArtifactsDirectory();
        await AuthenticateAsync();

        var activeYearId = await GetActiveAcademicYearIdAsync();
        var distributionBookId = await EnsureOperationalBookAsync(activeYearId);
        var student = await EnsureStudentWithParentAsync(activeYearId);
        var parentId = await GetFirstParentIdAsync(student.Id);

        var distributionId = await CreateDistributionAsync(activeYearId, student.Id, parentId, distributionBookId);
        await FinalizeDistributionAsync(distributionId);
        await DownloadPdfAsync($"/api/Distributions/{distributionId}/print", "distribution-finalized.pdf");

        var returnId = await CreateReturnAsync(activeYearId, student.Id, parentId, distributionBookId);
        await DownloadPdfAsync($"/api/Returns/{returnId}/print", "return-finalized.pdf");

        var distributionCancelId = await CreateDistributionAsync(activeYearId, student.Id, parentId, distributionBookId);
        await CancelDistributionAsync(distributionCancelId);
        await DownloadPdfAsync($"/api/Distributions/{distributionCancelId}/print", "distribution-cancelled.pdf");

        var teacherId = await EnsureTeacherAsync();
        var teacherIssueId = await CreateTeacherIssueAsync(activeYearId, teacherId, distributionBookId);
        await FinalizeTeacherIssueAsync(teacherIssueId);
        await DownloadPdfAsync($"/api/TeacherIssues/{teacherIssueId}/print", "teacher-issue-finalized.pdf");

        await ProcessTeacherReturnAsync(teacherIssueId);
        await DownloadPdfAsync($"/api/TeacherIssues/{teacherIssueId}/return/print", "teacher-return-finalized.pdf");

        var teacherIssueCancelId = await CreateTeacherIssueAsync(activeYearId, teacherId, distributionBookId);
        await CancelTeacherIssueAsync(teacherIssueCancelId);
        await DownloadPdfAsync($"/api/TeacherIssues/{teacherIssueCancelId}/print", "teacher-issue-cancelled.pdf");
    }

    private async Task AuthenticateAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = TestSettings.Current.AdminEmail,
            password = TestSettings.Current.AdminPassword
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
        {
            return books[0].GetProperty("id").GetInt32();
        }

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
        using var createBookDoc = JsonDocument.Parse(await createBookResponse.Content.ReadAsStringAsync());
        var bookId = createBookDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32();

        var addStockResponse = await _client.PostAsJsonAsync($"/api/Books/{bookId}/stock-entry", new
        {
            academicYearId,
            quantity = 20,
            source = "Integration Test",
            notes = "Seeded for PDF artifact generation"
        });
        addStockResponse.EnsureSuccessStatusCode();

        return bookId;
    }

    private async Task<(int Id, string Name)> EnsureStudentWithParentAsync(int academicYearId)
    {
        var response = await _client.GetAsync($"/api/lookups/operations/students?academicYearId={academicYearId}&take=1");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var students = doc.RootElement.GetProperty("data");
        if (students.GetArrayLength() > 0)
        {
            return (
                students[0].GetProperty("id").GetInt32(),
                students[0].GetProperty("fullName").GetString() ?? string.Empty
            );
        }

        var classSectionId = await EnsureClassSectionAsync(academicYearId);

        var unique = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var createParentResponse = await _client.PostAsJsonAsync("/api/Parents", new
        {
            fullName = $"Integration Parent {unique}",
            nationalId = $"P-{unique}",
            phone = "7000001",
            relationship = "Guardian"
        });
        createParentResponse.EnsureSuccessStatusCode();
        using var createParentDoc = JsonDocument.Parse(await createParentResponse.Content.ReadAsStringAsync());
        var parentId = createParentDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32();

        var createStudentResponse = await _client.PostAsJsonAsync("/api/Students", new
        {
            fullName = $"Integration Student {unique}",
            indexNo = $"IT-{unique}",
            nationalId = $"S-{unique}",
            classSectionId,
            parents = new[]
            {
                new { parentId, isPrimary = true }
            }
        });
        createStudentResponse.EnsureSuccessStatusCode();
        using var createStudentDoc = JsonDocument.Parse(await createStudentResponse.Content.ReadAsStringAsync());

        return (
            createStudentDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32(),
            createStudentDoc.RootElement.GetProperty("data").GetProperty("fullName").GetString() ?? string.Empty
        );
    }

    private async Task<int> GetFirstParentIdAsync(int studentId)
    {
        var response = await _client.GetAsync($"/api/lookups/operations/parents?studentId={studentId}&take=1");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var parents = doc.RootElement.GetProperty("data");
        parents.GetArrayLength().Should().BeGreaterThan(0, "selected student should have at least one linked parent");
        return parents[0].GetProperty("id").GetInt32();
    }

    private async Task<int> CreateDistributionAsync(int academicYearId, int studentId, int parentId, int bookId)
    {
        var now = DateTime.UtcNow;
        var createResponse = await _client.PostAsJsonAsync("/api/Distributions", new
        {
            academicYearId,
            term = "Both",
            studentId,
            parentId,
            issuedDate = DateOnly.FromDateTime(now),
            issuedTime = TimeOnly.FromDateTime(now),
            notes = "Integration sample",
            items = new[]
            {
                new { bookId, quantity = 1 }
            }
        });
        createResponse.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("data").GetProperty("id").GetInt32();
    }

    private async Task FinalizeDistributionAsync(int id)
    {
        var response = await _client.PostAsync($"/api/Distributions/{id}/finalize", null);
        response.EnsureSuccessStatusCode();
    }

    private async Task CancelDistributionAsync(int id)
    {
        var response = await _client.DeleteAsync($"/api/Distributions/{id}");
        response.EnsureSuccessStatusCode();
    }

    private async Task<int> EnsureTeacherAsync()
    {
        var lookupResponse = await _client.GetAsync("/api/lookups/operations/teachers?take=1");
        lookupResponse.EnsureSuccessStatusCode();

        using var lookupDoc = JsonDocument.Parse(await lookupResponse.Content.ReadAsStringAsync());
        var teachers = lookupDoc.RootElement.GetProperty("data");
        if (teachers.GetArrayLength() > 0)
        {
            return teachers[0].GetProperty("id").GetInt32();
        }

        var unique = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var createResponse = await _client.PostAsJsonAsync("/api/Teachers", new
        {
            fullName = $"Sample Teacher {unique}",
            nationalId = $"T-{unique}",
            email = $"teacher{unique}@booksportal.local",
            phone = "7000000"
        });
        createResponse.EnsureSuccessStatusCode();

        using var createDoc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        return createDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32();
    }

    private async Task<int> EnsureSubjectAsync()
    {
        var subjectsResponse = await _client.GetAsync("/api/lookups/subjects");
        subjectsResponse.EnsureSuccessStatusCode();
        using var subjectDoc = JsonDocument.Parse(await subjectsResponse.Content.ReadAsStringAsync());
        var subjects = subjectDoc.RootElement.GetProperty("data");
        if (subjects.GetArrayLength() > 0)
        {
            return subjects[0].GetProperty("id").GetInt32();
        }

        var unique = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var createSubjectResponse = await _client.PostAsJsonAsync("/api/Subjects", new
        {
            name = $"Integration Subject {unique}",
            code = $"SUB{unique % 100000}"
        });
        createSubjectResponse.EnsureSuccessStatusCode();
        using var createSubjectDoc = JsonDocument.Parse(await createSubjectResponse.Content.ReadAsStringAsync());
        return createSubjectDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32();
    }

    private async Task<int> EnsureClassSectionAsync(int academicYearId)
    {
        var classSectionsResponse = await _client.GetAsync($"/api/lookups/class-sections?academicYearId={academicYearId}");
        classSectionsResponse.EnsureSuccessStatusCode();
        using var classSectionDoc = JsonDocument.Parse(await classSectionsResponse.Content.ReadAsStringAsync());
        var sections = classSectionDoc.RootElement.GetProperty("data");
        if (sections.GetArrayLength() > 0)
        {
            return sections[0].GetProperty("id").GetInt32();
        }

        var keystagesResponse = await _client.GetAsync("/api/lookups/keystages");
        keystagesResponse.EnsureSuccessStatusCode();
        using var keyDoc = JsonDocument.Parse(await keystagesResponse.Content.ReadAsStringAsync());
        var keystages = keyDoc.RootElement.GetProperty("data");
        keystages.GetArrayLength().Should().BeGreaterThan(0);
        var keystageId = keystages[0].GetProperty("id").GetInt32();

        var gradesResponse = await _client.GetAsync($"/api/lookups/grades?keystageId={keystageId}");
        gradesResponse.EnsureSuccessStatusCode();
        using var gradeDoc = JsonDocument.Parse(await gradesResponse.Content.ReadAsStringAsync());
        var grades = gradeDoc.RootElement.GetProperty("data");
        grades.GetArrayLength().Should().BeGreaterThan(0);
        var gradeId = grades[0].GetProperty("id").GetInt32();

        var createClassSectionResponse = await _client.PostAsJsonAsync("/api/ClassSections", new
        {
            academicYearId,
            keystageId,
            gradeId,
            section = "Z"
        });
        createClassSectionResponse.EnsureSuccessStatusCode();
        using var classCreateDoc = JsonDocument.Parse(await createClassSectionResponse.Content.ReadAsStringAsync());
        return classCreateDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32();
    }

    private async Task<int> CreateTeacherIssueAsync(int academicYearId, int teacherId, int bookId)
    {
        var now = DateTime.UtcNow;
        var createResponse = await _client.PostAsJsonAsync("/api/TeacherIssues", new
        {
            academicYearId,
            teacherId,
            issuedDate = DateOnly.FromDateTime(now),
            issuedTime = TimeOnly.FromDateTime(now),
            expectedReturnDate = DateTime.UtcNow.AddDays(7),
            notes = "Integration sample",
            items = new[]
            {
                new { bookId, quantity = 1 }
            }
        });
        createResponse.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("data").GetProperty("id").GetInt32();
    }

    private async Task FinalizeTeacherIssueAsync(int id)
    {
        var response = await _client.PostAsync($"/api/TeacherIssues/{id}/finalize", null);
        response.EnsureSuccessStatusCode();
    }

    private async Task CancelTeacherIssueAsync(int id)
    {
        var response = await _client.DeleteAsync($"/api/TeacherIssues/{id}");
        response.EnsureSuccessStatusCode();
    }

    private async Task<int> CreateReturnAsync(int academicYearId, int studentId, int parentId, int bookId)
    {
        var now = DateTime.UtcNow;
        var createResponse = await _client.PostAsJsonAsync("/api/Returns", new
        {
            academicYearId,
            studentId,
            returnedById = parentId,
            receivedDate = DateOnly.FromDateTime(now),
            receivedTime = TimeOnly.FromDateTime(now),
            notes = "Integration return sample",
            items = new[]
            {
                new
                {
                    bookId,
                    quantity = 1,
                    condition = BookCondition.Good,
                    conditionNotes = "Returned in good condition"
                }
            }
        });
        createResponse.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("data").GetProperty("id").GetInt32();
    }

    private async Task ProcessTeacherReturnAsync(int teacherIssueId)
    {
        var issueResponse = await _client.GetAsync($"/api/TeacherIssues/{teacherIssueId}");
        issueResponse.EnsureSuccessStatusCode();

        using var issueDoc = JsonDocument.Parse(await issueResponse.Content.ReadAsStringAsync());
        var items = issueDoc.RootElement.GetProperty("data").GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(0);

        var teacherIssueItemId = items[0].GetProperty("id").GetInt32();
        var now = DateTime.UtcNow;

        var returnResponse = await _client.PostAsJsonAsync($"/api/TeacherIssues/{teacherIssueId}/return", new
        {
            receivedDate = DateOnly.FromDateTime(now),
            receivedTime = TimeOnly.FromDateTime(now),
            notes = "Integration teacher return sample",
            items = new[]
            {
                new
                {
                    teacherIssueItemId,
                    quantity = 1
                }
            }
        });
        returnResponse.EnsureSuccessStatusCode();
    }

    private async Task DownloadPdfAsync(string route, string fileName)
    {
        var response = await _client.GetAsync(route);
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");

        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(0);

        var targetPath = Path.Combine(ArtifactDirectory, fileName);
        await File.WriteAllBytesAsync(targetPath, bytes);
    }

    private static void ResetArtifactsDirectory()
    {
        if (Directory.Exists(ArtifactDirectory))
        {
            Directory.Delete(ArtifactDirectory, recursive: true);
        }

        Directory.CreateDirectory(ArtifactDirectory);
    }
}
