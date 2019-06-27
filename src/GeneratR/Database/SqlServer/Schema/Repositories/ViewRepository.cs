using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public List<View> GetAll()
        {
            return GetWhere(string.Empty, null);
        }

        private List<View> GetWhere(string whereSql, object whereParams)
        {
            if (!string.IsNullOrWhiteSpace(whereSql) && !whereSql.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
            {
                whereSql = "WHERE " + whereSql;
            }

            var views = new List<View>();
            var sqlText = $"SELECT * FROM ({SqlQueries.SelectViews}) AS [t] {whereSql} ORDER BY [t].[Schema], [t].[Name]";

            using (var conn = _schemaContext.GetConnection())
            {
                var dbViews = conn.Query(sqlText, whereParams).ToList();
                if (dbViews.Any())
                {
                    // Load relations.
                    var columnLookup = _schemaContext.Columns.GetAllForViews().ToLookup(x => x.ParentObjectID);

                    // Map.
                    views = dbViews
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
