using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace ITTitans.Hackathon2025.WebAPI;

public static class PolicyRegistration
{
    public static IServiceCollection AddHackathonPolicies(this IServiceCollection services)
    {
        AuthorizationBuilder authorizationBuilder = services.AddAuthorizationBuilder();
        
        AuthClaimType[] authClaimTypes = Enum.GetValues<AuthClaimType>();
        foreach (AuthClaimType authClaimType in authClaimTypes)
        {
            authorizationBuilder.AddPolicy(
                authClaimType.ToString(),
                policy =>
                {
                    policy.AddRequirements(new ClaimsAuthorizationRequirement(ClaimUtils.AuthClaimType, [authClaimType.ToString()]));
                }
            );
        }

        return services;
    }
}