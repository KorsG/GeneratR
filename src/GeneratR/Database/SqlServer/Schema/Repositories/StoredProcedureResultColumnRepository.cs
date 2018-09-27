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
            return GetWhere("", null);
        }

        private IEnumerable<StoredProcedureResultColumn> GetWhere(string whereSql, object whereParams)
        {
            var data = new List<StoredProcedureResultColumn>();
            using (var conn = _schemaContext.GetConnection())
            {
                var sql = $"SELECT * FROM ({SqlQueries.SelectStoredProcedureResultColumns}) AS [t1] {whereSql} ORDER BY [t1].[ParentSchema], [t1].[ParentName], [t1].[Position]";
                var queryResult = conn.Query(sql, whereParams);
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
