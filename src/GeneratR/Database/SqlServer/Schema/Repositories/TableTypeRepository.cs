using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace GeneratR.Database.SqlServer.Schema
{
    public class TableTypeRepository
    {
        private readonly SqlServerSchemaContext _schemaContext;

        public TableTypeRepository(SqlServerSchemaContext schemaContext)
        {
            _schemaContext = schemaContext;
        }

        public IEnumerable<TableType> GetAll()
        {
            return GetWhere(string.Empty, null);
        }

        private IEnumerable<TableType> GetWhere(string whereSql, object whereParams)
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
{SqlQueries.SelectTableTypes}
) AS [t] 
/**where**/
ORDER BY [t].[Schema], [t].[Name];");

            var tableTypes = new List<TableType>();
            using (var conn = _schemaContext.GetConnection())
            {
                var dbTableTypes = conn.Query(query.RawSql, query.Parameters);
                if (dbTableTypes.Any())
                {
                    // Load relations.
                    var columnLookup = _schemaContext.Columns.GetAllForTableTypes().ToLookup(x => x.ParentObjectID);

                    // Map.
                    tableTypes = dbTableTypes
                        .Select(x => new TableType()
                        {
                            ObjectID = x.ObjectID,
                            Schema = x.Schema,
                            Name = x.Name,
                            Columns = columnLookup[(int)x.ObjectID].ToList()
                        }).ToList();

                    return tableTypes;
                }
            }

            return tableTypes;
        }
    }
}
