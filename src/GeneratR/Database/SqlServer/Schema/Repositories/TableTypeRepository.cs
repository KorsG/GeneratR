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
            return GetWhere("", null);
        }

        private IEnumerable<TableType> GetWhere(string whereSql, object whereParams)
        {
            var data = new List<TableType>();
            using (var conn = _schemaContext.GetConnection())
            {
                var sql = $"SELECT * FROM ({SqlQueries.SelectTableTypes}) AS [t] {whereSql} ORDER BY [t].[Schema], [t].[Name]";
                var queryResult = conn.Query(sql, whereParams);
                if (queryResult.Any())
                {
                    var columns = _schemaContext.Columns.GetAllForTableTypes();
                    foreach (var q in queryResult)
                    {
                        var obj = new TableType()
                        {
                            Schema = q.Schema,
                            Name = q.Name,
                            IsNullable = q.IsNullable ?? false,
                        };

                        obj.Columns = (
                            from c in columns
                            where c.ParentName.Equals(obj.Name, StringComparison.OrdinalIgnoreCase)
                            && c.ParentSchema.Equals(obj.Schema, StringComparison.OrdinalIgnoreCase)
                            select c).ToList();

                        data.Add(obj);
                    }
                }
            }
            return data;
        }
    }
}
