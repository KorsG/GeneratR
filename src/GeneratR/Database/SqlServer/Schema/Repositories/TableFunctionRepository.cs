using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace GeneratR.Database.SqlServer.Schema
{
    public class TableFunctionRepository
    {
        private readonly SqlServerSchemaContext _schemaContext;

        public TableFunctionRepository(SqlServerSchemaContext schemaContext)
        {
            _schemaContext = schemaContext;
        }

        public IEnumerable<TableFunction> GetAll()
        {
            return GetWhere(string.Empty, null);
        }

        private IEnumerable<TableFunction> GetWhere(string whereSql, object whereParams)
        {
            var sqlBuilder = new SqlBuilder();

            if (!string.IsNullOrWhiteSpace(whereSql))
            {
                sqlBuilder.Where(whereSql, whereParams);
            }

            if (_schemaContext.IncludeSchemas != null && _schemaContext.IncludeSchemas.Any())
            {
                sqlBuilder.Where("[t].[Schema] IN @IncludeSchemas", new { IncludeSchemas = _schemaContext.IncludeSchemas, });
            }

            if (_schemaContext.ExcludeSchemas != null && _schemaContext.ExcludeSchemas.Any())
            {
                sqlBuilder.Where("[t].[Schema] NOT IN @ExcludeSchemas", new { ExcludeSchemas = _schemaContext.ExcludeSchemas, });
            }

            var query = sqlBuilder.AddTemplate($@"
SELECT * FROM (
{SqlQueries.SelectTableFunctions}
) AS [t] 
/**where**/
ORDER BY [t].[Schema], [t].[Name];");

            var tableFunctions = new List<TableFunction>();
            using (var conn = _schemaContext.GetConnection())
            {
                var dbTableFunctions = conn.Query(query.RawSql, query.Parameters).ToList();
                if (dbTableFunctions.Any())
                {
                    // Load relations.
                    var columnLookup = _schemaContext.Columns.GetAllForTableFunctions().ToLookup(x => x.ParentObjectID);
                    var paramLookup = _schemaContext.Parameters.GetAllForTableFunctions().ToLookup(x => x.ParentObjectID);

                    // Map.
                    tableFunctions = dbTableFunctions
                        .Select(x => new TableFunction()
                        {
                            ObjectID = x.ObjectID,
                            Schema = x.Schema,
                            Name = x.Name,
                            Columns = columnLookup[(int)x.ObjectID].ToList(),
                            Parameters = paramLookup[(int)x.ObjectID].ToList(),
                        }).ToList();
                }
            }

            return tableFunctions;
        }
    }
}
