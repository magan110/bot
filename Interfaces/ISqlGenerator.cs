using DbAutoChat.Win.Models;

namespace DbAutoChat.Win.Interfaces;

public interface ISqlGenerator
{
    Task<SqlGenerationResult> GenerateSqlAsync(
        string naturalLanguageQuery, 
        DatabaseSchema schema, 
        ConversationContext context);
}