using ITTitans.Hackathon2025.Model.Auth;
using Microsoft.AspNetCore.Authorization;

namespace ITTitans.Hackathon2025.WebAPI.Auth;

public class OrClaimsRequirementAuthorizationHandler : AuthorizationHandler<OrClaimsRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OrClaimsRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);
        
        foreach (AuthClaimType authClaim in requirement.AuthClaims)
        {
            bool isClaimAssignedToUser = context.User.HasClaim(claim => claim.Type == HackathonClaims.AuthClaimName && claim.Value == authClaim.ToString());
            if (isClaimAssignedToUser)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }
        
        return Task.CompletedTask;
    }
}
