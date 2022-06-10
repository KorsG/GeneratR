using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerScalarFunctionSettings : CodeModelSettingsBase
    {
        public SqlServerScalarFunctionSettings()
        {
        }

        public SqlServerScalarFunctionSettings Clone()
        {
            return (SqlServerScalarFunctionSettings)MemberwiseClone();
        }

        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;
    }
}
