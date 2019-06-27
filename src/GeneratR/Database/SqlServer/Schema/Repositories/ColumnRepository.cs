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

            var sqlText = $"SELECT * FROM ({SqlQueries.SelectColumns}) AS [t] {whereSql} ORDER BY [t].[ParentSchema], [t].[ParentName], [t].[Position]";

            using (var conn = _schemaContext.GetConnection())
            {
                var dbColumns = conn.Query(sqlText, whereParams);

                var columns = dbColumns.Select(x => new Column
                {
                    ParentObjectID = x.ParentObjectID,
                    ParentSchema = x.ParentSchema,
                    ParentName = x.ParentName,
                    Name = x.Name,
                    DataType = x.DataType,
                    Length = (short)x.Length,
                    Precision = (byte)x.Precision,
                    Scale = (byte)x.Scale,
                    Position = (int)x.Position,
                    IsNullable = x.IsNullable ?? false,
                    IsComputed = x.IsComputed ?? false,
                    IsPrimaryKey = x.IsPrimaryKey ?? false,
                    PrimaryKeyPosition = (short)(x.PrimaryKeyPosition ?? 0),
                    IsIdentity = x.IsIdentity ?? false,
                    IdentitySeed = (long)x.IdentitySeed,
                    IdentityIncrement = (long)x.IdentityIncrement,
                    IsRowGuid = x.IsRowGuid ?? false,
                    DefaultValueDefinition = x.DefaultValueDefinition ?? string.Empty,
                    Description = x.Description ?? string.Empty,
                });

                return columns;
            }
        }
    }
}
