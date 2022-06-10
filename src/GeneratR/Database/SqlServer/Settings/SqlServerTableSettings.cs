using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTableSettings : CodeModelSettingsBase
    {
        public SqlServerTableSettings()
        {
        }

        public SqlServerTableSettings Clone()
        {
            return (SqlServerTableSettings)MemberwiseClone();
        }

        public bool GenerateForeignKeys { get; set; }
        public bool GenerateReferencingForeignKeys { get; set; }

        public ForeignKeyNamingStrategy ForeignKeyNamingStrategy { get; set; } = ForeignKeyNamingStrategy.ReferencingTableName;
        public ForeignKeyCollectionType ForeignKeyCollectionType { get; set; } = ForeignKeyCollectionType.ICollection;

        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;
        public DotNetModifierKeyword ForeignKeyModifiers { get; set; } = DotNetModifierKeyword.Public;
    }
}
