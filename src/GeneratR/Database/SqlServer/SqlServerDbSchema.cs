using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerDbSchema
    {
        public SqlServerDbSchema()
        {
            Tables = new List<SqlServerTableConfiguration>();
            Views = new List<SqlServerViewConfiguration>();
            TableFunctions = new List<SqlServerTableFunctionConfiguration>();
            StoredProcedures = new List<SqlServerStoredProcedureConfiguration>();
            TableTypes = new List<SqlServerTableTypeConfiguration>();
        }

        public List<SqlServerTableConfiguration> Tables { get; }

        public List<SqlServerViewConfiguration> Views { get; }

        public List<SqlServerTableFunctionConfiguration> TableFunctions { get; }

        public List<SqlServerStoredProcedureConfiguration> StoredProcedures { get; }

        public List<SqlServerTableTypeConfiguration> TableTypes { get; }

        /// <summary>
        /// Get distinct collection of Schema names from currently loaded collections.
        /// </summary>
        public IEnumerable<string> GetSchemaNames()
        {
            return Tables.Select(q => q.DbObject.Schema)
                .Union(Views.Select(q => q.DbObject.Schema))
                .Union(TableFunctions.Select(q => q.DbObject.Schema))
                .Union(StoredProcedures.Select(q => q.DbObject.Schema))
                .Union(TableTypes.Select(q => q.DbObject.Schema))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
