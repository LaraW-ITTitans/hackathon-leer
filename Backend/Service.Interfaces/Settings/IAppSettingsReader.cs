namespace ITTitans.Hackathon2025.Service.Interfaces.Settings;

public interface IAppSettingsReader
{
    string GetValueAsString(string key);
    
    int GetValueAsInt(string key);
    
    bool GetValueAsBool(string key);
}
