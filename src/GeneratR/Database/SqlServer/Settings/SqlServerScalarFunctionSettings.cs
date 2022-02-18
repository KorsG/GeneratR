using System;
using System.Collections.Generic;
using GeneratR.Database.SqlServer.Schema;
using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerScalarFunctionSettings
    {
        public SqlServerScalarFunctionSettings()
        {
        }

        internal SqlServerScalarFunctionSettings Clone()
        {
            return (SqlServerScalarFunctionSettings)MemberwiseClone();
        }

        public Func<SqlServerScalarFunctionConfiguration, string> GenerateFactory { get; set; } = (x) => "TODO: Not implemented";

        public bool Generate { get; set; }
        public string Namespace { get; set; } = string.Empty;
        public List<string> ImplementInterfaces { get; set; } = new List<string>();
        public string InheritClass { get; set; }
        public bool AddConstructor { get; set; }
        public bool AddDataAnnotationAttributes { get; set; }
        public string OutputProjectPath { get; set; }
        public string OutputFolderPath { get; set; }
        public NamingStrategy NamingStrategy { get; set; }= NamingStrategy.KeepOriginal;
        public DotNetModifierKeyword Modifiers { get; set; } = DotNetModifierKeyword.Public | DotNetModifierKeyword.Partial;
        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;

        public Func<ScalarFunction, bool> IgnoreObject { get; set; } = x => false;

        public Action<SqlServerTableTypeSettings, ScalarFunction> ApplyObjectSettings { get; set; } = null;
    }
}
