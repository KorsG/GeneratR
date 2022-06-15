using System.Collections.Generic;

namespace GeneratR.Database.SqlServer.Schema
{
    public class SqlServerSchema
    {
        public SqlServerSchema()
        {
        }

        public List<Table> Tables { get; internal set; } = new List<Table>();

        public List<View> Views { get; internal set; } = new List<View>();

        public List<TableFunction> TableFunctions { get; internal set; } = new List<TableFunction>();

        public List<TableType> TableTypes { get; internal set; } = new List<TableType>();

        public List<StoredProcedure> StoredProcedures { get; internal set; } = new List<StoredProcedure>();
    }
}
