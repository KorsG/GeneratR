using System.Collections.Generic;

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

        public NamespaceImportCodeModel Clone()
        {
            var clone = (NamespaceImportCodeModel)MemberwiseClone();
            return clone;
        }
    }
}
