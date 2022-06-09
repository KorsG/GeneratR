using GeneratR.DotNet;
using System.Collections.Generic;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTableSettings
    {
        public SqlServerTableSettings()
        {
        }

        public SqlServerTableSettings Clone()
        {
            return (SqlServerTableSettings)MemberwiseClone();
        }

        public bool Generate { get; set; }
        public string Namespace { get; set; } = string.Empty;
        public List<string> ImplementInterfaces { get; set; } = new List<string>();
        public string InheritClass { get; set; }
        public bool AddConstructor { get; set; }
        public bool AddDataAnnotationAttributes { get; set; }
        public string OutputFolderPath { get; set; }

        public bool GenerateForeignKeys { get; set; }
        public bool GenerateReferencingForeignKeys { get; set; }

        public NamingStrategy NamingStrategy { get; set; } = NamingStrategy.KeepOriginal;
        public ForeignKeyNamingStrategy ForeignKeyNamingStrategy { get; set; } = ForeignKeyNamingStrategy.ReferencingTableName;
        public ForeignKeyCollectionType ForeignKeyCollectionType { get; set; } = ForeignKeyCollectionType.ICollection;

        public DotNetModifierKeyword Modifiers { get; set; } = DotNetModifierKeyword.Public | DotNetModifierKeyword.Partial;
        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;
        public DotNetModifierKeyword ForeignKeyModifiers { get; set; } = DotNetModifierKeyword.Public;
    }
}
