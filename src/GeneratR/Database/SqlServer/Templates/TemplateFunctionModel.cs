namespace GeneratR.Database.SqlServer.Templates
{
    public class TemplateFunctionModel : TemplateModel<SqlServerTableFunctionConfiguration>
    {
        public TemplateFunctionModel(SqlServerSchemaGenerator generator, SqlServerTableFunctionConfiguration @object)
            : base(generator, @object) { }
    }
}
