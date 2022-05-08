using System;
using System.Collections.Generic;
using GeneratR.Database.SqlServer.Schema;
using GeneratR.Database.SqlServer.Templates;
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

        public Func<SqlServerTableFunctionConfiguration, string> GenerateFactory { get; set; } = (x) => new TableFunctionTemplate(x).Generate();

        public bool Generate { get; set; }
        public string Namespace { get; set; } = string.Empty;
        public List<string> ImplementInterfaces { get; set; } = new List<string>();
        public string InheritClass { get; set; }
        public bool AddConstructor { get; set; }
        public bool AddDataAnnotationAttributes { get; set; }
        public string OutputProjectPath { get; set; }
        public string OutputFolderPath { get; set; }
        public NamingStrategy NamingStrategy { get; set; } = NamingStrategy.KeepOriginal;
        public DotNetModifierKeyword Modifiers { get; set; } = DotNetModifierKeyword.Public;
        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;

        public Func<TableFunction, bool> IgnoreObject { get; set; } = x => false;

        public Action<SqlServerTableFunctionSettings, TableFunction> ApplyObjectSettings { get; set; } = null;
    }
}
