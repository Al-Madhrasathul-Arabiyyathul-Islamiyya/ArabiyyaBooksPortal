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

public partial class Program { }
