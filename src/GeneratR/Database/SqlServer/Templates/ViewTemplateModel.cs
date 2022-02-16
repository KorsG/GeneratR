namespace GeneratR.Database.SqlServer.Templates
{
    public class ViewTemplateModel 
    {
        public ViewTemplateModel(SqlServerSchemaGenerator generator, SqlServerViewConfiguration view)
        {
            Generator = generator;
            View = view;
        }

        public SqlServerSchemaGenerator Generator { get; }

        public SqlServerViewConfiguration View { get; }
    }
}
