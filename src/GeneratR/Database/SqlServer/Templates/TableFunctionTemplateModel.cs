namespace GeneratR.Database.SqlServer.Templates
{
    public class TableFunctionTemplateModel : TemplateModel<SqlServerTableFunctionConfiguration>
    {
        public TableFunctionTemplateModel(SqlServerSchemaGenerator generator, SqlServerTableFunctionConfiguration @object)
            : base(generator, @object) { }
    }
}
