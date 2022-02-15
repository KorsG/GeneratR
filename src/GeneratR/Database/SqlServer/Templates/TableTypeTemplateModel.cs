namespace GeneratR.Database.SqlServer.Templates
{
    public class TableTypeTemplateModel : TemplateModel<SqlServerTableTypeConfiguration>
    {
        public TableTypeTemplateModel(SqlServerSchemaGenerator generator, SqlServerTableTypeConfiguration @object)
            : base(generator, @object) { }
    }
}
