using System.Data;
using System.Diagnostics;
using DbAutoChat.Win.Interfaces;
using DbAutoChat.Win.Models;
using Microsoft.Extensions.Logging;

namespace DbAutoChat.Win.Services;

public class QueryOrchestrator
{
    private readonly SqlGeneratorFactory _sqlGeneratorFactory;
    private readonly ISqlValidator _sqlValidator;
    private readonly IDatabaseRepository _databaseRepository;
    private readonly IConfigurationService _configService;
    private readonly ILogger<QueryOrchestrator> _logger;

    public QueryOrchestrator(
        SqlGeneratorFactory sqlGeneratorFactory,
        ISqlValidator sqlValidator,
        IDatabaseRepository databaseRepository,
        IConfigurationService configService,
        ILogger<QueryOrchestrator> logger)
    {
        _sqlGeneratorFactory = sqlGeneratorFactory;
        _sqlValidator = sqlValidator;
        _databaseRepository = databaseRepository;
        _configService = configService;
        _logger = logger;
    }

    public async Task<QueryExecutionResult> ProcessQueryAsync(
        string naturalLanguageQuery,
        ConversationContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Processing query: {Query}", naturalLanguageQuery);

            // Step 1: Get database schema
            var schema = await _databaseRepository.IntrospectSchemaAsync();
            
            // Step 2: Generate SQL using LLM
            var sqlGenerator = _sqlGeneratorFactory.CreateSqlGenerator();
            var generationResult = await sqlGenerator.GenerateSqlAsync(
                naturalLanguageQuery, schema, context);

            _logger.LogInformation("Generated SQL: {Sql}", generationResult.GeneratedSql);

            // Step 3: Check if clarification is needed
            if (generationResult.RequiresClarification)
            {
                return new QueryExecutionResult
                {
                    Success = false,
                    RequiresClarification = true,
                    ClarificationQuestion = generationResult.ClarificationQuestion,
                    Language = generationResult.Language,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };
            }

            // Step 4: Validate the generated SQL
            var validationResult = _sqlValidator.ValidateQuery(generationResult.GeneratedSql, schema);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("SQL validation failed: {Error}", validationResult.ErrorMessage);
                return new QueryExecutionResult
                {
                    Success = false,
                    ErrorMessage = validationResult.ErrorMessage,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };
            }

            // Step 5: Ensure TOP clause is present
            var finalSql = EnsureTopClause(generationResult.GeneratedSql);

            // Step 6: Validate query structure with database
            var isStructureValid = await _databaseRepository.ValidateQueryStructureAsync(finalSql);
            if (!isStructureValid)
            {
                _logger.LogWarning("SQL structure validation failed for: {Sql}", finalSql);
                return new QueryExecutionResult
                {
                    Success = false,
                    ErrorMessage = "The generated SQL query has structural issues and cannot be executed safely.",
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };
            }

            // Step 7: Execute the query
            var results = await _databaseRepository.ExecuteQueryAsync(finalSql, generationResult.Parameters);
            
            stopwatch.Stop();
            
            _logger.LogInformation("Query executed successfully. Rows returned: {RowCount}, Execution time: {ExecutionTime}ms", 
                results.Rows.Count, stopwatch.ElapsedMilliseconds);

            return new QueryExecutionResult
            {
                Success = true,
                Results = results,
                GeneratedSql = finalSql,
                Parameters = generationResult.Parameters,
                Language = generationResult.Language,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                RowCount = results.Rows.Count
            };
        }
        catch (LlmProviderException ex)
        {
            _logger.LogError(ex, "LLM provider error during query processing");
            return new QueryExecutionResult
            {
                Success = false,
                ErrorMessage = $"AI service error: {ex.Message}",
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (SqlValidationException ex)
        {
            _logger.LogError(ex, "SQL validation error during query processing");
            return new QueryExecutionResult
            {
                Success = false,
                ErrorMessage = $"Query validation error: {ex.Message}",
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during query processing");
            return new QueryExecutionResult
            {
                Success = false,
                ErrorMessage = $"An unexpected error occurred: {ex.Message}",
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private string EnsureTopClause(string sql)
    {
        // Check if TOP clause is already present
        if (sql.Contains("TOP ", StringComparison.OrdinalIgnoreCase))
        {
            return sql;
        }

        var maxRows = _configService.GetSetting<int>("Bot:MaxRows");
        if (maxRows <= 0) maxRows = 1000;

        // Insert TOP clause after SELECT
        var selectIndex = sql.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);
        if (selectIndex >= 0)
        {
            var insertIndex = selectIndex + 6; // Length of "SELECT"
            
            // Skip any whitespace after SELECT
            while (insertIndex < sql.Length && char.IsWhiteSpace(sql[insertIndex]))
            {
                insertIndex++;
            }

            // Check if DISTINCT is present
            if (sql.Substring(insertIndex).StartsWith("DISTINCT", StringComparison.OrdinalIgnoreCase))
            {
                insertIndex += 8; // Length of "DISTINCT"
                while (insertIndex < sql.Length && char.IsWhiteSpace(sql[insertIndex]))
                {
                    insertIndex++;
                }
            }

            sql = sql.Insert(insertIndex, $"TOP {maxRows} ");
        }

        return sql;
    }
}

public class QueryExecutionResult
{
    public bool Success { get; set; }
    public DataTable? Results { get; set; }
    public string GeneratedSql { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string ErrorMessage { get; set; } = string.Empty;
    public bool RequiresClarification { get; set; }
    public string ClarificationQuestion { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public long ExecutionTimeMs { get; set; }
    public int RowCount { get; set; }
}