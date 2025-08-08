using System.Data;
using DbAutoChat.Win.Models;

namespace DbAutoChat.Win.Interfaces;

public interface IDatabaseRepository
{
    Task<DatabaseSchema> IntrospectSchemaAsync();
    Task<DataTable> ExecuteQueryAsync(string sql, Dictionary<string, object> parameters);
    Task<bool> ValidateQueryStructureAsync(string sql);
}