using GeneratR.Database.SqlServer.Schema;
using GeneratR.Database.SqlServer.Templates;
using GeneratR.DotNet;
using System;
using System.Collections.Generic;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerViewSettings
    {
        public SqlServerViewSettings()
        {
        }

        internal SqlServerViewSettings Clone()
        {
            return (SqlServerViewSettings)MemberwiseClone();
        }

        public Func<SqlServerViewConfiguration, string> GenerateFactory { get; set; } = (x) => new ViewTemplate(x).Generate();

        public bool Generate { get; set; }
        public string Namespace { get; set; } = string.Empty;
        public List<string> ImplementInterfaces { get; set; } = new List<string>();
        public string InheritClass { get; set; }
        public bool AddConstructor { get; set; }
        public bool AddDataAnnotationAttributes { get; set; }
        public string OutputProjectPath { get; set; }
        public string OutputFolderPath { get; set; }
        public NamingStrategy NamingStrategy { get; set; } = NamingStrategy.KeepOriginal;
        public DotNetModifierKeyword Modifiers { get; set; } = DotNetModifierKeyword.Public | DotNetModifierKeyword.Partial;
        public DotNetModifierKeyword ColumnModifiers { get; set; } = DotNetModifierKeyword.Public;

        public Func<View, bool> IgnoreObject { get; set; } = x => false;

        public Action<SqlServerViewSettings, View> ApplyObjectSettings { get; set; } = null;
    }
}
