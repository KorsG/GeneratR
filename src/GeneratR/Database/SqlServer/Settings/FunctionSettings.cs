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

        public bool AddDataAnnotationAttributes { get; set; }

        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;
    }
}
