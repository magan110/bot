using System.Text.RegularExpressions;
using DbAutoChat.Win.Interfaces;
using DbAutoChat.Win.Models;
using Microsoft.Extensions.Logging;

namespace DbAutoChat.Win.Services;

public class ConversationService : IConversationService
{
    private ConversationContext _context;
    private readonly ILogger<ConversationService> _logger;

    public ConversationService(ILogger<ConversationService> logger)
    {
        _context = new ConversationContext();
        _logger = logger;
    }

    public void AddUserMessage(string message)
    {
        // Detect language preference from user message
        var detectedLanguage = DetectLanguage(message);
        if (!string.IsNullOrEmpty(detectedLanguage))
        {
            _context.PreferredLanguage = detectedLanguage;
        }

        _context.Messages.Add(new ConversationMessage
        {
            Role = "user",
            Content = message,
            Timestamp = DateTime.Now
        });

        _logger.LogDebug("Added user message to conversation context. Language: {Language}", _context.PreferredLanguage);
    }

    public void AddSystemResponse(string response)
    {
        _context.Messages.Add(new ConversationMessage
        {
            Role = "system",
            Content = response,
            Timestamp = DateTime.Now
        });

        _logger.LogDebug("Added system response to conversation context");
    }

    public ConversationContext GetCurrentContext()
    {
        return _context;
    }

    public void ClearContext()
    {
        _logger.LogInformation("Clearing conversation context");
        _context = new ConversationContext();
    }

    public void SetSessionVariable(string key, object value)
    {
        _context.SessionVariables[key] = value;
        _logger.LogDebug("Set session variable: {Key} = {Value}", key, value);
    }

    public T? GetSessionVariable<T>(string key)
    {
        if (_context.SessionVariables.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    public void RemoveSessionVariable(string key)
    {
        _context.SessionVariables.Remove(key);
        _logger.LogDebug("Removed session variable: {Key}", key);
    }

    public void SetPreferredLanguage(string language)
    {
        _context.PreferredLanguage = language;
        _logger.LogDebug("Set preferred language: {Language}", language);
    }

    public string GetConversationSummary(int maxMessages = 10)
    {
        var recentMessages = _context.Messages
            .TakeLast(maxMessages)
            .Select(m => $"{m.Role}: {m.Content}")
            .ToList();

        return string.Join("\n", recentMessages);
    }

    public int GetMessageCount()
    {
        return _context.Messages.Count;
    }

    public TimeSpan GetSessionDuration()
    {
        return DateTime.Now - _context.SessionStart;
    }

    private string DetectLanguage(string text)
    {
        // Simple Hinglish detection - look for common Hindi words in Latin script
        var hinglishPatterns = new[]
        {
            @"\b(mein|ke|ki|ka|hai|hain|dikhao|batao|kya|kaise|kahan|kab|kitna|kitne)\b",
            @"\b(aur|ya|se|tak|par|liye|sath|wala|wale|wali|chahiye|karo|karna)\b",
            @"\b(abhi|phir|jab|tab|yahan|wahan|iska|uska|mere|tere|apna)\b"
        };

        foreach (var pattern in hinglishPatterns)
        {
            if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
            {
                return "hi-en";
            }
        }

        return "en";
    }
}