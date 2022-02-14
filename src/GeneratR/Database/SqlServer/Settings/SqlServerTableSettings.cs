using GeneratR.Database.SqlServer.Templates;
using GeneratR.Templating;
using System;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTableSettings : GenericTableSettings
    {
        public SqlServerTableSettings()
            : base()
        {
        }

        public Func<TableTemplateContext, ITemplate> TemplateFactory { get; set; } = (ctx) => new DefaultTableTemplate(ctx);
    }
}
