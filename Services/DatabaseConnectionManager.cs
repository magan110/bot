using DbAutoChat.Win.Interfaces;
using Microsoft.Extensions.Logging;

namespace DbAutoChat.Win.Services;

public class DatabaseConnectionManager
{
    private readonly IConfigurationService _configService;
    private readonly ILogger<DatabaseConnectionManager> _logger;

    public DatabaseConnectionManager(IConfigurationService configService, ILogger<DatabaseConnectionManager> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    public async Task<bool> SwitchToDatabase(string databaseName)
    {
        try
        {
            var connectionKey = $"ConnectionStrings:{databaseName}";
            var connectionString = _configService.GetSetting<string>(connectionKey);

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogWarning("Connection string not found for database: {DatabaseName}", databaseName);
                return false;
            }

            // Update the Default connection string to point to the selected database
            _configService.SetSetting("ConnectionStrings:Default", connectionString);
            await _configService.SaveAsync();

            _logger.LogInformation("Switched to database: {DatabaseName}", databaseName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to switch to database: {DatabaseName}", databaseName);
            return false;
        }
    }

    public List<string> GetAvailableDatabases()
    {
        var databases = new List<string>();

        // Check for each configured connection string
        var connectionStrings = new[] { "bwlive", "itkHaria", "imageData" };

        foreach (var dbName in connectionStrings)
        {
            var connectionString = _configService.GetSetting<string>($"ConnectionStrings:{dbName}");
            if (!string.IsNullOrEmpty(connectionString))
            {
                databases.Add(dbName);
            }
        }

        return databases;
    }

    public string GetCurrentDatabase()
    {
        var defaultConnection = _configService.GetSetting<string>("ConnectionStrings:Default");

        // Try to match with known connection strings
        var connectionStrings = new[] { "bwlive", "itkHaria", "imageData" };

        foreach (var dbName in connectionStrings)
        {
            var connectionString = _configService.GetSetting<string>($"ConnectionStrings:{dbName}");
            if (connectionString == defaultConnection)
            {
                return dbName;
            }
        }

        return "Default (Local)";
    }
}