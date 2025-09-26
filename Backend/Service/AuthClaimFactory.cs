using ITTitans.Hackathon2025.Model.Auth;
using ITTitans.Hackathon2025.Service.Interfaces;
using ITTitans.Hackathon2025.Utils;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ITTitans.Hackathon2025.Service;

public class AuthClaimFactory : IAuthClaimFactory
{
    private readonly ILogger<AuthClaimFactory> logger;

    public AuthClaimFactory(ILogger<AuthClaimFactory> logger)
    {
        this.logger = logger;
    }
    
    /// <inheritdoc />
    public Claim BuildClaim(AuthClaimType authClaimType)
    {
        return new Claim(ClaimUtils.AuthClaimType, authClaimType.ToString());
    }

    /// <inheritdoc />
    public AuthClaimType? Extract(Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);

        return claim.Type is ClaimUtils.AuthClaimType ? this.Extract(claim.Value) : null;
    }

    /// <inheritdoc />
    public AuthClaimType? Extract(string claimValue)
    {
        if (!Enum.TryParse(claimValue, out AuthClaimType authClaimType))
        {
            this.logger.LogCannotExtractAuthClaimFromClaimValue(claimValue);
            return null;
        }

        return authClaimType;
    }

    /// <inheritdoc />
    public string GetClaimType()
    {
        return ClaimUtils.AuthClaimType;
    }
}

internal static partial class LoggerMessageDefinitions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = """Cannot extract auth claim type from claim value "{claimValue}".""", SkipEnabledCheck = true)]
    public static partial void LogCannotExtractAuthClaimFromClaimValue(this ILogger<AuthClaimFactory> logger, string claimValue);
}
