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
            return GetWhere("", null);
        }

        private List<View> GetWhere(string whereSql, object whereParams)
        {
            if (!string.IsNullOrWhiteSpace(whereSql) && !whereSql.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
            {
                whereSql = "WHERE " + whereSql;
            }

            var data = new List<View>();
            var sqlText = $"SELECT * FROM ({SqlQueries.SelectViews}) AS [t1] {whereSql} ORDER BY [t1].[Schema], [t1].[Name]";
            using (var conn = _schemaContext.GetConnection())
            {
                var queryResult = conn.Query(sqlText, whereParams);
                if (queryResult.Any())
                {
                    var columns = _schemaContext.Columns.GetAllForViews();
                    foreach (var q in queryResult)
                    {
                        var obj = new View()
                        {
                            Schema = q.Schema,
                            Name = q.Name,
                        };
                        obj.Columns = (
                            from c in columns
                            where c.ParentName.Equals(obj.Name, StringComparison.OrdinalIgnoreCase)
                            && c.ParentSchema.Equals(obj.Schema, StringComparison.OrdinalIgnoreCase)
                            select c).ToList();

                        data.Add(obj);
                    }
                }
                return data;
            }
        }
    }
}
