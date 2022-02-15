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

        public Func<ViewTemplateModel, ITemplate> TemplateFactory { get; set; } = (x) => new ViewTemplate(x);
    }
}
