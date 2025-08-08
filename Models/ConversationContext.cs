namespace DbAutoChat.Win.Models;

public class ConversationContext
{
    public List<ConversationMessage> Messages { get; set; } = new();
    public Dictionary<string, object> SessionVariables { get; set; } = new();
    public string PreferredLanguage { get; set; } = "en";
    public DateTime SessionStart { get; set; } = DateTime.Now;
}

public class ConversationMessage
{
    public string Role { get; set; } = string.Empty; // "user" or "system"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}