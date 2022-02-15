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

        public Func<TableTemplateModel, ITemplate> TemplateFactory { get; set; } = (x) => new TableTemplate(x);
    }
}
