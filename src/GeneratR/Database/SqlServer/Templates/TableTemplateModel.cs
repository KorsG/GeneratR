namespace GeneratR.Database.SqlServer.Templates
{
    public class TableTemplateModel : TemplateModel<SqlServerTableConfiguration>
    {
        public TableTemplateModel(SqlServerSchemaGenerator generator, SqlServerTableConfiguration @object)
            : base(generator, @object) { }
    }
}
