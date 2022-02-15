using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerSchemaGeneratorSettings : GenericDbSchemaGeneratorSettings
    {
        private SqlServerTableSettings _table;
        private SqlServerViewSettings _view;
        private SqlServerTableFunctionSettings _tableFunction;
        private SqlServerTableTypeSettings _tableType;
        private SqlServerScalarFunctionSettings _scalarFunction;
        private SqlServerStoredProcedureSettings _storedProcedure;

        public SqlServerSchemaGeneratorSettings(DotNetLanguageType dotNetLanguage = DotNetLanguageType.CS)
           : this(DotNetGenerator.Create(dotNetLanguage))
        {
        }

        public SqlServerSchemaGeneratorSettings(DotNetGenerator dotNetGenerator)
            : base(dotNetGenerator)
        {
            Table = new SqlServerTableSettings();
            View = new SqlServerViewSettings();
            TableFunction = new SqlServerTableFunctionSettings();
            ScalarFunction = new SqlServerScalarFunctionSettings();
            StoredProcedure = new SqlServerStoredProcedureSettings();
            TableType = new SqlServerTableTypeSettings();
        }

        public new SqlServerTableSettings Table { get { return _table; } set { base.Table = value ?? new SqlServerTableSettings(); _table = value ?? new SqlServerTableSettings(); } }

        public new SqlServerViewSettings View { get { return _view; } set { base.View = value ?? new SqlServerViewSettings(); _view = value ?? new SqlServerViewSettings(); } }

        public SqlServerTableFunctionSettings TableFunction { get { return _tableFunction; } set { _tableFunction = value ?? new SqlServerTableFunctionSettings(); } }

        public SqlServerScalarFunctionSettings ScalarFunction { get { return _scalarFunction; } set { _scalarFunction = value ?? new SqlServerScalarFunctionSettings(); } }

        public SqlServerStoredProcedureSettings StoredProcedure { get { return _storedProcedure; } set { _storedProcedure = value ?? new SqlServerStoredProcedureSettings(); } }

        public SqlServerTableTypeSettings TableType { get { return _tableType; } set { _tableType = value ?? new SqlServerTableTypeSettings(); } }

    }
}
