using BooksPortal.API.Filters;
using BooksPortal.API.Middleware;
using BooksPortal.API.Services;
using BooksPortal.Application;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Common.Models;
using BooksPortal.Infrastructure;
using BooksPortal.Infrastructure.Data;
using BooksPortal.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Events;
using System.Text.Json.Serialization;

QuestPDF.Settings.License = LicenseType.Community;

// Register Faruma font for Thaana text support
var farumaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Faruma.ttf");
if (File.Exists(farumaPath))
{
    using var fontStream = File.OpenRead(farumaPath);
    QuestPDF.Drawing.FontManager.RegisterFont(fontStream);
}

var builder = WebApplication.CreateBuilder(args);

ValidateRequiredProductionConfiguration(builder.Configuration, builder.Environment);

// Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Application & Infrastructure services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddSingleton<BookBulkImportJobStore>();
builder.Services.Configure<ImportTemplateCacheOptions>(
    builder.Configuration.GetSection(ImportTemplateCacheOptions.SectionName));
builder.Services.AddScoped<ImportTemplateCacheService>();

// JWT Settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSigningCredentialsProvider = new JwtSigningCredentialsProvider(jwtSettings);
builder.Services.AddSingleton<IJwtSigningCredentialsProvider>(jwtSigningCredentialsProvider);

// Authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = jwtSigningCredentialsProvider.CreateTokenValidationParameters(validateLifetime: true);
    });

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:3000"];

    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BooksPortalDbContext>();
    await dbContext.Database.MigrateAsync();
    await SeedData.SeedAsync(scope.ServiceProvider, app.Environment.IsDevelopment());

    var templateCache = scope.ServiceProvider.GetRequiredService<ImportTemplateCacheService>();
    await templateCache.InitializeAsync();
}

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = static (httpContext, _, exception) =>
    {
        if (exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
            return LogEventLevel.Error;

        if (httpContext.Response.StatusCode >= StatusCodes.Status400BadRequest)
            return LogEventLevel.Warning;

        return LogEventLevel.Information;
    };
    options.EnrichDiagnosticContext = static (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty);

        var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        diagnosticContext.Set("UserId", userId ?? string.Empty);
    };
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();

static void ValidateRequiredProductionConfiguration(IConfiguration configuration, IWebHostEnvironment environment)
{
    if (environment.IsDevelopment())
    {
        return;
    }

    static string Require(IConfiguration config, string key)
    {
        var value = config[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required configuration key: {key}");
        }

        return value;
    }

    Require(configuration, "ConnectionStrings:DefaultConnection");

    var signingMode = Require(configuration, "JwtSettings:SigningMode");
    if (string.Equals(signingMode, JwtSettings.SymmetricMode, StringComparison.OrdinalIgnoreCase))
    {
        Require(configuration, "JwtSettings:Secret");
    }
    else if (string.Equals(signingMode, JwtSettings.CertificateMode, StringComparison.OrdinalIgnoreCase))
    {
        var hasCertificatePath = !string.IsNullOrWhiteSpace(configuration["JwtSettings:CertificatePath"]);
        var hasCertificateBase64 = !string.IsNullOrWhiteSpace(configuration["JwtSettings:CertificateBase64"]);
        if (!hasCertificatePath && !hasCertificateBase64)
        {
            throw new InvalidOperationException(
                "JwtSettings in Certificate mode requires either JwtSettings:CertificatePath or JwtSettings:CertificateBase64.");
        }
    }

    Require(configuration, "SuperAdminSeed:UserName");
    Require(configuration, "SuperAdminSeed:Email");
    Require(configuration, "SuperAdminSeed:Password");
    Require(configuration, "SuperAdminSeed:FullName");
    Require(configuration, "SuperAdminSeed:NationalId");
    Require(configuration, "SuperAdminSeed:Designation");
}

public partial class Program { }
