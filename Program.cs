using DbAutoChat.Win.Forms;
using DbAutoChat.Win.Interfaces;
using DbAutoChat.Win.Repositories;
using DbAutoChat.Win.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DbAutoChat.Win;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/dbautochat-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Console.WriteLine("Initializing application...");
            ApplicationConfiguration.Initialize();

            // Debug tests disabled for GUI mode
            // Tests can be run separately using dotnet test

            // Setup dependency injection
            Console.WriteLine("Setting up dependency injection...");
            var services = new ServiceCollection();
            ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            // Create and run the main form
            Console.WriteLine("Creating main form...");
            var mainForm = serviceProvider.GetRequiredService<MainForm>();
            Console.WriteLine("Starting application...");
            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });

        // Register services
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IConversationService, ConversationService>();
        services.AddSingleton<IDatabaseRepository, DatabaseRepository>();
        services.AddSingleton<ISqlValidator, SqlValidationPipeline>();
        services.AddSingleton<DatabaseConnectionManager>();

        // Register HTTP client for LLM providers
        services.AddHttpClient();

        // Register LLM providers
        services.AddTransient<OpenAiSqlGenerator>();
        services.AddTransient<GeminiSqlGenerator>();
        services.AddTransient<OllamaSqlGenerator>();
        services.AddTransient<SqlGeneratorFactory>();
        services.AddTransient<QueryOrchestrator>();

        // Register forms
        services.AddTransient<MainForm>();
        services.AddTransient<SettingsForm>();
    }
}