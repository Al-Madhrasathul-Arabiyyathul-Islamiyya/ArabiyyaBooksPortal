using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.Auth.DTOs;
using BooksPortal.Infrastructure.Data;
using BooksPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BooksPortal.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<Staff> _userManager;
    private readonly SignInManager<Staff> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly BooksPortalDbContext _context;

    public AuthService(
        UserManager<Staff> userManager,
        SignInManager<Staff> signInManager,
        IOptions<JwtSettings> jwtSettings,
        BooksPortalDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings.Value;
        _context = context;
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new BusinessRuleException("Invalid email or password.");

        if (!user.IsActive)
            throw new BusinessRuleException("Account is deactivated.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            throw new BusinessRuleException("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = GenerateJwtToken(user, roles);
        var refreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            StaffId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt
        };
    }

    public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = GetPrincipalFromExpiredToken(request.AccessToken)
            ?? throw new BusinessRuleException("Invalid access token.");

        var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new BusinessRuleException("Invalid access token.");

        var userId = int.Parse(userIdClaim);

        var existingToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken && r.StaffId == userId)
            ?? throw new BusinessRuleException("Invalid refresh token.");

        if (!existingToken.IsActive)
            throw new BusinessRuleException("Refresh token is expired or revoked.");

        existingToken.RevokedAt = DateTime.UtcNow;

        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("Staff", userId);

        if (!user.IsActive)
            throw new BusinessRuleException("Account is deactivated.");

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = GenerateJwtToken(user, roles);
        var newRefreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

        existingToken.ReplacedByToken = newRefreshToken;

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            StaffId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt
        };
    }

    public async Task LogoutAsync(int userId)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(r => r.StaffId == userId && r.RevokedAt == null)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<UserProfileResponse> GetCurrentUserAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("Staff", userId);

        var roles = await _userManager.GetRolesAsync(user);

        return new UserProfileResponse
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            NationalId = user.NationalId,
            Designation = user.Designation,
            Roles = roles.ToList()
        };
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("Staff", userId);

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BusinessRuleException(errors);
        }
    }

    private string GenerateJwtToken(Staff user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            return null;
        }

        return principal;
    }
}
