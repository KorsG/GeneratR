using System;
using System.Collections.Generic;
using GeneratR.Database.SqlServer.Schema;
using GeneratR.Database.SqlServer.Templates;
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

        public Func<SqlServerStoredProcedureConfiguration, string> GenerateFactory { get; set; } = (x) => new StoredProcedureTemplate(x).Generate();

        public bool Generate { get; set; }
        public string Namespace { get; set; } = string.Empty;
        public List<string> ImplementInterfaces { get; set; } = new List<string>();
        public string InheritClass { get; set; }
        public bool AddConstructor { get; set; }
        public bool AddDataAnnotationAttributes { get; set; }
        public string OutputProjectPath { get; set; }
        public string OutputFolderPath { get; set; }

        /// <summary>
        /// Only works with SQL Server 2012+.
        /// </summary>
        public bool GenerateResultSet { get; set; } 
        public bool GenerateOutputParameters { get; set; }

        public NamingStrategy NamingStrategy { get; set; } = NamingStrategy.KeepOriginal;
        public DotNetModifierKeyword Modifiers { get; set; } = DotNetModifierKeyword.Public | DotNetModifierKeyword.Partial;
        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;

        public Func<StoredProcedure, bool> IgnoreObject { get; set; } = x => false;

        public Action<SqlServerStoredProcedureSettings, StoredProcedure> ApplyObjectSettings { get; set; } = null;
    }
}
