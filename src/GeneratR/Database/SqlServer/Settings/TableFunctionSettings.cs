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

        public bool AddDataAnnotationAttributes { get; set; }

        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;
    }
}
