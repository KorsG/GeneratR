using System.Collections.Generic;
using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerStoredProcedureSettings
    {
        public SqlServerStoredProcedureSettings()
        {
        }

        public SqlServerStoredProcedureSettings Clone()
        {
            return (SqlServerStoredProcedureSettings)MemberwiseClone();
        }

        public bool Generate { get; set; }
        public string Namespace { get; set; } = string.Empty;
        public List<string> ImplementInterfaces { get; set; } = new List<string>();
        public string InheritClass { get; set; }
        public bool AddConstructor { get; set; }
        public bool AddDataAnnotationAttributes { get; set; }
        public string OutputFolderPath { get; set; }

        /// <summary>
        /// Only works with SQL Server 2012+.
        /// </summary>
        public bool GenerateResultSet { get; set; } 
        public bool GenerateOutputParameters { get; set; }

        public NamingStrategy NamingStrategy { get; set; } = NamingStrategy.KeepOriginal;
        public DotNetModifierKeyword Modifiers { get; set; } = DotNetModifierKeyword.Public | DotNetModifierKeyword.Partial;
        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;
    }
}
