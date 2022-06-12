using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class TableFunctionSettings : CodeModelSettingsBase
    {
        public TableFunctionSettings()
        {
        }

        public TableFunctionSettings Clone()
        {
            return (TableFunctionSettings)MemberwiseClone();
        }

        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;
    }
}
