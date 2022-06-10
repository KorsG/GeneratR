using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerStoredProcedureSettings : CodeModelSettingsBase
    {
        public SqlServerStoredProcedureSettings()
        {
        }

        public SqlServerStoredProcedureSettings Clone()
        {
            return (SqlServerStoredProcedureSettings)MemberwiseClone();
        }

        /// <summary>
        /// Only works with SQL Server 2012+.
        /// </summary>
        public bool GenerateResultSet { get; set; } 
        public bool GenerateOutputParameters { get; set; }

        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;
    }
}
