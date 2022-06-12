using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class FunctionSettings : CodeModelSettingsBase
    {
        public FunctionSettings()
        {
        }

        public FunctionSettings Clone()
        {
            return (FunctionSettings)MemberwiseClone();
        }

        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;
    }
}
