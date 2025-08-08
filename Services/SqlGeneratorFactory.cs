using DbAutoChat.Win.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DbAutoChat.Win.Services;

public class SqlGeneratorFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfigurationService _configService;
    private readonly ILoggerFactory _loggerFactory;

    public SqlGeneratorFactory(
        IServiceProvider serviceProvider,
        IConfigurationService configService,
        ILoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _configService = configService;
        _loggerFactory = loggerFactory;
    }

    public ISqlGenerator CreateSqlGenerator()
    {
        var provider = _configService.GetSetting<string>("Bot:Provider") ?? "OpenAI";
        
        var logger = _loggerFactory.CreateLogger<SqlGeneratorFactory>();
        logger.LogInformation("Creating SQL generator for provider: {Provider}", provider);

        return provider.ToUpperInvariant() switch
        {
            "OPENAI" => CreateWithRetry(() => _serviceProvider.GetService<OpenAiSqlGenerator>()),
            "GEMINI" => CreateWithRetry(() => _serviceProvider.GetService<GeminiSqlGenerator>()),
            "OLLAMA" => CreateWithRetry(() => _serviceProvider.GetService<OllamaSqlGenerator>()),
            _ => throw new ArgumentException($"Unknown LLM provider: {provider}")
        };
    }

    private ISqlGenerator CreateWithRetry<T>(Func<T?> factory) where T : class, ISqlGenerator
    {
        var generator = factory();
        if (generator == null)
        {
            throw new InvalidOperationException($"Failed to create SQL generator of type {typeof(T).Name}");
        }

        return new RetryingSqlGenerator(generator, _loggerFactory.CreateLogger<RetryingSqlGenerator>());
    }
}

public class RetryingSqlGenerator : ISqlGenerator
{
    private readonly ISqlGenerator _innerGenerator;
    private readonly ILogger<RetryingSqlGenerator> _logger;
    private const int MaxRetries = 3;
    private readonly TimeSpan[] RetryDelays = { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4) };

    public RetryingSqlGenerator(ISqlGenerator innerGenerator, ILogger<RetryingSqlGenerator> logger)
    {
        _innerGenerator = innerGenerator;
        _logger = logger;
    }

    public async Task<Models.SqlGenerationResult> GenerateSqlAsync(
        string naturalLanguageQuery, 
        Models.DatabaseSchema schema, 
        Models.ConversationContext context)
    {
        Exception? lastException = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                if (attempt > 0)
                {
                    _logger.LogWarning("Retrying SQL generation, attempt {Attempt}/{MaxAttempts}", attempt + 1, MaxRetries + 1);
                    await Task.Delay(RetryDelays[attempt - 1]);
                }

                return await _innerGenerator.GenerateSqlAsync(naturalLanguageQuery, schema, context);
            }
            catch (HttpRequestException ex) when (attempt < MaxRetries)
            {
                lastException = ex;
                _logger.LogWarning(ex, "HTTP error during SQL generation, will retry. Attempt {Attempt}", attempt + 1);
            }
            catch (TaskCanceledException ex) when (attempt < MaxRetries)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Timeout during SQL generation, will retry. Attempt {Attempt}", attempt + 1);
            }
            catch (Models.LlmProviderException ex) when (attempt < MaxRetries && IsRetryableError(ex))
            {
                lastException = ex;
                _logger.LogWarning(ex, "Retryable LLM provider error, will retry. Attempt {Attempt}", attempt + 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Non-retryable error during SQL generation");
                throw;
            }
        }

        _logger.LogError(lastException, "All retry attempts failed for SQL generation");
        throw new Models.LlmProviderException("Failed to generate SQL after multiple attempts", lastException!);
    }

    private static bool IsRetryableError(Models.LlmProviderException ex)
    {
        // Check if the error message indicates a retryable condition
        var message = ex.Message.ToLowerInvariant();
        return message.Contains("rate limit") ||
               message.Contains("timeout") ||
               message.Contains("service unavailable") ||
               message.Contains("internal server error") ||
               message.Contains("502") ||
               message.Contains("503") ||
               message.Contains("504");
    }
}