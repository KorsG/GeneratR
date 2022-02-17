using GeneratR.Database.SqlServer.Schema;
using GeneratR.Database.SqlServer.Templates;
using GeneratR.DotNet;
using System;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTableSettings
    {
        public SqlServerTableSettings()
        {
            Namespace = string.Empty;
            NamingStrategy = NamingStrategy.KeepOriginal;
            ForeignKeyNamingStrategy = ForeignKeyNamingStrategy.ReferencingTableName;
            ForeignKeyCollectionType = ForeignKeyCollectionType.ICollection;
            DefaultClassDotNetModifier = DotNetModifierKeyword.Public;
            DefaultColumnDotNetModifier = DotNetModifierKeyword.Public;
            DefaultForeignKeyDotNetModifier = DotNetModifierKeyword.Public;
        }

        public Func<SqlServerTableConfiguration, string> GenerateFactory { get; set; } = (x) => new TableTemplate(x).Generate();

        public bool Generate { get; set; }
        public string Namespace { get; set; }
        public string ImplementInterface { get; set; }
        public string InheritClass { get; set; }
        public bool AddConstructor { get; set; }
        public bool AddDataAnnotationAttributes { get; set; }
        public string OutputProjectPath { get; set; }
        public string OutputFolderPath { get; set; }

        public bool GenerateForeignKeys { get; set; }
        public bool GenerateReferencingForeignKeys { get; set; }

        public NamingStrategy NamingStrategy { get; set; }
        public ForeignKeyNamingStrategy ForeignKeyNamingStrategy { get; set; }
        public ForeignKeyCollectionType ForeignKeyCollectionType { get; set; }

        public DotNetModifierKeyword DefaultClassDotNetModifier { get; set; }
        public DotNetModifierKeyword DefaultColumnDotNetModifier { get; set; }
        public DotNetModifierKeyword DefaultForeignKeyDotNetModifier { get; set; }

        public Func<Table, bool> ShouldInclude { get; set; } = x => true;
    }
}
