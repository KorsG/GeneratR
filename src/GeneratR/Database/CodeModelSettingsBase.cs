using GeneratR.DotNet;
using System.Collections.Generic;

namespace GeneratR.Database
{
    public abstract class CodeModelSettingsBase
    {
        public bool Generate { get; set; }

        public string Namespace { get; set; } = string.Empty;

        public List<string> ImplementInterfaces { get; set; } = new List<string>();

        public string InheritClass { get; set; }

        public bool AddConstructor { get; set; }

        public bool AddDataAnnotationAttributes { get; set; }

        public virtual NamingStrategy NamingStrategy { get; set; } = NamingStrategy.KeepOriginal;

        public virtual DotNetModifierKeyword Modifiers { get; set; } = DotNetModifierKeyword.Public | DotNetModifierKeyword.Partial;

        public string OutputFolderPath { get; set; }
    }
}
