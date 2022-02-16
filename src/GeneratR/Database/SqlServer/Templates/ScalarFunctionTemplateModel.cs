namespace GeneratR.Database.SqlServer.Templates
{
    public class ScalarFunctionTemplateModel 
    {
        public ScalarFunctionTemplateModel(SqlServerSchemaGenerator generator, SqlServerScalarFunctionConfiguration scalarFunction)
        {
            Generator = generator;
            ScalarFunction = scalarFunction;
        }

        public SqlServerSchemaGenerator Generator { get; }

        public SqlServerScalarFunctionConfiguration ScalarFunction { get; }
    }
}
