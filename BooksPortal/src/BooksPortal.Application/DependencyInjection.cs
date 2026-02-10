using BooksPortal.Application.Common.Mappings;
using BooksPortal.Application.Features.AuditLogs.Interfaces;
using BooksPortal.Application.Features.AuditLogs.Services;
using BooksPortal.Application.Features.Books.Interfaces;
using BooksPortal.Application.Features.Books.Services;
using BooksPortal.Application.Features.BulkImport.Interfaces;
using BooksPortal.Application.Features.BulkImport.Services;
using BooksPortal.Application.Features.Distribution.Interfaces;
using BooksPortal.Application.Features.Distribution.Services;
using BooksPortal.Application.Features.Returns.Interfaces;
using BooksPortal.Application.Features.Returns.Services;
using BooksPortal.Application.Features.Reports.Interfaces;
using BooksPortal.Application.Features.Reports.Services;
using BooksPortal.Application.Features.TeacherIssues.Interfaces;
using BooksPortal.Application.Features.TeacherIssues.Services;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Application.Features.MasterData.Services;
using BooksPortal.Application.Features.Settings.Interfaces;
using BooksPortal.Application.Features.Settings.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BooksPortal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        MapsterConfig.Configure();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAcademicYearService, AcademicYearService>();
        services.AddScoped<IKeystageService, KeystageService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IClassSectionService, ClassSectionService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IParentService, ParentService>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<ILookupService, LookupService>();

        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IBookBulkImportService, BookBulkImportService>();
        services.AddScoped<ITeacherBulkImportService, TeacherBulkImportService>();
        services.AddScoped<IStudentBulkImportService, StudentBulkImportService>();
        services.AddScoped<IImportTemplateService, ImportTemplateService>();
        services.AddScoped<IDistributionService, DistributionService>();
        services.AddScoped<IReturnService, ReturnService>();
        services.AddScoped<ITeacherIssueService, TeacherIssueService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IReportService, ReportService>();

        services.AddScoped<IReferenceNumberFormatService, ReferenceNumberFormatService>();
        services.AddScoped<ISlipTemplateSettingService, SlipTemplateSettingService>();

        return services;
    }
}
