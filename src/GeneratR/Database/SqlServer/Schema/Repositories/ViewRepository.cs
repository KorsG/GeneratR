using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace GeneratR.Database.SqlServer.Schema
{
    public sealed class ViewRepository
    {
        private readonly SqlServerSchemaContext _schemaContext;

        public ViewRepository(SqlServerSchemaContext schemaContext)
        {
            _schemaContext = schemaContext;
        }

        public IEnumerable<View> GetAll()
        {
            return GetWhere(string.Empty, null);
        }

        private IEnumerable<View> GetWhere(string whereSql, object whereParams)
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
{SqlQueries.SelectViews}
) AS [t] 
/**where**/
ORDER BY [t].[Schema], [t].[Name];");

            var views = new List<View>();
            using (var conn = _schemaContext.GetConnection())
            {
                var dbViews = conn.Query(query.RawSql, query.Parameters).ToList();
                if (dbViews.Any())
                {
                    // Load relations.
                    var columnLookup = _schemaContext.Columns.GetAllForViews().ToLookup(x => x.ParentObjectID);

                    // Map.
                    return views = dbViews
                        .Select(x => new View()
                        {
                            ObjectID = x.ObjectID,
                            Schema = x.Schema,
                            Name = x.Name,
                            Columns = columnLookup[(int)x.ObjectID].ToList()
                        }).ToList();
                }
            }

            return views;
        }
    }
}
