using System;
using GeneratR.Database.SqlServer.Schema;
using GeneratR.Database.SqlServer.Templates;
using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTableFunctionSettings
    {
        public SqlServerTableFunctionSettings()
        {
            Namespace = string.Empty;
            NamingStrategy = NamingStrategy.KeepOriginal;
            DefaultColumnDotNetModifier = DotNetModifierKeyword.Public;
        }

        public Func<SqlServerTableFunctionConfiguration, string> GenerateFactory { get; set; } = (x) => new TableFunctionTemplate(x).Generate();

        public bool Generate { get; set; }
        public string Namespace { get; set; }
        public bool ClassAsPartial { get; set; }
        public bool ClassAsAbstract { get; set; }
        public string ImplementInterface { get; set; }
        public string InheritClass { get; set; }
        public bool AddConstructor { get; set; }
        public bool AddAnnotations { get; set; }
        public string OutputProjectPath { get; set; }
        public string OutputFolderPath { get; set; }

        public NamingStrategy NamingStrategy { get; set; }

        public DotNetModifierKeyword DefaultColumnDotNetModifier { get; set; }

        public Func<TableFunction, bool> ShouldInclude { get; set; } = x => true;
    }
}
