using System.Security.Cryptography;
using System.Text;
using DbAutoChat.Win.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DbAutoChat.Win.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly string _configPath;
    private Dictionary<string, object> _settings = new();

    public ConfigurationService()
    {
        _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        
        _configuration = builder.Build();
        LoadSettings();
    }

    public T GetSetting<T>(string key)
    {
        if (_settings.TryGetValue(key, out var value))
        {
            if (value is T directValue)
                return directValue;
            
            if (typeof(T) == typeof(string) && value is string stringValue)
            {
                // Decrypt if it's an encrypted API key
                if (key.Contains("ApiKey") && !string.IsNullOrEmpty(stringValue))
                {
                    try
                    {
                        var decrypted = DecryptString(stringValue);
                        return (T)(object)decrypted;
                    }
                    catch
                    {
                        // If decryption fails, return as-is (might be unencrypted)
                        return (T)(object)stringValue;
                    }
                }
            }
            
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value)) ?? default(T)!;
        }

        var configValue = _configuration[key];
        if (configValue != null)
        {
            if (typeof(T) == typeof(string))
                return (T)(object)configValue;
            
            return JsonConvert.DeserializeObject<T>(configValue) ?? default(T)!;
        }

        return default(T)!;
    }

    public void SetSetting<T>(string key, T value)
    {
        if (typeof(T) == typeof(string) && key.Contains("ApiKey") && value is string apiKey && !string.IsNullOrEmpty(apiKey))
        {
            // Encrypt API keys using DPAPI
            var encrypted = EncryptString(apiKey);
            _settings[key] = encrypted;
        }
        else
        {
            _settings[key] = value!;
        }
    }

    public async Task SaveAsync()
    {
        var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
        await File.WriteAllTextAsync(_configPath, json);
    }

    private void LoadSettings()
    {
        _settings = new Dictionary<string, object>();
        
        if (File.Exists(_configPath))
        {
            var json = File.ReadAllText(_configPath);
            var loaded = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            if (loaded != null)
            {
                _settings = loaded;
            }
        }
        else
        {
            // Create default configuration
            _settings = new Dictionary<string, object>
            {
                ["ConnectionStrings:Default"] = "Server=localhost;Database=SampleDB;Integrated Security=true;TrustServerCertificate=true;",
                ["Bot:MaxRows"] = 1000,
                ["Bot:Provider"] = "OpenAI",
                ["Bot:OpenAI:ApiKey"] = "",
                ["Bot:OpenAI:Model"] = "gpt-4",
                ["Bot:OpenAI:BaseUrl"] = "https://api.openai.com/v1",
                ["Bot:Gemini:ApiKey"] = "",
                ["Bot:Gemini:Model"] = "gemini-pro",
                ["Bot:Ollama:BaseUrl"] = "http://localhost:11434",
                ["Bot:Ollama:Model"] = "llama2",
                ["Logging:LogLevel:Default"] = "Information",
                ["Logging:LogLevel:Microsoft"] = "Warning",
                ["Logging:File:Path"] = "logs/dbautochat-.log",
                ["Logging:File:RollingInterval"] = "Day"
            };
        }
    }

    private string EncryptString(string plainText)
    {
        try
        {
            var data = Encoding.UTF8.GetBytes(plainText);
            var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }
        catch
        {
            // If encryption fails, return plain text (fallback)
            return plainText;
        }
    }

    private string DecryptString(string encryptedText)
    {
        try
        {
            var data = Convert.FromBase64String(encryptedText);
            var decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            // If decryption fails, return as-is
            return encryptedText;
        }
    }
}