using BooksPortal.Application.Common.Mappings;
using BooksPortal.Application.Features.Books.Interfaces;
using BooksPortal.Application.Features.Books.Services;
using BooksPortal.Application.Features.MasterData.Interfaces;
using BooksPortal.Application.Features.MasterData.Services;
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

        return services;
    }
}
