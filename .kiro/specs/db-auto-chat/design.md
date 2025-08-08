# Design Document

## Overview

DbAutoChat.Win is a .NET 8 Windows Forms desktop application that provides a conversational interface for querying SQL Server databases using natural language (English and Hinglish). The application follows a layered architecture with clear separation of concerns, implementing the Repository pattern for data access, Strategy pattern for LLM providers, and Chain of Responsibility pattern for SQL validation.

The system consists of five main layers:
- **Presentation Layer**: Windows Forms UI components
- **Application Layer**: Orchestration and workflow management
- **Domain Layer**: Business logic and core entities
- **Infrastructure Layer**: External service integrations (LLM providers, database)
- **Cross-Cutting Concerns**: Logging, configuration, validation

## Architecture

### High-Level Architecture Diagram

```mermaid
graph TB
    subgraph "Presentation Layer"
        UI[MainForm]
        Settings[SettingsForm]
        Grid[DataGridView]
    end
    
    subgraph "Application Layer"
        ChatService[ChatService]
        QueryOrchestrator[QueryOrchestrator]
        ValidationPipeline[ValidationPipeline]
    end
    
    subgraph "Domain Layer"
        Schema[SchemaModel]
        Conversation[ConversationContext]
        SqlQuery[SqlQueryModel]
    end
    
    subgraph "Infrastructure Layer"
        LLM[LLM Providers]
        DB[Database Repository]
        Config[Configuration]
        Logger[Logging]
    end
    
    UI --> ChatService
    Settings --> Config
    ChatService --> QueryOrchestrator
    QueryOrchestrator --> ValidationPipeline
    QueryOrchestrator --> LLM
    QueryOrchestrator --> DB
    ValidationPipeline --> Schema
    DB --> Schema
```

### Component Interaction Flow

```mermaid
sequenceDiagram
    participant User
    participant MainForm
    participant ChatService
    participant QueryOrchestrator
    participant LLMProvider
    participant SqlValidator
    participant DatabaseRepository
    participant DataGridView

    User->>MainForm: Enter natural language query
    MainForm->>ChatService: ProcessUserQuery(query)
    ChatService->>QueryOrchestrator: GenerateAndExecuteQuery(query, context)
    QueryOrchestrator->>LLMProvider: GenerateSql(query, schema, context)
    LLMProvider-->>QueryOrchestrator: SQL query
    QueryOrchestrator->>SqlValidator: ValidateQuery(sql, schema)
    
    alt Query is valid
        SqlValidator-->>QueryOrchestrator: Valid
        QueryOrchestrator->>DatabaseRepository: ExecuteQuery(sql, parameters)
        DatabaseRepository-->>QueryOrchestrator: DataTable results
        QueryOrchestrator-->>ChatService: QueryResult
        ChatService-->>MainForm: Display results
        MainForm->>DataGridView: Bind data
    else Query needs clarification
        SqlValidator-->>QueryOrchestrator: NeedsClarification
        QueryOrchestrator-->>ChatService: ClarificationRequest
        ChatService-->>MainForm: Display clarifying question
    else Query is invalid/unsafe
        SqlValidator-->>QueryOrchestrator: Invalid/Unsafe
        QueryOrchestrator-->>ChatService: Error message
        ChatService-->>MainForm: Display error
    end
```

## Components and Interfaces

### Core Interfaces

```csharp
// LLM Provider abstraction
public interface ISqlGenerator
{
    Task<SqlGenerationResult> GenerateSqlAsync(
        string naturalLanguageQuery, 
        DatabaseSchema schema, 
        ConversationContext context);
}

// Database operations
public interface IDatabaseRepository
{
    Task<DatabaseSchema> IntrospectSchemaAsync();
    Task<DataTable> ExecuteQueryAsync(string sql, Dictionary<string, object> parameters);
    Task<bool> ValidateQueryStructureAsync(string sql);
}

// SQL validation pipeline
public interface ISqlValidator
{
    ValidationResult ValidateQuery(string sql, DatabaseSchema schema);
}

// Configuration management
public interface IConfigurationService
{
    T GetSetting<T>(string key);
    void SetSetting<T>(string key, T value);
    Task SaveAsync();
}

// Conversation context management
public interface IConversationService
{
    void AddUserMessage(string message);
    void AddSystemResponse(string response);
    ConversationContext GetCurrentContext();
    void ClearContext();
}
```

