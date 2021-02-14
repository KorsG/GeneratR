using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace GeneratR.Database.SqlServer.Schema
{
    public class SqlServerSchemaContext
    {
        private readonly string _connectionString;

        public SqlServerSchemaContext(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) { throw new ArgumentException(nameof(connectionString) + " must not be null/empty/whitespace", nameof(connectionString)); }
            _connectionString = connectionString;
            Columns = new ColumnRepository(this);
            ForeignKeys = new ForeignKeyRepository(this);
            Indexes = new IndexRepository(this);
            Tables = new TableRepository(this);
            Views = new ViewRepository(this);
            Parameters = new ParameterRepository(this);
            TableFunctions = new TableFunctionRepository(this);
            StoredProcedures = new StoredProcedureRepository(this);
            StoredProcedureResultColumns = new StoredProcedureResultColumnRepository(this);
            TableTypes = new TableTypeRepository(this);
        }

        public SqlConnection GetConnection() => new SqlConnection(_connectionString);

        public ICollection<string> IncludeSchemas { get; set; } = new HashSet<string>();

        public ICollection<string> ExcludeSchemas { get; set; } = new HashSet<string>();

        public ColumnRepository Columns { get; }

        public ForeignKeyRepository ForeignKeys { get; }

        public IndexRepository Indexes { get; }

        public TableRepository Tables { get; }

        public ViewRepository Views { get; }

        public ParameterRepository Parameters { get; }

        public TableFunctionRepository TableFunctions { get; }

        public StoredProcedureRepository StoredProcedures { get; }

        public StoredProcedureResultColumnRepository StoredProcedureResultColumns { get; }

        public TableTypeRepository TableTypes { get; }
    }
}
