using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BooksPortal.IntegrationTests;

[Collection("Integration API")]
public class RbacAndHierarchyBulkIntegrationTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RbacAndHierarchyBulkIntegrationTests(IntegrationTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Admin_Cannot_CreateUser_Or_MutateReferenceFormat()
    {
        await AuthenticateAsync("opsadmin@booksportal.local", "Admin@123456");

        var createUserResponse = await _client.PostAsJsonAsync("/api/users", new
        {
            userName = $"blocked-admin-{Guid.NewGuid():N}"[..18],
            email = $"blocked-admin-{Guid.NewGuid():N}@booksportal.local",
            password = "Admin@123456",
            fullName = "Blocked Admin",
            roles = new[] { "User" }
        });

        createUserResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var mutateReferenceFormatResponse = await _client.PostAsJsonAsync("/api/ReferenceNumberFormats", new
        {
            slipType = 1,
            academicYearId = 1,
            formatTemplate = "DST{year}{autonum}",
            paddingWidth = 6
        });

        mutateReferenceFormatResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SuperAdmin_Can_DownloadHierarchyTemplate_And_UpsertHierarchy()
    {
        await AuthenticateAsync(TestSettings.Current.AdminEmail, TestSettings.Current.AdminPassword);

        var templateResponse = await _client.GetAsync("/api/import-templates/master-data-hierarchy");
        templateResponse.EnsureSuccessStatusCode();
        templateResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var templateJson = await templateResponse.Content.ReadAsStringAsync();
        using var templateDoc = JsonDocument.Parse(templateJson);
        templateDoc.RootElement.TryGetProperty("AcademicYears", out _).Should().BeTrue();

        var year = 2600 + DateTime.UtcNow.Second;
        var payload = new
        {
            academicYears = new[]
            {
                new
                {
                    name = $"AY {year}",
                    year,
                    isActive = false,
                    keystages = new[]
                    {
                        new
                        {
                            name = $"KS {year}",
                            code = $"KS{year}",
                            sortOrder = 99,
                            grades = new[]
                            {
                                new
                                {
                                    name = $"Grade {year}",
                                    code = $"G{year}",
                                    sortOrder = 99,
                                    classes = new[]
                                    {
                                        new { section = "A" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        var upsertResponse = await _client.PostAsJsonAsync("/api/master-data/hierarchy/bulk/upsert", payload);
        upsertResponse.EnsureSuccessStatusCode();

        using var upsertDoc = JsonDocument.Parse(await upsertResponse.Content.ReadAsStringAsync());
        var data = upsertDoc.RootElement.GetProperty("data");
        data.GetProperty("createdCount").GetInt32().Should().BeGreaterThan(0);
        data.GetProperty("results").EnumerateArray().Any().Should().BeTrue();
    }

    [Fact]
    public async Task SuperAdmin_HierarchyBulk_Rejects_When_AcademicYear_Already_Exists()
    {
        await AuthenticateAsync(TestSettings.Current.AdminEmail, TestSettings.Current.AdminPassword);

        var activeYearResponse = await _client.GetAsync("/api/AcademicYears/active");
        activeYearResponse.EnsureSuccessStatusCode();
        using var activeYearDoc = JsonDocument.Parse(await activeYearResponse.Content.ReadAsStringAsync());
        var existingYear = activeYearDoc.RootElement.GetProperty("data").GetProperty("year").GetInt32();

        var payload = new
        {
            academicYears = new[]
            {
                BuildAcademicYearNode(existingYear, false, "dup")
            }
        };

        var response = await _client.PostAsJsonAsync("/api/master-data/hierarchy/bulk/upsert", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("already exist");
    }

    [Fact]
    public async Task SuperAdmin_HierarchyBulk_Rejects_When_Multiple_AcademicYears_Are_Marked_Active()
    {
        await AuthenticateAsync(TestSettings.Current.AdminEmail, TestSettings.Current.AdminPassword);

        var baseYear = 3300 + DateTime.UtcNow.Second;
        var payload = new
        {
            academicYears = new[]
            {
                BuildAcademicYearNode(baseYear, true, "m1"),
                BuildAcademicYearNode(baseYear + 1, true, "m2")
            }
        };

        var response = await _client.PostAsJsonAsync("/api/master-data/hierarchy/bulk/upsert", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Only one academic year can be marked active");
    }

    [Fact]
    public async Task SuperAdmin_HierarchyBulk_Activates_Only_One_AcademicYear()
    {
        await AuthenticateAsync(TestSettings.Current.AdminEmail, TestSettings.Current.AdminPassword);

        var baseYear = 3600 + DateTime.UtcNow.Second;
        var activeYear = baseYear;
        var payload = new
        {
            academicYears = new[]
            {
                BuildAcademicYearNode(activeYear, true, "active"),
                BuildAcademicYearNode(baseYear + 1, false, "inactive")
            }
        };

        var upsertResponse = await _client.PostAsJsonAsync("/api/master-data/hierarchy/bulk/upsert", payload);
        var upsertBody = await upsertResponse.Content.ReadAsStringAsync();
        upsertResponse.StatusCode.Should().Be(HttpStatusCode.OK, upsertBody);

        var yearsResponse = await _client.GetAsync("/api/AcademicYears");
        yearsResponse.EnsureSuccessStatusCode();
        using var yearsDoc = JsonDocument.Parse(await yearsResponse.Content.ReadAsStringAsync());
        var years = yearsDoc.RootElement.GetProperty("data").EnumerateArray().ToList();

        var activeYears = years.Where(x => x.GetProperty("isActive").GetBoolean()).ToList();
        activeYears.Should().HaveCount(1);
        activeYears[0].GetProperty("year").GetInt32().Should().Be(activeYear);
    }

    [Fact]
    public async Task Admin_Can_ReadReferenceFormats_But_Cannot_Mutate()
    {
        await AuthenticateAsync("opsadmin@booksportal.local", "Admin@123456");

        var readResponse = await _client.GetAsync("/api/ReferenceNumberFormats");
        readResponse.EnsureSuccessStatusCode();

        var deleteResponse = await _client.DeleteAsync("/api/ReferenceNumberFormats/1");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task AuthenticateAsync(string email, string password)
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password
        });
        loginResponse.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
        token.Should().NotBeNullOrWhiteSpace();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static object BuildAcademicYearNode(int year, bool isActive, string suffix)
    {
        var suffixToken = string.IsNullOrWhiteSpace(suffix)
            ? "X"
            : suffix.Trim()[0].ToString().ToUpperInvariant();
        var yearToken = (year % 1000).ToString("D3");
        var keystageCode = $"K{yearToken}{suffixToken}";
        var gradeCode = $"G{yearToken}{suffixToken}";

        return new
        {
            name = $"AY {year} {suffix}",
            year,
            isActive,
            keystages = new[]
            {
                new
                {
                    name = $"KS {year} {suffix}",
                    code = keystageCode,
                    sortOrder = 1,
                    grades = new[]
                    {
                        new
                        {
                            name = $"Grade {year} {suffix}",
                            code = gradeCode,
                            sortOrder = 1,
                            classes = new[]
                            {
                                new { section = "A" }
                            }
                        }
                    }
                }
            }
        };
    }
}