### Key Domain Models

```csharp
// Database schema representation
public class DatabaseSchema
{
    public List<TableInfo> Tables { get; set; }
    public List<RelationshipInfo> Relationships { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class TableInfo
{
    public string Name { get; set; }
    public string Schema { get; set; }
    public List<ColumnInfo> Columns { get; set; }
    public List<string> PrimaryKeys { get; set; }
}

public class ColumnInfo
{
    public string Name { get; set; }
    public string DataType { get; set; }
    public bool IsNullable { get; set; }
    public bool IsIdentity { get; set; }
}

// SQL generation result
public class SqlGenerationResult
{
    public string GeneratedSql { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    public bool RequiresClarification { get; set; }
    public string ClarificationQuestion { get; set; }
    public string Language { get; set; } // "en" or "hi-en" for Hinglish
}

// Conversation context for session memory
public class ConversationContext
{
    public List<ConversationMessage> Messages { get; set; }
    public Dictionary<string, object> SessionVariables { get; set; }
    public string PreferredLanguage { get; set; }
    public DateTime SessionStart { get; set; }
}
```

### LLM Provider Implementations

```csharp
// OpenAI implementation
public class OpenAiSqlGenerator : ISqlGenerator
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<OpenAiSqlGenerator> _logger;

    public async Task<SqlGenerationResult> GenerateSqlAsync(
        string naturalLanguageQuery, 
        DatabaseSchema schema, 
        ConversationContext context)
    {
        var systemPrompt = BuildSystemPrompt(schema, context.PreferredLanguage);
        var userPrompt = BuildUserPrompt(naturalLanguageQuery, context);
        
        // Call OpenAI API with structured prompts
        // Parse response and extract SQL + parameters
        // Detect if clarification is needed
    }
}

// Similar implementations for Gemini and Ollama
public class GeminiSqlGenerator : ISqlGenerator { /* ... */ }
public class OllamaSqlGenerator : ISqlGenerator { /* ... */ }
```

### SQL Validation Pipeline

```csharp
public class SqlValidationPipeline : ISqlValidator
{
    private readonly List<ISqlValidationRule> _rules;

    public ValidationResult ValidateQuery(string sql, DatabaseSchema schema)
    {
        foreach (var rule in _rules)
        {
            var result = rule.Validate(sql, schema);
            if (!result.IsValid)
                return result;
        }
        return ValidationResult.Success();
    }
}

// Individual validation rules
public class SelectOnlyRule : ISqlValidationRule { /* Ensures only SELECT statements */ }
public class NoSemicolonRule : ISqlValidationRule { /* Blocks semicolons */ }
public class NoCommentsRule : ISqlValidationRule { /* Blocks SQL comments */ }
public class DangerousFunctionRule : ISqlValidationRule { /* Blocks xp_cmdshell, etc. */ }
public class SchemaValidationRule : ISqlValidationRule { /* Validates table/column names */ }
public class RowLimitRule : ISqlValidationRule { /* Enforces TOP clause */ }
```

## Data Models

### Configuration Schema (appsettings.json)

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=SampleDB;Integrated Security=true;TrustServerCertificate=true;"
  },
  "Bot": {
    "MaxRows": 1000,
    "Provider": "OpenAI",
    "OpenAI": {
      "ApiKey": "",
      "Model": "gpt-4",
      "BaseUrl": "https://api.openai.com/v1"
    },
    "Gemini": {
      "ApiKey": "AIzaSyDYIfu2K0VpG3M42WIjJrY48mgh7L0Ir7s",
      "Model": "gemini-pro"
    },
    "Ollama": {
      "BaseUrl": "http://localhost:11434",
      "Model": "llama2"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    },
    "File": {
      "Path": "logs/dbautochat-.log",
      "RollingInterval": "Day"
    }
  }
}
```

### Schema Cache (schema.catalog.json)

```json
{
  "lastUpdated": "2024-01-15T10:30:00Z",
  "tables": [
    {
      "name": "Customers",
      "schema": "dbo",
      "columns": [
        {
          "name": "CustomerID",
          "dataType": "int",
          "isNullable": false,
          "isIdentity": true
        },
        {
          "name": "CustomerName",
          "dataType": "nvarchar(100)",
          "isNullable": false,
          "isIdentity": false
        }
      ],
      "primaryKeys": ["CustomerID"]
    }
  ],
  "relationships": [
    {
      "fromTable": "Orders",
      "fromColumn": "CustomerID",
      "toTable": "Customers",
      "toColumn": "CustomerID"
    }
  ]
}
```

## Error Handling

### Exception Hierarchy

```csharp
public class DbAutoChatException : Exception
{
    public string ErrorCode { get; }
    public DbAutoChatException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}

