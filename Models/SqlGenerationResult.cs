namespace DbAutoChat.Win.Models;

public class SqlGenerationResult
{
    public string GeneratedSql { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public bool RequiresClarification { get; set; }
    public string ClarificationQuestion { get; set; } = string.Empty;
    public string Language { get; set; } = "en"; // "en" or "hi-en" for Hinglish
}