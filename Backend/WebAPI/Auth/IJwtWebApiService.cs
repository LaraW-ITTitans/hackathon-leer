using ITTitans.Hackathon2025.EntityModel;
using ITTitans.Hackathon2025.EntityModel.Auth;
using System.Security.Claims;

namespace ITTitans.Hackathon2025.WebAPI.Auth;

public interface IJwtWebApiService
{
    Task<string> CreateToken(
        HackathonUserEntity user,
        ClaimsPrincipal userAsClaimsPrincipal,
        bool longLived,
        CancellationToken cancellationToken = default);
}