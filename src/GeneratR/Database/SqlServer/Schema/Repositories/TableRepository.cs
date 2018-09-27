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
            return GetWhere("", null);
        }

        private IEnumerable<Table> GetWhere(string whereSql, object whereParams)
        {
            if (!string.IsNullOrWhiteSpace(whereSql) && !whereSql.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
            {
                whereSql = "WHERE " + whereSql;
            }

            var tables = new List<Table>();

            using (var conn = _schemaContext.GetConnection())
            {
                var sqlText = $"SELECT * FROM ({SqlQueries.SelectTables}) AS [t] {whereSql} ORDER BY [t].[Schema], [t].[Name]";
                var result = conn.Query(sqlText, whereParams).ToList();
                if (result.Any())
                {
                    var columns = _schemaContext.Columns.GetAllForTables().ToList();
                    var foreignKeys = _schemaContext.ForeignKeys.GetAll().ToList();
                    var indexes = _schemaContext.Indexes.GetAllForTables().ToList();
                    foreach (var q in result)
                    {
                        var tbl = new Table
                        {
                            TableID = q.TableID,
                            Schema = q.Schema,
                            Name = q.Name,
                        };

                        tbl.Columns = (
                            from c in columns
                            where c.ParentName.Equals(tbl.Name, StringComparison.OrdinalIgnoreCase)
                            && c.ParentSchema.Equals(tbl.Schema, StringComparison.OrdinalIgnoreCase)
                            select c).ToList();

                        tbl.Indexes = (
                            from i in indexes
                            where i.ParentSchema.Equals(tbl.Schema, StringComparison.OrdinalIgnoreCase)
                            && i.ParentName.Equals(tbl.Name, StringComparison.OrdinalIgnoreCase)
                            select i).ToList();

                        tbl.ForeignKeys = (
                            from f in foreignKeys
                            where (f.FromSchema.Equals(tbl.Schema, StringComparison.OrdinalIgnoreCase)
                            && f.FromName.Equals(tbl.Name, StringComparison.OrdinalIgnoreCase))
                            select f).ToList();

                        tbl.ReferencingForeignKeys = (
                            from f in foreignKeys
                            where (f.ToSchema.Equals(tbl.Schema, StringComparison.OrdinalIgnoreCase)
                            && f.ToName.Equals(tbl.Name, StringComparison.OrdinalIgnoreCase))
                            select f).ToList();

                        tables.Add(tbl);
                    }
                }
            }
            return tables;
        }
    }
}
