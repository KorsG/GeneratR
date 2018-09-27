using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace GeneratR.Database.SqlServer.Schema
{
    public class IndexRepository
    {
        private readonly SqlServerSchemaContext _schemaContext;

        public IndexRepository(SqlServerSchemaContext schemaContext)
        {
            _schemaContext = schemaContext;
        }

        public IEnumerable<Index> GetAll()
        {
            return GetWhere("", null);
        }

        public IEnumerable<Index> GetAllForTables()
        {
            return GetWhere("ParentType=@ParentType", new { ParentType = "U" });
        }

        public IEnumerable<Index> GetAllForViews()
        {
            return GetWhere("ParentType=@ParentType", new { ParentType = "V" });
        }

        public IEnumerable<Index> GetAllForTableFunctions()
        {
            return GetWhere("ParentType IN @ParentTypes", new { ParentTypes = new List<string>() { "TF" } });
        }

        private IEnumerable<Index> GetWhere(string whereSql, object whereParams)
        {
            if (!string.IsNullOrWhiteSpace(whereSql) && !whereSql.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
            {
                whereSql = "WHERE " + whereSql;
            }

            var sqlText = $"SELECT * FROM ({SqlQueries.SelectIndexes}) [t] {whereSql} ORDER BY [t].[ParentSchema], [t].[ParentName], [t].[ColumnOrdinalPosition]";
            using (var conn = _schemaContext.GetConnection())
            {
                var data = conn.Query<Index>(sqlText, whereParams);
                return data;
            }
        }
    }
}
