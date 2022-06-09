using System.Collections.Generic;
using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTableFunctionSettings
    {
        public SqlServerTableFunctionSettings()
        {
        }

        public SqlServerTableFunctionSettings Clone()
        {
            return (SqlServerTableFunctionSettings)MemberwiseClone();
        }

        public bool Generate { get; set; }
        public string Namespace { get; set; } = string.Empty;
        public List<string> ImplementInterfaces { get; set; } = new List<string>();
        public string InheritClass { get; set; }
        public bool AddConstructor { get; set; }
        public bool AddDataAnnotationAttributes { get; set; }
        public string OutputFolderPath { get; set; }
        public NamingStrategy NamingStrategy { get; set; } = NamingStrategy.KeepOriginal;
        public DotNetModifierKeyword Modifiers { get; set; } = DotNetModifierKeyword.Public;
        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;
    }
}
