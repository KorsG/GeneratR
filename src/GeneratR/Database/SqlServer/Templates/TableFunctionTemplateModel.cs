namespace GeneratR.Database.SqlServer.Templates
{
    public class TableFunctionTemplateModel
    {
        public TableFunctionTemplateModel(SqlServerSchemaGenerator generator, SqlServerTableFunctionConfiguration tableFunction)
        {
            Generator = generator;
            TableFunction = tableFunction;
        }

        public SqlServerSchemaGenerator Generator { get; }

        public SqlServerTableFunctionConfiguration TableFunction { get; }
    }
}
