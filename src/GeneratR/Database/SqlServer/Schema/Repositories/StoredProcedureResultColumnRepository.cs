using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace GeneratR.Database.SqlServer.Schema
{
    public class StoredProcedureResultColumnRepository
    {
        // TODO: Check SQL server version, and throw exception if not supported (Requires 2012+)

        private readonly SqlServerSchemaContext _schemaContext;

        public StoredProcedureResultColumnRepository(SqlServerSchemaContext schemaContext)
        {
            _schemaContext = schemaContext;
        }

        public IEnumerable<StoredProcedureResultColumn> GetAll()
        {
            return GetWhere(string.Empty, null);
        }

        private IEnumerable<StoredProcedureResultColumn> GetWhere(string whereSql, object whereParams)
        {
            var sqlBuilder = new SqlBuilder();

            if (!string.IsNullOrWhiteSpace(whereSql))
            {
                sqlBuilder.Where(whereSql, whereParams);
            }

            if (_schemaContext.IncludeSchemas != null && _schemaContext.IncludeSchemas.Any())
            {
                sqlBuilder.Where("[t].[ParentSchema] IN @IncludeSchemas", new { IncludeSchemas = _schemaContext.IncludeSchemas, });
            }

            if (_schemaContext.ExcludeSchemas != null && _schemaContext.ExcludeSchemas.Any())
            {
                sqlBuilder.Where("[t].[ParentSchema] NOT IN @ExcludeSchemas", new { ExcludeSchemas = _schemaContext.ExcludeSchemas, });
            }

            var query = sqlBuilder.AddTemplate($@"
SELECT * FROM (
{SqlQueries.SelectStoredProcedureResultColumns}
) AS [t] 
/**where**/
ORDER BY [t].[ParentSchema], [t].[ParentName], [t].[Position];");

            var data = new List<StoredProcedureResultColumn>();
            using (var conn = _schemaContext.GetConnection())
            {
                var queryResult = conn.Query(query.RawSql, query.Parameters);
                if (queryResult.Any())
                {
                    foreach (var q in queryResult)
                    {
                        var obj = new StoredProcedureResultColumn
                        {
                            ParentSchema = q.ParentSchema,
                            ParentName = q.ParentName,
                            Name = q.Name,
                            DataType = q.DataType,
                            Length = (short)q.Length,
                            Precision = (byte)q.Precision,
                            Scale = (byte)q.Scale,
                            IsNullable = q.IsNullable ?? false,
                            IsComputed = q.IsComputed ?? false,
                        };
                        data.Add(obj);
                    }
                }
            }
            return data;
        }
    }
}
