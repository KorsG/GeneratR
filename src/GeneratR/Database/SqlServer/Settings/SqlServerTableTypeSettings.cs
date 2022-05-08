using System;
using System.Collections.Generic;
using GeneratR.Database.SqlServer.Schema;
using GeneratR.Database.SqlServer.Templates;
using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTableTypeSettings
    {
        public SqlServerTableTypeSettings()
        {
        }

        public SqlServerTableTypeSettings Clone()
        {
            return (SqlServerTableTypeSettings)MemberwiseClone();
        }

        public Func<SqlServerTableTypeConfiguration, string> GenerateFactory { get; set; } = (x) => new TableTypeTemplate(x).Generate();

        /// <summary>
        /// If table types should be generated.
        /// </summary>
        public bool Generate { get; set; }

        public string Namespace { get; set; } = string.Empty;
        public List<string> ImplementInterfaces { get; set; } = new List<string>();
        public string InheritClass { get; set; }
        public bool AddConstructor { get; set; }
        public bool AddDataAnnotationAttributes { get; set; }
        public bool AddSqlDataRecordMappings { get; set; }

        public string OutputProjectPath { get; set; }
        public string OutputFolderPath { get; set; }

        public NamingStrategy NamingStrategy { get; set; } = NamingStrategy.KeepOriginal;

        public DotNetModifierKeyword Modifiers { get; set; } = DotNetModifierKeyword.Public | DotNetModifierKeyword.Partial;
        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;

        public Func<TableType, bool> IgnoreObject { get; set; } = x => false;

        public Action<SqlServerTableTypeSettings, TableType> ApplyObjectSettings { get; set; } = null;
    }
}