public class SqlValidationException : DbAutoChatException
{
    public SqlValidationException(string message) : base("SQL_VALIDATION", message) { }
}

public class SchemaException : DbAutoChatException
{
    public SchemaException(string message) : base("SCHEMA_ERROR", message) { }
}

public class LlmProviderException : DbAutoChatException
{
    public LlmProviderException(string message) : base("LLM_ERROR", message) { }
}
```

### Error Handling Strategy

1. **UI Level**: Display user-friendly error messages in the conversation
2. **Application Level**: Log detailed errors and attempt graceful recovery
3. **Infrastructure Level**: Handle network timeouts, database connection issues
4. **Validation Level**: Provide specific feedback about SQL safety violations

### Retry Policies

- **LLM API calls**: Exponential backoff with 3 retries
- **Database connections**: Immediate retry once, then fail
- **Schema refresh**: Manual retry only

## Testing Strategy

### Unit Testing

```csharp
// Example test structure
[TestClass]
public class SqlValidationPipelineTests
{
    [TestMethod]
    public void ValidateQuery_WithSelectStatement_ShouldPass()
    {
        // Arrange
        var pipeline = new SqlValidationPipeline();
        var sql = "SELECT * FROM Customers WHERE City = @city";
        
        // Act
        var result = pipeline.ValidateQuery(sql, _mockSchema);
        
        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void ValidateQuery_WithInsertStatement_ShouldFail()
    {
        // Test SQL injection prevention
    }
}
```

### Integration Testing

- **Database Schema Introspection**: Test against real SQL Server instances
- **LLM Provider Integration**: Test with actual API calls (using test keys)
- **End-to-End Workflows**: Simulate complete user interactions

### UI Testing

- **Form Behavior**: Test Windows Forms interactions
- **DataGridView Operations**: Test data binding and export functionality
- **Settings Persistence**: Test configuration save/load

### Performance Testing

- **Large Result Sets**: Test with queries returning maximum allowed rows
- **Schema Caching**: Measure schema introspection and cache performance
- **LLM Response Times**: Monitor API call latencies

### Security Testing

- **SQL Injection Prevention**: Test with malicious input patterns
- **SQL Safety Validation**: Verify all dangerous operations are blocked
- **Configuration Security**: Test API key storage and retrieval

### Hinglish Language Testing

- **Mixed Language Queries**: Test Hindi-English code-switching
- **Cultural Context**: Test India-specific terms (cities, names, etc.)
- **Clarification Questions**: Verify bilingual follow-up questions

## Implementation Notes

### Windows Forms UI Design

- **Main Form**: MDI container with docked panels for conversation and results
- **Conversation Panel**: RichTextBox with custom formatting for user/system messages
- **Input Panel**: TextBox with Enter key handling and Ask button
- **Results Panel**: DataGridView with context menu for copy/export
- **Status Bar**: Connection status, query execution progress, row counts
- **Menu Bar**: File, Edit, Tools (Settings, Refresh Schema), Help

### Performance Optimizations

- **Schema Caching**: In-memory cache with file persistence
- **Lazy Loading**: Load schema details only when needed
- **Connection Pooling**: Reuse database connections efficiently
- **Async Operations**: Non-blocking UI during LLM calls and query execution

### Security Considerations

- **API Key Storage**: Use Windows Data Protection API (DPAPI) for encryption
- **SQL Parameterization**: Always use named parameters for user input
- **Connection String Security**: Support Windows Authentication and encrypted connections
- **Audit Logging**: Log all queries and their sources for security monitoring

### Localization Support

- **Language Detection**: Automatically detect English vs Hinglish input
- **Response Language**: Match system responses to user's input language
- **Cultural Adaptation**: Handle Indian date formats, currency, and naming conventions