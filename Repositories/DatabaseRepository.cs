using System.Data;
using Dapper;
using DbAutoChat.Win.Interfaces;
using DbAutoChat.Win.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DbAutoChat.Win.Repositories;

public class DatabaseRepository : IDatabaseRepository
{
    private readonly IConfigurationService _configService;
    private readonly ILogger<DatabaseRepository> _logger;
    private readonly string _schemaCachePath;
    private DatabaseSchema? _cachedSchema;

    public DatabaseRepository(IConfigurationService configService, ILogger<DatabaseRepository> logger)
    {
        _configService = configService;
        _logger = logger;
        _schemaCachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "schema.catalog.json");
    }

    public async Task<DatabaseSchema> IntrospectSchemaAsync()
    {
        try
        {
            _logger.LogInformation("Starting database schema introspection");
            
            var connectionString = _configService.GetSetting<string>("ConnectionStrings:Default");
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var schema = new DatabaseSchema
            {
                LastUpdated = DateTime.UtcNow
            };

            // Get tables and columns
            var tablesQuery = @"
                SELECT 
                    t.TABLE_SCHEMA,
                    t.TABLE_NAME,
                    c.COLUMN_NAME,
                    c.DATA_TYPE,
                    c.IS_NULLABLE,
                    CASE WHEN ic.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IS_IDENTITY
                FROM INFORMATION_SCHEMA.TABLES t
                INNER JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
                LEFT JOIN sys.identity_columns ic ON ic.object_id = OBJECT_ID(t.TABLE_SCHEMA + '.' + t.TABLE_NAME) 
                    AND ic.name = c.COLUMN_NAME
                WHERE t.TABLE_TYPE = 'BASE TABLE'
                ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME, c.ORDINAL_POSITION";

            var tableData = await connection.QueryAsync<dynamic>(tablesQuery);
            
            var tableGroups = tableData.GroupBy(row => new { 
                Schema = (string)((IDictionary<string, object>)row)["TABLE_SCHEMA"], 
                Name = (string)((IDictionary<string, object>)row)["TABLE_NAME"] 
            });

            foreach (var tableGroup in tableGroups)
            {
                var table = new TableInfo
                {
                    Schema = tableGroup.Key.Schema,
                    Name = tableGroup.Key.Name
                };

                foreach (var column in tableGroup)
                {
                    var columnDict = (IDictionary<string, object>)column;
                    table.Columns.Add(new ColumnInfo
                    {
                        Name = (string)columnDict["COLUMN_NAME"],
                        DataType = (string)columnDict["DATA_TYPE"],
                        IsNullable = (string)columnDict["IS_NULLABLE"] == "YES",
                        IsIdentity = (int)columnDict["IS_IDENTITY"] == 1
                    });
                }

                schema.Tables.Add(table);
            }

            // Get primary keys
            var primaryKeysQuery = @"
                SELECT 
                    tc.TABLE_SCHEMA,
                    tc.TABLE_NAME,
                    kcu.COLUMN_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu 
                    ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
                WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                ORDER BY tc.TABLE_SCHEMA, tc.TABLE_NAME, kcu.ORDINAL_POSITION";

            var primaryKeys = await connection.QueryAsync<dynamic>(primaryKeysQuery);
            
            foreach (var pk in primaryKeys)
            {
                var pkDict = (IDictionary<string, object>)pk;
                var table = schema.Tables.FirstOrDefault(t => 
                    t.Schema == (string)pkDict["TABLE_SCHEMA"] && t.Name == (string)pkDict["TABLE_NAME"]);
                table?.PrimaryKeys.Add((string)pkDict["COLUMN_NAME"]);
            }

            // Get foreign key relationships
            var relationshipsQuery = @"
                SELECT 
                    fk.name AS FK_NAME,
                    tp.name AS parent_table,
                    cp.name AS parent_column,
                    tr.name AS referenced_table,
                    cr.name AS referenced_column
                FROM sys.foreign_keys fk
                INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                INNER JOIN sys.tables tp ON fkc.parent_object_id = tp.object_id
                INNER JOIN sys.columns cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
                INNER JOIN sys.tables tr ON fkc.referenced_object_id = tr.object_id
                INNER JOIN sys.columns cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id";

            var relationships = await connection.QueryAsync<dynamic>(relationshipsQuery);
            
            foreach (var rel in relationships)
            {
                var relDict = (IDictionary<string, object>)rel;
                schema.Relationships.Add(new RelationshipInfo
                {
                    FromTable = (string)relDict["parent_table"],
                    FromColumn = (string)relDict["parent_column"],
                    ToTable = (string)relDict["referenced_table"],
                    ToColumn = (string)relDict["referenced_column"]
                });
            }

            // Cache the schema
            _cachedSchema = schema;
            await SaveSchemaCacheAsync(schema);

            _logger.LogInformation("Schema introspection completed. Found {TableCount} tables", schema.Tables.Count);
            return schema;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during schema introspection");
            
            // Try to load from cache if available
            if (File.Exists(_schemaCachePath))
            {
                _logger.LogWarning("Loading schema from cache due to introspection failure");
                return await LoadSchemaCacheAsync();
            }
            
            throw;
        }
    }

    public async Task<DataTable> ExecuteQueryAsync(string sql, Dictionary<string, object> parameters)
    {
        try
        {
            _logger.LogInformation("Executing SQL query: {Sql}", sql);
            
            var connectionString = _configService.GetSetting<string>("ConnectionStrings:Default");
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var result = await connection.QueryAsync(sql, parameters);
            
            var dataTable = new DataTable();
            if (result.Any())
            {
                var firstRow = result.First() as IDictionary<string, object>;
                if (firstRow != null)
                {
                    // Add columns
                    foreach (var kvp in firstRow)
                    {
                        dataTable.Columns.Add(kvp.Key, kvp.Value?.GetType() ?? typeof(object));
                    }

                    // Add rows
                    foreach (var row in result)
                    {
                        var dataRow = dataTable.NewRow();
                        var rowDict = row as IDictionary<string, object>;
                        if (rowDict != null)
                        {
                            foreach (var kvp in rowDict)
                            {
                                dataRow[kvp.Key] = kvp.Value ?? DBNull.Value;
                            }
                        }
                        dataTable.Rows.Add(dataRow);
                    }
                }
            }

            _logger.LogInformation("Query executed successfully. Returned {RowCount} rows", dataTable.Rows.Count);
            return dataTable;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SQL query: {Sql}", sql);
            throw;
        }
    }

    public async Task<bool> ValidateQueryStructureAsync(string sql)
    {
        try
        {
            var connectionString = _configService.GetSetting<string>("ConnectionStrings:Default");
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Use sys.sp_describe_first_result_set to validate SQL structure
            var validateQuery = "EXEC sys.sp_describe_first_result_set @tsql = @sql";
            var parameters = new { sql };
            
            var result = await connection.QueryAsync(validateQuery, parameters);
            return result.Any(); // If we get results, the SQL structure is valid
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SQL structure validation failed for: {Sql}", sql);
            return false;
        }
    }

    private async Task SaveSchemaCacheAsync(DatabaseSchema schema)
    {
        try
        {
            var json = JsonConvert.SerializeObject(schema, Formatting.Indented);
            await File.WriteAllTextAsync(_schemaCachePath, json);
            _logger.LogDebug("Schema cache saved to {Path}", _schemaCachePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save schema cache");
        }
    }

    private async Task<DatabaseSchema> LoadSchemaCacheAsync()
    {
        try
        {
            if (!File.Exists(_schemaCachePath))
                return new DatabaseSchema();

            var json = await File.ReadAllTextAsync(_schemaCachePath);
            var schema = JsonConvert.DeserializeObject<DatabaseSchema>(json);
            _cachedSchema = schema;
            
            _logger.LogDebug("Schema cache loaded from {Path}", _schemaCachePath);
            return schema ?? new DatabaseSchema();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load schema cache");
            return new DatabaseSchema();
        }
    }
}