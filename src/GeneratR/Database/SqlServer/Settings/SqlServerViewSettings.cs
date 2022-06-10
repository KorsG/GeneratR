using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerViewSettings : CodeModelSettingsBase
    {
        public SqlServerViewSettings()
        {
        }

        public SqlServerViewSettings Clone()
        {
            return (SqlServerViewSettings)MemberwiseClone();
        }

        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;
    }
}
