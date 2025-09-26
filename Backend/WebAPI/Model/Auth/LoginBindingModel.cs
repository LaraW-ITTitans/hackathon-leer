namespace ITTitans.Hackathon2025.WebAPI.Model.Auth;

public class LoginBindingModel
{
    public required string UserName { get; init; }
    
    public required string Password { get; init; }
    
    // If true, the JWT validity will be 30 days; otherwise, 1 day. Defaults to false when omitted.
    public bool RememberMe { get; init; } = false;
}