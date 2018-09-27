using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace GeneratR.Database.SqlServer.Schema
{
    public class ColumnRepository
    {
        private readonly SqlServerSchemaContext _schemaContext;

        public ColumnRepository(SqlServerSchemaContext schemaContext)
        {
            _schemaContext = schemaContext;
        }

        public IEnumerable<Column> GetAll()
        {
            return GetWhere("", null);
        }

        public IEnumerable<Column> GetAllForTables()
        {
            return GetWhere("ParentType=@ParentType", new { ParentType = "U" });
        }

        public IEnumerable<Column> GetAllForViews()
        {
            return GetWhere("ParentType=@ParentType", new { ParentType = "V" });
        }

        public IEnumerable<Column> GetAllForTableFunctions()
        {
            return GetWhere("ParentType IN @ParentTypes", new { ParentTypes = new List<string>() { "IF", "TF" } });
        }

        public IEnumerable<Column> GetAllForTableTypes()
        {
            return GetWhere("ParentType=@ParentType", new { ParentType = "TT" });
        }

        private IEnumerable<Column> GetWhere(string whereSql, object whereParams)
        {
            if (!string.IsNullOrWhiteSpace(whereSql) && !whereSql.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
            {
                whereSql = "WHERE " + whereSql;
            }

            using (var conn = _schemaContext.GetConnection())
            {
                var sqlText = $"SELECT * FROM ({SqlQueries.SelectColumns}) AS [t] {whereSql} ORDER BY [t].[ParentSchema], [t].[ParentName], [t].[Position]";
                var result = conn.Query(sqlText, whereParams);

                var data = result.Select(q => new Column
                {
                    ParentSchema = q.ParentSchema,
                    ParentName = q.ParentName,
                    Name = q.Name,
                    DataType = q.DataType,
                    Length = (short)q.Length,
                    Precision = (byte)q.Precision,
                    Scale = (byte)q.Scale,
                    Position = (int)q.Position,
                    IsNullable = q.IsNullable ?? false,
                    IsComputed = q.IsComputed ?? false,
                    IsPrimaryKey = q.IsPrimaryKey ?? false,
                    PrimaryKeyPosition = (short)(q.PrimaryKeyPosition ?? 0),
                    IsIdentity = q.IsIdentity ?? false,
                    IdentitySeed = (long)q.IdentitySeed,
                    IdentityIncrement = (long)q.IdentityIncrement,
                    IsRowGuid = q.IsRowGuid ?? false,
                    DefaultValueDefinition = q.DefaultValueDefinition ?? string.Empty,
                    Description = q.Description ?? string.Empty,
                });

                return data;
            }
        }
    }
}
