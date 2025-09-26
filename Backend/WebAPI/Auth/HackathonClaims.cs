using System.Security.Claims;

namespace ITTitans.Hackathon2025.WebAPI.Auth;

public class HackathonClaims
{
    public const string UserIdClaimName = "Hackathon-User-ID";
    public const string UsernameClaimName = ClaimTypes.Name;
    public const string AuthClaimName = "Hackathon-Auth";
    public const string DisplayNameClaimName = "Hackathon-Display-Name";
}