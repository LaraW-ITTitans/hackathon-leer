using ITTitans.Hackathon2025.Model;

namespace ITTitans.Hackathon2025.Service.Interfaces.Settings;

public interface IWebServerAppSettingsService
{
    Uri GetFrontendUri();

    JwtAppSettingsDto GetJwtSettings();

    string GetPredefinedAdminUserPassword();

    string GetHackathonDbContextConnectionString();
    
    string GetGoogleAiApiKey();
}