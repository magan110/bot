using DbAutoChat.Win.Models;

namespace DbAutoChat.Win.Interfaces;

public interface IConversationService
{
    void AddUserMessage(string message);
    void AddSystemResponse(string response);
    ConversationContext GetCurrentContext();
    void ClearContext();
    void SetSessionVariable(string key, object value);
    T? GetSessionVariable<T>(string key);
    void RemoveSessionVariable(string key);
    void SetPreferredLanguage(string language);
    string GetConversationSummary(int maxMessages = 10);
    int GetMessageCount();
    TimeSpan GetSessionDuration();
}