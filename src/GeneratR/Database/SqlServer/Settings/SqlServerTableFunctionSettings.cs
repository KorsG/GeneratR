using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTableFunctionSettings : CodeModelSettingsBase
    {
        public SqlServerTableFunctionSettings()
        {
        }

        public SqlServerTableFunctionSettings Clone()
        {
            return (SqlServerTableFunctionSettings)MemberwiseClone();
        }

        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;
    }
}
