using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class ViewSettings : CodeModelSettingsBase
    {
        public ViewSettings()
        {
        }

        public ViewSettings Clone()
        {
            return (ViewSettings)MemberwiseClone();
        }

        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;
    }
}
