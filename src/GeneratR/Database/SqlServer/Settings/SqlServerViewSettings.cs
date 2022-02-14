using GeneratR.Database.SqlServer.Templates;
using GeneratR.Templating;
using System;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerViewSettings : GenericViewSettings
    {
        public SqlServerViewSettings()
            : base()
        {
        }

        public Func<ViewTemplateContext, ITemplate> TemplateFactory { get; set; } 
            = (ctx) => new DefaultViewTemplate(ctx);
    }
}
