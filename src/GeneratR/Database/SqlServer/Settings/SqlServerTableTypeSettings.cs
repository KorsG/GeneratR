using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTableTypeSettings : CodeModelSettingsBase
    {
        public SqlServerTableTypeSettings()
        {
        }

        public SqlServerTableTypeSettings Clone()
        {
            return (SqlServerTableTypeSettings)MemberwiseClone();
        }

        public bool AddSqlDataRecordMappings { get; set; }

        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;
    }
}
