using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DbAutoChat.Win.Interfaces;
using DbAutoChat.Win.Models;
using Microsoft.Extensions.Logging;

namespace DbAutoChat.Win.Services;

public class OllamaSqlGenerator : ISqlGenerator
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _model;
    private readonly ILogger<OllamaSqlGenerator> _logger;

    public OllamaSqlGenerator(
        HttpClient httpClient,
        IConfigurationService configService,
        ILogger<OllamaSqlGenerator> logger)
    {
        _httpClient = httpClient;
        _baseUrl = configService.GetSetting<string>("Bot:Ollama:BaseUrl") ?? "http://localhost:11434";
        _model = configService.GetSetting<string>("Bot:Ollama:Model") ?? "llama2";
        _logger = logger;
    }

    public async Task<SqlGenerationResult> GenerateSqlAsync(
        string naturalLanguageQuery, 
        DatabaseSchema schema, 
        ConversationContext context)
    {
        try
        {
            _logger.LogInformation("Generating SQL with Ollama for query: {Query}", naturalLanguageQuery);
            
            var language = DetectLanguage(naturalLanguageQuery);
            var prompt = BuildPrompt(naturalLanguageQuery, schema, context, language);

            var requestBody = new
            {
                model = _model,
                prompt = prompt,
                stream = false,
                options = new
                {
                    temperature = 0.1,
                    num_predict = 1000
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);
            
            var generatedText = responseObj.GetProperty("response").GetString() ?? "";

            return ParseLlmResponse(generatedText, language);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL with Ollama");
            throw new LlmProviderException($"Ollama API error: {ex.Message}");
        }
    }

    private string DetectLanguage(string query)
    {
        // Simple Hinglish detection - look for common Hindi words in Latin script
        var hinglishPatterns = new[]
        {
            @"\b(mein|ke|ki|ka|hai|hain|dikhao|batao|kya|kaise|kahan|kab|kitna|kitne)\b",
            @"\b(aur|ya|se|tak|par|liye|sath|wala|wale|wali)\b"
        };

        foreach (var pattern in hinglishPatterns)
        {
            if (Regex.IsMatch(query, pattern, RegexOptions.IgnoreCase))
            {
                return "hi-en";
            }
        }

        return "en";
    }

    private string BuildPrompt(string naturalLanguageQuery, DatabaseSchema schema, ConversationContext context, string language)
    {
        var prompt = new StringBuilder();
        
        if (language == "hi-en")
        {
            prompt.AppendLine("You are a SQL query generator that understands both English and Hinglish (Hindi-English mix). Generate ONLY SELECT statements.");
        }
        else
        {
            prompt.AppendLine("You are a SQL query generator. Generate ONLY SELECT statements.");
        }

        prompt.AppendLine("CRITICAL RULES:");
        prompt.AppendLine("1. Generate ONLY SELECT statements - no INSERT, UPDATE, DELETE, or DDL");
        prompt.AppendLine("2. Always use parameterized queries with named parameters (@param)");
        prompt.AppendLine("3. Always include TOP clause to limit results");
        prompt.AppendLine("4. Use only tables and columns from the provided schema");
        prompt.AppendLine("5. If the query is unclear, respond with CLARIFICATION_NEEDED followed by a question");
        prompt.AppendLine();

        prompt.AppendLine("DATABASE SCHEMA:");
        foreach (var table in schema.Tables)
        {
            prompt.AppendLine($"Table: {table.Schema}.{table.Name}");
            foreach (var column in table.Columns)
            {
                prompt.AppendLine($"  - {column.Name} ({column.DataType})");
            }
            prompt.AppendLine();
        }

        prompt.AppendLine("RESPONSE FORMAT:");
        prompt.AppendLine("If generating SQL: SQL|<sql_query>|PARAMS|<param1>=<value1>,<param2>=<value2>");
        prompt.AppendLine("If clarification needed: CLARIFICATION_NEEDED|<question>");
        prompt.AppendLine();

        prompt.AppendLine($"Query: {naturalLanguageQuery}");

        if (context.Messages.Any())
        {
            prompt.AppendLine("\nConversation Context:");
            foreach (var msg in context.Messages.TakeLast(5))
            {
                prompt.AppendLine($"{msg.Role}: {msg.Content}");
            }
        }

        if (context.SessionVariables.Any())
        {
            prompt.AppendLine("\nSession Variables:");
            foreach (var var in context.SessionVariables)
            {
                prompt.AppendLine($"{var.Key}: {var.Value}");
            }
        }

        return prompt.ToString();
    }

    private SqlGenerationResult ParseLlmResponse(string response, string language)
    {
        var result = new SqlGenerationResult { Language = language };

        if (response.StartsWith("CLARIFICATION_NEEDED"))
        {
            var parts = response.Split('|', 2);
            result.RequiresClarification = true;
            result.ClarificationQuestion = parts.Length > 1 ? parts[1].Trim() : "Could you please provide more details?";
            return result;
        }

        if (response.StartsWith("SQL"))
        {
            var parts = response.Split('|');
            if (parts.Length >= 2)
            {
                result.GeneratedSql = parts[1].Trim();
                
                if (parts.Length >= 4 && parts[2] == "PARAMS")
                {
                    var paramString = parts[3];
                    result.Parameters = ParseParameters(paramString);
                }
            }
        }
        else
        {
            // Fallback - try to extract SQL from response
            result.GeneratedSql = ExtractSqlFromResponse(response);
        }

        return result;
    }

    private Dictionary<string, object> ParseParameters(string paramString)
    {
        var parameters = new Dictionary<string, object>();
        
        if (string.IsNullOrWhiteSpace(paramString))
            return parameters;

        var pairs = paramString.Split(',');
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=', 2);
            if (keyValue.Length == 2)
            {
                parameters[keyValue[0].Trim()] = keyValue[1].Trim();
            }
        }

        return parameters;
    }

    private string ExtractSqlFromResponse(string response)
    {
        // Try to find SQL in code blocks or clean up the response
        var sqlMatch = Regex.Match(response, @"```sql\s*(.*?)\s*```", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (sqlMatch.Success)
        {
            return sqlMatch.Groups[1].Value.Trim();
        }

        // If no code block, return the response as-is (cleaned up)
        return response.Trim();
    }
}