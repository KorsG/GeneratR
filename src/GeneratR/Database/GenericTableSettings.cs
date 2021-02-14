using System;
using GeneratR.Database.SqlServer.Schema;
using GeneratR.DotNet;

namespace GeneratR.Database
{
    public class GenericTableSettings
    {
        public GenericTableSettings()
        {
            Namespace = string.Empty;
            NamingStrategy = NamingStrategy.KeepOriginal;
            ForeignKeyNamingStrategy = ForeignKeyNamingStrategy.ReferencingTableName;
            ForeignKeyCollectionType = ForeignKeyCollectionType.ICollection;
            DefaultColumnDotNetModifier = DotNetModifierKeyword.Public;
            DefaultForeignKeyDotNetModifier = DotNetModifierKeyword.Public;
        }

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

        public bool GenerateForeignKeys { get; set; }
        public bool GenerateReferencingForeignKeys { get; set; }

        public NamingStrategy NamingStrategy { get; set; }
        public ForeignKeyNamingStrategy ForeignKeyNamingStrategy { get; set; }
        public ForeignKeyCollectionType ForeignKeyCollectionType { get; set; }

        public DotNetModifierKeyword DefaultColumnDotNetModifier { get; set; }
        public DotNetModifierKeyword DefaultForeignKeyDotNetModifier { get; set; }

        public Func<Table, bool> ShouldInclude { get; set; } = x => true;
    }
}
