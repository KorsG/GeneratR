using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer.Templates
{
    public class ViewTemplateModel : TemplateModel<SqlServerViewConfiguration>
    {
        public ViewTemplateModel(SqlServerSchemaGenerator generator, SqlServerViewConfiguration @object)
            : base(generator, @object) { }
    }
}
