using GeneratR.DotNet;
using System.Collections.Generic;

namespace GeneratR.Database.SqlServer
{
    public class LinqToDbSqlServerGeneratorSettings : SqlServerSchemaGenerationSettings
    {
        public LinqToDbSqlServerGeneratorSettings()
        {
        }

        public bool AddLinqToDbMappingAttributes { get; set; }

        public DataConnectionSettings DataConnection { get; set; } = new DataConnectionSettings();

        public class DataConnectionSettings  
        {
            public bool Generate { get; set; }

            public string ClassName { get; set; }

            public string Namespace { get; set; } = string.Empty;

            public List<string> ImplementInterfaces { get; set; } = new List<string>();

            public string InheritClass { get; set; }

            public bool AddConstructor { get; set; }

            public virtual DotNetModifierKeyword Modifiers { get; set; } = DotNetModifierKeyword.Public | DotNetModifierKeyword.Partial;

            public string OutputFolderPath { get; set; }
        }
    }
}
