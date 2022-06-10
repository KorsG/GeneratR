﻿using System;
using System.Collections.Generic;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerSchemaGenerationSettings
    {
        private SqlServerTableSettings _table;
        private SqlServerViewSettings _view;
        private SqlServerTableFunctionSettings _tableFunction;
        private SqlServerTableTypeSettings _tableType;
        private SqlServerScalarFunctionSettings _scalarFunction;
        private SqlServerStoredProcedureSettings _storedProcedure;

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

        public SqlServerTableSettings Table { get { return _table; } set { _table = value ?? new SqlServerTableSettings(); } }

        public SqlServerViewSettings View { get { return _view; } set { _view = value ?? new SqlServerViewSettings(); } }

        public SqlServerTableFunctionSettings TableFunction { get { return _tableFunction; } set { _tableFunction = value ?? new SqlServerTableFunctionSettings(); } }

        public SqlServerScalarFunctionSettings ScalarFunction { get { return _scalarFunction; } set { _scalarFunction = value ?? new SqlServerScalarFunctionSettings(); } }

        public SqlServerStoredProcedureSettings StoredProcedure { get { return _storedProcedure; } set { _storedProcedure = value ?? new SqlServerStoredProcedureSettings(); } }

        public SqlServerTableTypeSettings TableType { get { return _tableType; } set { _tableType = value ?? new SqlServerTableTypeSettings(); } }
    }
}