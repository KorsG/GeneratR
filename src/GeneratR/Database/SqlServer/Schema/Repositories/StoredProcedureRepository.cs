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
            return GetWhere("", null, includeResultColumns);
        }

        private IEnumerable<StoredProcedure> GetWhere(string whereSql, object whereParams, bool includeResultColumns = true)
        {
            var data = new List<StoredProcedure>();
            using (var conn = _schemaContext.GetConnection())
            {
                var sql = $"SELECT * FROM ({SqlQueries.SelectStoredProcedures}) AS [t] {whereSql} ORDER BY [t].[Schema], [t].[Name]";
                var queryResult = conn.Query(sql, whereParams);
                if (queryResult.Any())
                {
                    var param = _schemaContext.Parameters.GetAllForStoredProcedures();
                    var columns = includeResultColumns ? _schemaContext.StoredProcedureResultColumns.GetAll() : null;
                    foreach (var q in queryResult)
                    {
                        var obj = new StoredProcedure()
                        {
                            Schema = q.Schema,
                            Name = q.Name,
                        };

                        obj.Parameters = (
                            from p in param
                            where p.ParentName.Equals(obj.Name, StringComparison.OrdinalIgnoreCase)
                            && p.ParentSchema.Equals(obj.Schema, StringComparison.OrdinalIgnoreCase)
                            select p).ToList();

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
