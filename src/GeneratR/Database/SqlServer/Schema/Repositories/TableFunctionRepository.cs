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
            return GetWhere("", null);
        }

        private IEnumerable<TableFunction> GetWhere(string whereSql, object whereParams)
        {
            var data = new List<TableFunction>();
            using (var conn = _schemaContext.GetConnection())
            {
                var sqlText = $"SELECT * FROM ({SqlQueries.SelectTableFunctions}) AS [t] {whereSql} ORDER BY [t].[Schema], [t].[Name]";
                var queryResult = conn.Query(sqlText, whereParams);
                if (queryResult.Any())
                {
                    var columns = _schemaContext.Columns.GetAllForTableFunctions();
                    var param = _schemaContext.Parameters.GetAllForTableFunctions();
                    foreach (var q in queryResult)
                    {
                        var obj = new TableFunction()
                        {
                            Schema = q.Schema,
                            Name = q.Name,
                        };
                        
                        obj.Columns = (
                            from c in columns
                            where c.ParentName.Equals(obj.Name, StringComparison.OrdinalIgnoreCase)
                            && c.ParentSchema.Equals(obj.Schema, StringComparison.OrdinalIgnoreCase)
                            select c).ToList();

                        obj.Parameters = (
                            from p in param
                            where p.ParentName.Equals(obj.Name, StringComparison.OrdinalIgnoreCase)
                            && p.ParentSchema.Equals(obj.Schema, StringComparison.OrdinalIgnoreCase)
                            select p).ToList();

                        data.Add(obj);
                    }
                }
            }
            return data;
        }
    }
}
