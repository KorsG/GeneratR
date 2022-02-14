using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer.Templates
{
    public class ViewTemplateContext : TemplateContext<SqlServerViewSettings, SqlServerViewConfiguration>
    {
        public ViewTemplateContext(DotNetGenerator dotNetGenerator, SqlServerViewSettings settings, SqlServerViewConfiguration @object) 
            : base(dotNetGenerator, settings, @object)
        {
        }
    }
}
