using Microsoft.IdentityModel.Tokens;

namespace BooksPortal.Infrastructure.Security;

public interface IJwtSigningCredentialsProvider
{
    SigningCredentials SigningCredentials { get; }
    SecurityKey ValidationKey { get; }
    string Algorithm { get; }
    TokenValidationParameters CreateTokenValidationParameters(bool validateLifetime);
}
