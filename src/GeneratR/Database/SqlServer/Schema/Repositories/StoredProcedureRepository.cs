using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace GeneratR.Database.SqlServer.Schema
{
    public class StoredProcedureRepository
    {
        private readonly SqlServerSchemaContext _schemaContext;

        public StoredProcedureRepository(SqlServerSchemaContext schemaContext)
        {
            _schemaContext = schemaContext;
        }

        public IEnumerable<StoredProcedure> GetAll(bool includeResultColumns = true)
        {
            return GetWhere(string.Empty, null, includeResultColumns);
        }

        private IEnumerable<StoredProcedure> GetWhere(string whereSql, object whereParams, bool includeResultColumns = true)
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
{SqlQueries.SelectStoredProcedures}
) AS [t] 
/**where**/
ORDER BY [t].[Schema], [t].[Name];");

            var data = new List<StoredProcedure>();
            using (var conn = _schemaContext.GetConnection())
            {
                var queryResult = conn.Query(query.RawSql, query.Parameters);
                if (queryResult.Any())
                {
                    var paramLookup = _schemaContext.Parameters.GetAllForStoredProcedures().ToLookup(x => x.ParentObjectID);
                    var columns = includeResultColumns ? _schemaContext.StoredProcedureResultColumns.GetAll() : null;
                    foreach (var q in queryResult)
                    {
                        var obj = new StoredProcedure()
                        {
                            ObjectID = q.ObjectID,
                            Schema = q.Schema,
                            Name = q.Name,
                        };

                        obj.Parameters = paramLookup[obj.ObjectID].ToList();

                        if (includeResultColumns)
                        {
                            obj.ResultColumns = (
                              from p in columns
                              where p.ParentName.Equals(obj.Name, StringComparison.OrdinalIgnoreCase)
                              && p.ParentSchema.Equals(obj.Schema, StringComparison.OrdinalIgnoreCase)
                              select p).ToList();
                        }
                        else
                        {
                            obj.ResultColumns = new List<StoredProcedureResultColumn>();
                        }

                        data.Add(obj);
                    }
                }
            }
            return data;
        }
    }
}
