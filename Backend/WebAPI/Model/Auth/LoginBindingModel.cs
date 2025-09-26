namespace ITTitans.Hackathon2025.WebAPI.Model.Auth;

public class LoginBindingModel
{
    public required string UserName { get; init; }
    
    public required string Password { get; init; }
    
    public required bool RememberMe { get; init; }
}