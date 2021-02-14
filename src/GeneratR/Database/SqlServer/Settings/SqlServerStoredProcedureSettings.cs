using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneratR.Database.SqlServer.Schema;
using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerStoredProcedureSettings
    {
        public SqlServerStoredProcedureSettings()
        {
            Namespace = string.Empty;
            NamingStrategy = NamingStrategy.KeepOriginal;
            DefaultColumnDotNetModifier = DotNetModifierKeyword.Public;
        }

        public bool Generate { get; set; }
        public string Namespace { get; set; }
        public bool ClassAsPartial { get; set; }
        public bool ClassAsAbstract { get; set; }
        public string ImplementInterface { get; set; }
        public string InheritClass { get; set; }
        public bool AddConstructor { get; set; }
        public bool AddAnnotations { get; set; }
        public string OutputProjectPath { get; set; }
        public string OutputFolderPath { get; set; }

        /// <summary>
        /// Only work with SQL Server 2012+.
        /// </summary>
        public bool GenerateResultSet { get; set; } 
        public bool GenerateOutputParameters { get; set; }

        public NamingStrategy NamingStrategy { get; set; }

        public DotNetModifierKeyword DefaultColumnDotNetModifier { get; set; }

        public Func<StoredProcedure, bool> ShouldInclude { get; set; } = x => true;
    }
}
