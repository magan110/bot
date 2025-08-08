namespace DbAutoChat.Win.Interfaces;

public interface IConfigurationService
{
    T GetSetting<T>(string key);
    void SetSetting<T>(string key, T value);
    Task SaveAsync();
}