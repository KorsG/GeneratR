﻿using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class TableSettings : CodeModelSettingsBase
    {
        public TableSettings()
        {
        }

        public TableSettings Clone()
        {
            return (TableSettings)MemberwiseClone();
        }

        public bool AddDataAnnotationAttributes { get; set; }

        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;

        public bool GenerateForeignKeys { get; set; }
        public bool GenerateReferencingForeignKeys { get; set; }

        public DotNetModifierKeyword ForeignKeyModifiers { get; set; } = DotNetModifierKeyword.Public;
        public ForeignKeyNamingStrategy ForeignKeyNamingStrategy { get; set; } = ForeignKeyNamingStrategy.ReferencingTableName;
        public ForeignKeyCollectionType ForeignKeyCollectionType { get; set; } = ForeignKeyCollectionType.ICollection;
    }
}
