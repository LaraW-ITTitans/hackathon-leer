using ITTitans.Hackathon2025.Service.Interfaces.Settings;
using Microsoft.Extensions.Configuration;

namespace ITTitans.Hackathon2025.Service.Settings;

public class AppSettingsReader : IAppSettingsReader
{
    private readonly IConfiguration configuration;

    public AppSettingsReader(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    /// <inheritdoc />
    public string GetValueAsString(string key)
    {
        string configValue = this.configuration[key] ?? throw new InvalidOperationException($"the key {key} is not available in the app settings");
        return configValue;
    }

    /// <inheritdoc />
    public int GetValueAsInt(string key)
    {
        string configValueAsString = this.GetValueAsString(key);
        if (!int.TryParse(configValueAsString, out int configValueAsInt))
        {
            throw new InvalidOperationException($"""the value "{configValueAsInt}" of the app settings key {key} cannot be converted to a int""");
        }

        return configValueAsInt;
    }

    /// <inheritdoc />
    public bool GetValueAsBool(string key)
    {
        string configValueAsString = this.GetValueAsString(key);
        if (!bool.TryParse(configValueAsString, out bool configValueAsBool))
        {
            throw new InvalidOperationException($"""the value "{configValueAsBool}" of the app settings key {key} cannot be converted to a bool""");
        }

        return configValueAsBool;
    }
}