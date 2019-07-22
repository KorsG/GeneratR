using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace GeneratR.Database.SqlServer.Schema
{
    public class TableRepository
    {
        private readonly SqlServerSchemaContext _schemaContext;

        public TableRepository(SqlServerSchemaContext schemaContext)
        {
            _schemaContext = schemaContext;
        }

        public IEnumerable<Table> GetAll()
        {
            return GetWhere(string.Empty, null);
        }

        private IEnumerable<Table> GetWhere(string whereSql, object whereParams)
        {
            if (!string.IsNullOrWhiteSpace(whereSql) && !whereSql.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
            {
                whereSql = "WHERE " + whereSql;
            }

            var tables = new List<Table>();
            var sqlText = $"SELECT * FROM ({SqlQueries.SelectTables}) AS [t] {whereSql} ORDER BY [t].[Schema], [t].[Name]";

            using (var conn = _schemaContext.GetConnection())
            {
                var dbTables = conn.Query(sqlText, whereParams).ToList();
                if (dbTables.Any())
                {
                    // Load relations.
                    var indexLookup = _schemaContext.Indexes.GetAllForTables().ToLookup(x => x.ParentObjectID);
                    var columnLookup = _schemaContext.Columns.GetAllForTables().ToLookup(x => x.ParentObjectID);
                    var foreignKeys = _schemaContext.ForeignKeys.GetAll().ToList();
                    var foreignKeyToLookup = foreignKeys.ToLookup(x => x.ToObjectID);
                    var foreignKeyFromLookup = foreignKeys.ToLookup(x => x.FromObjectID);

                    // Map.
                    tables = dbTables
                        .Select(x => new Table()
                        {
                            ObjectID = x.ObjectID,
                            Schema = x.Schema,
                            Name = x.Name,
                            Columns = columnLookup[(int)x.ObjectID].ToList(),
                            Indexes = indexLookup[(int)x.ObjectID].ToList(),
                            ForeignKeys = foreignKeyFromLookup[(int)x.ObjectID].ToList(),
                            ReferencingForeignKeys = foreignKeyToLookup[(int)x.ObjectID].ToList(),
                        }).ToList();
                }
            }

            return tables;
        }
    }
}
