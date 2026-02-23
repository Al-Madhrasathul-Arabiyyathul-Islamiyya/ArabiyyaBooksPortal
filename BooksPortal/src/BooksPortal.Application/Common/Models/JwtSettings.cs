namespace BooksPortal.Application.Common.Models;

public class JwtSettings
{
    public const string SymmetricMode = "Symmetric";
    public const string CertificateMode = "Certificate";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryInMinutes { get; set; }
    public int RefreshTokenExpiryInDays { get; set; }
    public int ClockSkewSeconds { get; set; } = 30;
    public string SigningMode { get; set; } = SymmetricMode;
    public string? CertificatePath { get; set; }
    public string? CertificatePassword { get; set; }
    public string? CertificateBase64 { get; set; }
}
