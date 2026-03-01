using System.Security.Cryptography.X509Certificates;
using System.Text;
using BooksPortal.Application.Common.Models;
using Microsoft.IdentityModel.Tokens;

namespace BooksPortal.Infrastructure.Security;

public sealed class JwtSigningCredentialsProvider : IJwtSigningCredentialsProvider
{
    private readonly JwtSettings _settings;
    private readonly SecurityKey _validationKey;
    private readonly SigningCredentials _signingCredentials;

    public JwtSigningCredentialsProvider(JwtSettings settings)
    {
        _settings = settings;

        var mode = (_settings.SigningMode ?? string.Empty).Trim();
        if (string.Equals(mode, JwtSettings.CertificateMode, StringComparison.OrdinalIgnoreCase))
        {
            var certificate = LoadCertificate(_settings);
            if (!certificate.HasPrivateKey)
            {
                throw new InvalidOperationException("JWT certificate must include a private key.");
            }

            var key = new X509SecurityKey(certificate);
            _validationKey = key;
            _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.Secret))
        {
            throw new InvalidOperationException("JWT secret is required for symmetric signing mode.");
        }

        var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        _validationKey = symmetricKey;
        _signingCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
    }

    public SigningCredentials SigningCredentials => _signingCredentials;

    public SecurityKey ValidationKey => _validationKey;

    public string Algorithm => _signingCredentials.Algorithm;

    public TokenValidationParameters CreateTokenValidationParameters(bool validateLifetime)
    {
        var clockSkewSeconds = Math.Max(_settings.ClockSkewSeconds, 0);

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = validateLifetime,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _settings.Issuer,
            ValidAudience = _settings.Audience,
            IssuerSigningKey = _validationKey,
            ClockSkew = TimeSpan.FromSeconds(clockSkewSeconds),
        };
    }

    private static X509Certificate2 LoadCertificate(JwtSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.CertificateBase64))
        {
            var certBytes = Convert.FromBase64String(settings.CertificateBase64);
            return X509CertificateLoader.LoadPkcs12(
                certBytes,
                settings.CertificatePassword,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
        }

        if (!string.IsNullOrWhiteSpace(settings.CertificatePath))
        {
            var path = settings.CertificatePath;
            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(path);
            }

            if (!File.Exists(path))
            {
                throw new InvalidOperationException($"JWT certificate file not found at path '{path}'.");
            }

            return X509CertificateLoader.LoadPkcs12FromFile(
                path,
                settings.CertificatePassword,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
        }

        throw new InvalidOperationException(
            "JWT signing mode is Certificate, but no certificate source was configured. " +
            "Set JwtSettings:CertificateBase64 or JwtSettings:CertificatePath.");
    }
}
