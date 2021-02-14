using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace GeneratR.Database.SqlServer.Schema
{
    public class TableRepository
    {
        private readonly SqlServerSchemaContext _schemaContext;

        public TableRepository(SqlServerSchemaContext schemaContext)
        {
            _schemaContext = schemaContext;
        }

        public IEnumerable<Table> GetAll()
        {
            return GetWhere(string.Empty, null);
        }

        private IEnumerable<Table> GetWhere(string whereSql, object whereParams)
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
{SqlQueries.SelectTables}
) AS [t] 
/**where**/
ORDER BY [t].[Schema], [t].[Name];");

            var tables = new List<Table>();
            using (var conn = _schemaContext.GetConnection())
            {
                var dbTables = conn.Query(query.RawSql, query.Parameters).ToList();
                if (dbTables.Any())
                {
                    // Load relations.
                    var indexLookup = _schemaContext.Indexes.GetAllForTables().ToLookup(x => x.ParentObjectID);
                    var columnLookup = _schemaContext.Columns.GetAllForTables().ToLookup(x => x.ParentObjectID);
                    var foreignKeys = _schemaContext.ForeignKeys.GetAll().ToList();
                    var foreignKeyToLookup = foreignKeys.ToLookup(x => x.ToObjectID);
                    var foreignKeyFromLookup = foreignKeys.ToLookup(x => x.FromObjectID);

                    // Map.
                    tables = dbTables
                        .Select(x => new Table()
                        {
                            ObjectID = x.ObjectID,
                            Schema = x.Schema,
                            Name = x.Name,
                            Columns = columnLookup[(int)x.ObjectID].ToList(),
                            Indexes = indexLookup[(int)x.ObjectID].ToList(),
                            ForeignKeys = foreignKeyFromLookup[(int)x.ObjectID].ToList(),
                            ReferencingForeignKeys = foreignKeyToLookup[(int)x.ObjectID].ToList(),
                        }).ToList();
                }
            }

            return tables;
        }
    }
}
