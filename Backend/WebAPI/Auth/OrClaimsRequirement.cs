using ITTitans.Hackathon2025.Model.Auth;
using Microsoft.AspNetCore.Authorization;

namespace ITTitans.Hackathon2025.WebAPI.Auth;

public class OrClaimsRequirement : IAuthorizationRequirement
{
    public OrClaimsRequirement(params AuthClaimType[] authClaims)
    {
        this.AuthClaims = authClaims;
    }
    
    public IEnumerable<AuthClaimType> AuthClaims { get; }
}
