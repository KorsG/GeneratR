using GeneratR.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.DotNet
{
    public class NamespaceImportCodeModel
    {
        public NamespaceImportCodeModel(string @namespace, string alias = null) 
        {
            Namespace = @namespace;
            Alias = alias;
            HasAlias = !string.IsNullOrWhiteSpace(alias);
        }

        public string Namespace { get; }

        public string Alias { get; }

        public bool HasAlias { get; }
    }
}
