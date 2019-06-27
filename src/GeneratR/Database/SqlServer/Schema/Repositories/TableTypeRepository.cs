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
            return GetWhere(string.Empty, null);
        }

        private IEnumerable<TableType> GetWhere(string whereSql, object whereParams)
        {
            var tableTypes = new List<TableType>();
            var sqlText = $"SELECT * FROM ({SqlQueries.SelectTableTypes}) AS [t] {whereSql} ORDER BY [t].[Schema], [t].[Name]";

            using (var conn = _schemaContext.GetConnection())
            {
                var dbTableTypes = conn.Query(sqlText, whereParams);
                if (dbTableTypes.Any())
                {
                    // Load relations.
                    var columnLookup = _schemaContext.Columns.GetAllForTableTypes().ToLookup(x => x.ParentObjectID);

                    // Map.
                    tableTypes = dbTableTypes
                        .Select(x => new TableType()
                        {
                            ObjectID = x.ObjectID,
                            Schema = x.Schema,
                            Name = x.Name,
                            Columns = columnLookup[(int)x.ObjectID].ToList()
                        }).ToList();

                    return tableTypes;
                }
            }

            return tableTypes;
        }
    }
}
