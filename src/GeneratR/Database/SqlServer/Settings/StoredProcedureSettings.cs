using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class StoredProcedureSettings : CodeModelSettingsBase
    {
        public StoredProcedureSettings()
        {
        }

        public StoredProcedureSettings Clone()
        {
            return (StoredProcedureSettings)MemberwiseClone();
        }

        /// <summary>
        /// Only works with SQL Server 2012+.
        /// </summary>
        public bool GenerateResultSet { get; set; } 
        public bool GenerateOutputParameters { get; set; }

        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;
    }
}
