using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Infrastructure.Data;
using BooksPortal.Infrastructure.Identity;
using BooksPortal.Infrastructure.Repositories;
using BooksPortal.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;

namespace BooksPortal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<BooksPortalDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(BooksPortalDbContext).Assembly.FullName)));

        var dataProtectionBuilder = services.AddDataProtection();

        var keyStoragePath = configuration["DataProtection:KeyStoragePath"];
        if (!string.IsNullOrWhiteSpace(keyStoragePath))
        {
            dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(keyStoragePath));
        }

        var dataProtectionCertificate = LoadCertificateForDataProtection(configuration);
        if (dataProtectionCertificate is not null)
        {
            dataProtectionBuilder.ProtectKeysWithCertificate(dataProtectionCertificate);
        }

        services.AddIdentityCore<Staff>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = false;
            })
            .AddRoles<IdentityRole<int>>()
            .AddSignInManager()
            .AddEntityFrameworkStores<BooksPortalDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<BooksPortalDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IStaffDirectoryService, StaffDirectoryService>();
        services.AddScoped<ISetupIdentityService, SetupIdentityService>();
        services.AddScoped<IReferenceNumberService, ReferenceNumberService>();
        services.AddScoped<ISlipStorageService, SlipStorageService>();

        return services;
    }

    private static X509Certificate2? LoadCertificateForDataProtection(IConfiguration configuration)
    {
        var certBase64 = configuration["DataProtection:CertificateBase64"];
        var certPath = configuration["DataProtection:CertificatePath"];
        var certPassword = configuration["DataProtection:CertificatePassword"];

        if (string.IsNullOrWhiteSpace(certBase64) && string.IsNullOrWhiteSpace(certPath))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(certBase64))
        {
            var certBytes = Convert.FromBase64String(certBase64);
            return X509CertificateLoader.LoadPkcs12(
                certBytes,
                certPassword,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
        }

        var resolvedPath = certPath!;
        if (!Path.IsPathRooted(resolvedPath))
        {
            resolvedPath = Path.GetFullPath(resolvedPath);
        }

        if (!File.Exists(resolvedPath))
        {
            throw new InvalidOperationException($"DataProtection certificate file not found at path '{resolvedPath}'.");
        }

        return X509CertificateLoader.LoadPkcs12FromFile(
            resolvedPath,
            certPassword,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
    }
}
