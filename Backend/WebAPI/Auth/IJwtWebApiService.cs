using ITTitans.Hackathon2025.EntityModel;
using System.Security.Claims;

namespace ITTitans.Hackathon2025.WebAPI.Auth;

public interface IJwtWebApiService
{
    Task<string> CreateToken(
        HackathonUserEntity user,
        ClaimsPrincipal userAsClaimsPrincipal,
        CancellationToken cancellationToken = default);
}