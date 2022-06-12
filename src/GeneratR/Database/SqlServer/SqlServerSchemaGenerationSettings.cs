using System;
using System.Collections.Generic;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerSchemaGenerationSettings
    {
        private TableSettings _table;
        private ViewSettings _view;
        private TableFunctionSettings _tableFunction;
        private TableTypeSettings _tableType;
        private FunctionSettings _scalarFunction;
        private StoredProcedureSettings _storedProcedure;

        public SqlServerSchemaGenerationSettings()
        {
        }

        /// <summary>
        /// Inherited from ConnectionString if not set.
        /// </summary>
        public string DatabaseName { get; set; }

        public string ConnectionString { get; set; }

        [Obsolete("Not supported")]
        public string ConnectionStringName { get; set; }

        public ICollection<string> IncludeSchemas { get; set; } = new List<string>();

        public ICollection<string> ExcludeSchemas { get; set; } = new List<string>();

        public ICollection<string> IncludeObjects { get; set; } = new List<string>();

        public ICollection<string> ExcludeObjects { get; set; } = new List<string>();

        public string RootNamespace { get; set; } = string.Empty;

        public string RootOutputFolderPath { get; set; } = string.Empty;

        public TableSettings Table { get { return _table; } set { _table = value ?? new TableSettings(); } }

        public ViewSettings View { get { return _view; } set { _view = value ?? new ViewSettings(); } }

        public TableFunctionSettings TableFunction { get { return _tableFunction; } set { _tableFunction = value ?? new TableFunctionSettings(); } }

        public FunctionSettings ScalarFunction { get { return _scalarFunction; } set { _scalarFunction = value ?? new FunctionSettings(); } }

        public StoredProcedureSettings StoredProcedure { get { return _storedProcedure; } set { _storedProcedure = value ?? new StoredProcedureSettings(); } }

        public TableTypeSettings TableType { get { return _tableType; } set { _tableType = value ?? new TableTypeSettings(); } }
    }
}
