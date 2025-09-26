namespace ITTitans.Hackathon2025.Model;

public class JwtAppSettingsDto
{
    public required string Key { get; init; }
    
    public required string Issuer { get; init; }
    
    public required string Audience { get; init; }
    
    public required string Subject { get; init; }
}