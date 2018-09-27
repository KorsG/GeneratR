namespace GeneratR.Database.SqlServer
{
    public class SqlServerSchemaGeneratorSettings : GenericDbSchemaGeneratorSettings
    {
        public SqlServerSchemaGeneratorSettings()
        {
            Table = new SqlServerTableSettings();
            View = new SqlServerViewSettings();
            TableFunction = new SqlServerTableFunctionSettings();
            ScalarFunction = new SqlServerScalarFunctionSettings();
            StoredProcedure = new SqlServerStoredProcedureSettings();
            TableType = new SqlServerTableTypeSettings();
        }

        public new SqlServerTableSettings Table { get { return _Table; } set { base.Table = value ?? new SqlServerTableSettings(); _Table = value ?? new SqlServerTableSettings(); } }
        private SqlServerTableSettings _Table;

        public new SqlServerViewSettings View { get { return _View; } set { base.View = value ?? new SqlServerViewSettings(); _View = value ?? new SqlServerViewSettings(); } }
        private SqlServerViewSettings _View;

        public SqlServerTableFunctionSettings TableFunction { get { return _TableFunction; } set { _TableFunction = value ?? new SqlServerTableFunctionSettings(); } }
        private SqlServerTableFunctionSettings _TableFunction;

        public SqlServerScalarFunctionSettings ScalarFunction { get { return _ScalarFunction; } set { _ScalarFunction = value ?? new SqlServerScalarFunctionSettings(); } }
        private SqlServerScalarFunctionSettings _ScalarFunction;

        public SqlServerStoredProcedureSettings StoredProcedure { get { return _StoredProcedure; } set { _StoredProcedure = value ?? new SqlServerStoredProcedureSettings(); } }
        private SqlServerStoredProcedureSettings _StoredProcedure;

        public SqlServerTableTypeSettings TableType { get { return _TableType; } set { _TableType = value ?? new SqlServerTableTypeSettings(); } }
        private SqlServerTableTypeSettings _TableType;
    }
}
