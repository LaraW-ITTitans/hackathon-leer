using ITTitans.Hackathon2025.Model.Auth;
using System.Security.Claims;

namespace ITTitans.Hackathon2025.Service.Interfaces;

public interface IAuthClaimFactory
{
    Claim BuildClaim(AuthClaimType siGeKoAuthClaimType);
    
    AuthClaimType? Extract(Claim claim);
    
    AuthClaimType? Extract(string claimValue);
    
    string GetClaimType();
}