using ITTitans.Hackathon2025.Model;
using ITTitans.Hackathon2025.Service.Interfaces.Settings;

namespace ITTitans.Hackathon2025.Service.Settings;

public class WebServerAppSettingsService : IWebServerAppSettingsService
{
    private readonly IAppSettingsReader appSettingsReader;

    public WebServerAppSettingsService(IAppSettingsReader appSettingsReader)
    {
        this.appSettingsReader = appSettingsReader;
    }

    /// <inheritdoc />
    public Uri GetFrontendUri()
    {
        string frontendUriAsString = this.appSettingsReader.GetValueAsString("FrontendUri");
        return new Uri(frontendUriAsString);
    }

    /// <inheritdoc />
    public JwtAppSettingsDto GetJwtSettings()
    {
        string key = this.appSettingsReader.GetValueAsString("JWT:Key");
        string issuer = this.appSettingsReader.GetValueAsString("JWT:Issuer");
        string audience = this.appSettingsReader.GetValueAsString("JWT:Audience");
        string subject = this.appSettingsReader.GetValueAsString("JWT:Subject");

        return new JwtAppSettingsDto
        {
            Key = key,
            Issuer = issuer,
            Audience = audience,
            Subject = subject,
        };
    }

    /// <inheritdoc />
    public string GetPredefinedAdminUserPassword()
    {
        return this.appSettingsReader.GetValueAsString("PredefinedAdminUserPassword");
    }

    /// <inheritdoc />
    public string GetHackathonDbContextConnectionString()
    {
        return this.appSettingsReader.GetValueAsString("ConnectionStrings:HackathonDbContext");
    }
    
    /// <inheritdoc />
    public string GetGoogleAiApiKey()
    {
        return this.appSettingsReader.GetValueAsString("GoogleAI:ApiKey");
    }
}