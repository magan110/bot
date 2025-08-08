using System.Text.RegularExpressions;
using DbAutoChat.Win.Interfaces;
using DbAutoChat.Win.Models;
using Microsoft.Extensions.Logging;

namespace DbAutoChat.Win.Services;

public class SqlValidationPipeline : ISqlValidator
{
    private readonly List<ISqlValidationRule> _rules;
    private readonly ILogger<SqlValidationPipeline> _logger;

    public SqlValidationPipeline(IConfigurationService configService, ILogger<SqlValidationPipeline> logger)
    {
        _logger = logger;
        _rules = new List<ISqlValidationRule>
        {
            new SelectOnlyRule(),
            new NoSemicolonRule(),
            new NoCommentsRule(),
            new DangerousFunctionRule(),
            new SchemaValidationRule(),
            new RowLimitRule(configService)
        };
    }

    public ValidationResult ValidateQuery(string sql, DatabaseSchema schema)
    {
        _logger.LogDebug("Validating SQL query: {Sql}", sql);

        foreach (var rule in _rules)
        {
            var result = rule.Validate(sql, schema);
            if (!result.IsValid)
            {
                _logger.LogWarning("SQL validation failed with rule {RuleType}: {Error}", 
                    rule.GetType().Name, result.ErrorMessage);
                return result;
            }
        }

        _logger.LogDebug("SQL validation passed");
        return ValidationResult.Success();
    }
}

public class SelectOnlyRule : ISqlValidationRule
{
    private static readonly Regex DmlDdlRegex = new(
        @"\b(INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|TRUNCATE|EXEC|EXECUTE|MERGE|BULK)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public ValidationResult Validate(string sql, DatabaseSchema schema)
    {
        if (DmlDdlRegex.IsMatch(sql))
        {
            return ValidationResult.Failure(
                "Only SELECT statements are allowed. DML/DDL operations are prohibited.",
                "SELECT_ONLY_VIOLATION");
        }

        // Ensure the query starts with SELECT (after whitespace)
        var trimmed = sql.Trim();
        if (!trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            return ValidationResult.Failure(
                "Query must start with SELECT statement.",
                "SELECT_ONLY_VIOLATION");
        }

        return ValidationResult.Success();
    }
}

public class NoSemicolonRule : ISqlValidationRule
{
    public ValidationResult Validate(string sql, DatabaseSchema schema)
    {
        if (sql.Contains(';'))
        {
            return ValidationResult.Failure(
                "Semicolons are not allowed in queries to prevent SQL injection.",
                "SEMICOLON_VIOLATION");
        }

        return ValidationResult.Success();
    }
}

public class NoCommentsRule : ISqlValidationRule
{
    private static readonly Regex CommentRegex = new(
        @"(--.*$|/\*.*?\*/)",
        RegexOptions.Multiline | RegexOptions.Compiled);

    public ValidationResult Validate(string sql, DatabaseSchema schema)
    {
        if (CommentRegex.IsMatch(sql))
        {
            return ValidationResult.Failure(
                "SQL comments are not allowed to prevent injection attacks.",
                "COMMENT_VIOLATION");
        }

        return ValidationResult.Success();
    }
}

public class DangerousFunctionRule : ISqlValidationRule
{
    private static readonly string[] DangerousFunctions = {
        "xp_cmdshell", "sp_configure", "openrowset", "opendatasource",
        "xp_regread", "xp_regwrite", "xp_regdelete", "xp_instance_regread",
        "xp_instance_regwrite", "xp_instance_regdelete", "sp_oacreate",
        "sp_oamethod", "sp_oagetproperty", "sp_oasetproperty", "sp_oadestroy"
    };

    public ValidationResult Validate(string sql, DatabaseSchema schema)
    {
        var sqlLower = sql.ToLowerInvariant();
        
        foreach (var func in DangerousFunctions)
        {
            if (sqlLower.Contains(func.ToLowerInvariant()))
            {
                return ValidationResult.Failure(
                    $"Dangerous function '{func}' is not allowed.",
                    "DANGEROUS_FUNCTION_VIOLATION");
            }
        }

        return ValidationResult.Success();
    }
}

public class SchemaValidationRule : ISqlValidationRule
{
    private static readonly Regex TableRegex = new(
        @"\bFROM\s+(?:\[?(\w+)\]?\.)?(?:\[?(\w+)\]?)\b|\bJOIN\s+(?:\[?(\w+)\]?\.)?(?:\[?(\w+)\]?)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public ValidationResult Validate(string sql, DatabaseSchema schema)
    {
        if (!schema.Tables.Any())
        {
            // If no schema is available, skip validation
            return ValidationResult.Success();
        }

        var matches = TableRegex.Matches(sql);
        
        foreach (Match match in matches)
        {
            // Extract table name (could be in different capture groups)
            string? tableName = null;
            for (int i = 1; i < match.Groups.Count; i++)
            {
                if (!string.IsNullOrEmpty(match.Groups[i].Value))
                {
                    tableName = match.Groups[i].Value;
                }
            }

            if (!string.IsNullOrEmpty(tableName))
            {
                var tableExists = schema.Tables.Any(t => 
                    string.Equals(t.Name, tableName, StringComparison.OrdinalIgnoreCase));

                if (!tableExists)
                {
                    return ValidationResult.Failure(
                        $"Table '{tableName}' does not exist in the database schema.",
                        "SCHEMA_VIOLATION");
                }
            }
        }

        return ValidationResult.Success();
    }
}

public class RowLimitRule : ISqlValidationRule
{
    private readonly IConfigurationService _configService;
    private static readonly Regex TopRegex = new(
        @"\bSELECT\s+(?:DISTINCT\s+)?TOP\s+\(\s*\d+\s*\)|\bSELECT\s+(?:DISTINCT\s+)?TOP\s+\d+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public RowLimitRule(IConfigurationService configService)
    {
        _configService = configService;
    }

    public ValidationResult Validate(string sql, DatabaseSchema schema)
    {
        var maxRows = _configService.GetSetting<int>("Bot:MaxRows");
        if (maxRows <= 0) maxRows = 1000; // Default limit

        // Check if TOP clause is already present
        if (!TopRegex.IsMatch(sql))
        {
            return ValidationResult.Failure(
                $"Query must include a TOP clause to limit results to {maxRows} rows or fewer.",
                "ROW_LIMIT_VIOLATION");
        }

        // Extract the TOP value and validate it's within limits
        var match = TopRegex.Match(sql);
        if (match.Success)
        {
            var topValue = ExtractTopValue(match.Value);
            if (topValue > maxRows)
            {
                return ValidationResult.Failure(
                    $"TOP clause cannot exceed {maxRows} rows. Current value: {topValue}",
                    "ROW_LIMIT_EXCEEDED");
            }
        }

        return ValidationResult.Success();
    }

    private int ExtractTopValue(string topClause)
    {
        var numberRegex = new Regex(@"\d+", RegexOptions.Compiled);
        var match = numberRegex.Match(topClause);
        
        if (match.Success && int.TryParse(match.Value, out var value))
        {
            return value;
        }

        return 0;
    }
}