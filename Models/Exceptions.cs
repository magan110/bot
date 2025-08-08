namespace DbAutoChat.Win.Models;

public class DbAutoChatException : Exception
{
    public string ErrorCode { get; }
    
    public DbAutoChatException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
    
    public DbAutoChatException(string errorCode, string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

public class SqlValidationException : DbAutoChatException
{
    public SqlValidationException(string message) : base("SQL_VALIDATION", message) { }
    public SqlValidationException(string message, Exception innerException) : base("SQL_VALIDATION", message, innerException) { }
}

public class SchemaException : DbAutoChatException
{
    public SchemaException(string message) : base("SCHEMA_ERROR", message) { }
    public SchemaException(string message, Exception innerException) : base("SCHEMA_ERROR", message, innerException) { }
}

public class LlmProviderException : DbAutoChatException
{
    public LlmProviderException(string message) : base("LLM_ERROR", message) { }
    public LlmProviderException(string message, Exception innerException) : base("LLM_ERROR", message, innerException) { }
}

public class ConfigurationException : DbAutoChatException
{
    public ConfigurationException(string message) : base("CONFIG_ERROR", message) { }
    public ConfigurationException(string message, Exception innerException) : base("CONFIG_ERROR", message, innerException) { }
}