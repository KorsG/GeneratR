using System;
using GeneratR.Database.SqlServer.Schema;
using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerScalarFunctionSettings
    {
        public SqlServerScalarFunctionSettings()
        {
            Namespace = string.Empty;
            NamingStrategy = NamingStrategy.KeepOriginal;
            DefaultClassDotNetModifier = DotNetModifierKeyword.Public;
            DefaultColumnDotNetModifier = DotNetModifierKeyword.Public;
        }

        public Func<SqlServerScalarFunctionConfiguration, string> GenerateFactory { get; set; } = (x) => "TODO: Not implemented";

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

        public Func<ScalarFunction, bool> ShouldInclude { get; set; } = x => true;
    }
}
