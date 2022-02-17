using GeneratR.Database.SqlServer.Schema;
using GeneratR.Database.SqlServer.Templates;
using GeneratR.DotNet;
using System;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerViewSettings
    {
        public SqlServerViewSettings()
        {
            Namespace = string.Empty;
            NamingStrategy = NamingStrategy.KeepOriginal;
            DefaultClassDotNetModifier = DotNetModifierKeyword.Public;
            DefaultColumnDotNetModifier = DotNetModifierKeyword.Public;
        }

        public Func<SqlServerViewConfiguration, string> GenerateFactory { get; set; } = (x) => new ViewTemplate(x).Generate();

        public bool Generate { get; set; }
        public string Namespace { get; set; }
        public string ImplementInterface { get; set; }
        public string InheritClass { get; set; }
        public bool AddConstructor { get; set; }
        public bool AddDataAnnotationAttributes { get; set; }
        public string OutputProjectPath { get; set; }
        public string OutputFolderPath { get; set; }

        public NamingStrategy NamingStrategy { get; set; }

        public DotNetModifierKeyword DefaultClassDotNetModifier { get; set; }
        public DotNetModifierKeyword DefaultColumnDotNetModifier { get; set; }

        public Func<View, bool> ShouldInclude { get; set; } = x => true;
    }
}
