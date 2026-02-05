using BooksPortal.Application.Common.Mappings;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BooksPortal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        MapsterConfig.Configure();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
