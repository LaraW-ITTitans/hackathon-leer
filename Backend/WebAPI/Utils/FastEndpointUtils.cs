using ITTitans.Hackathon2025.Model;
using ITTitans.Hackathon2025.Model.Auth;

namespace ITTitans.Hackathon2025.WebAPI.Utils;

public static class FastEndpointUtils
{
    public static void AddHackathonPolicy<TRequest, TResponse>(this FastEndpoints.Endpoint<TRequest, TResponse> endpoint, AuthClaimType authClaimType)
        where TRequest : notnull
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        
        endpoint.Definition.Policies(authClaimType.ToString());
    }
}
