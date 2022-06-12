using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class TableTypeSettings : CodeModelSettingsBase
    {
        public TableTypeSettings()
        {
        }

        public TableTypeSettings Clone()
        {
            return (TableTypeSettings)MemberwiseClone();
        }

        public bool AddSqlDataRecordMappings { get; set; }

        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;
    }
}
