using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.Model;
using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.Service.Interfaces;
using ITTitans.Hackathon2025.Service.Interfaces.Settings;
using ITTitans.Hackathon2025.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ITTitans.Hackathon2025.WebAPI.Auth;

public class JwtWebApiService : IJwtWebApiService
{
    private static readonly TimeSpan TokenExpirationDuration = TimeSpan.FromDays(1);
    
    private readonly IWebServerAppSettingsService webServerAppSettingsService;
    private readonly IAuthClaimFactory authClaimFactory;
    private readonly HackathonDbContext hackathonDbContext;
    
    public JwtWebApiService(IWebServerAppSettingsService webServerAppSettingsService, IAuthClaimFactory authClaimFactory, HackathonDbContext hackathonDbContext)
    {
        this.webServerAppSettingsService = webServerAppSettingsService;
        this.authClaimFactory = authClaimFactory;
        this.hackathonDbContext = hackathonDbContext;
    }

    public async Task<string> CreateToken(
        HackathonUserEntity user,
        ClaimsPrincipal userAsClaimsPrincipal,
        CancellationToken cancellationToken = default)
    {
        JwtAppSettingsDto jwtSettings = this.webServerAppSettingsService.GetJwtSettings();

        IEnumerable<Claim> claims = await this.GenerateClaimsAsync(user, jwtSettings.Subject);
        DateTime expires = DateTime.Now.Add(TokenExpirationDuration);
        SigningCredentials signingCredentials = CreateSigningCredentials(jwtSettings.Key);

        var token = new JwtSecurityToken(
            jwtSettings.Issuer,
            jwtSettings.Audience,
            claims,
            expires: expires,
            signingCredentials: signingCredentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
    
    private async Task<List<Claim>> GenerateClaimsAsync(HackathonUserEntity user, string subject)
    {
        if (string.IsNullOrEmpty(user.UserName))
        {
            throw new ArgumentException("user name is not set", nameof(user));
        }

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, subject),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(HackathonClaims.UserIdClaimName, user.Id.ToString()),
            new(HackathonClaims.DisplayNameClaimName, user.DisplayName),
        ];
        
        IEnumerable<AuthClaimType> allClaims = await this.GetAuthClaimTypesOfUserAsync(user.Id, CancellationToken.None);
        var authClaims = new Claim(HackathonClaims.AuthClaimName, JsonSerializer.Serialize(allClaims));
        claims.AddRange(authClaims);

        return claims;
    }

    private static SigningCredentials CreateSigningCredentials(string key)
    {
        return new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
    }
    
    private async Task<IEnumerable<AuthClaimType>> GetAuthClaimTypesOfUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        List<Guid> roleIds = await this.hackathonDbContext.UserRoles
            .Where(entity => entity.UserId == userId)
            .Select(entity => entity.RoleId)
            .Distinct()
            .ToListAsync(cancellationToken);
        
        List<string> rawClaims = await this.hackathonDbContext.RoleClaims
            .Where(entity => roleIds.Contains(entity.RoleId))
            .Where(entity => entity.ClaimType == ClaimUtils.AuthClaimType)
            .Where(entity => entity.ClaimValue != null)
            .Select(entity => entity.ClaimValue!)
            .ToListAsync(cancellationToken);
        
        return rawClaims
            .Distinct()
            .Select(claim => this.authClaimFactory.Extract(claim))
            .Where(authClaimType => authClaimType.HasValue)
            .Select(authClaimType => authClaimType!.Value)
            .ToList();
    }
}