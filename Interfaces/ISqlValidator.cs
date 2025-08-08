using DbAutoChat.Win.Models;

namespace DbAutoChat.Win.Interfaces;

public interface ISqlValidator
{
    ValidationResult ValidateQuery(string sql, DatabaseSchema schema);
}

public interface ISqlValidationRule
{
    ValidationResult Validate(string sql, DatabaseSchema schema);
}