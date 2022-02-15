namespace GeneratR.Database.SqlServer.Templates
{
    public class TemplateModel<TObject>
    {
        public TemplateModel(SqlServerSchemaGenerator generator, TObject @object)
        {
            Generator = generator;
            Object = @object;
        }

        public SqlServerSchemaGenerator Generator { get; }
        public TObject Object { get; }
    }
}
