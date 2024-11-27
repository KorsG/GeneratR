using System.Collections.Generic;
using System.Linq;

namespace GeneratR.DotNet
{
    public class ClassCodeModel
    {
        private List<PropertyCodeModel> _properties = new();

        public ClassCodeModel()
        {
        }

        public string OutputFolderPath { get; set; }

        public List<NamespaceImportCodeModel> NamespaceImports { get; set; } = new List<NamespaceImportCodeModel>();

        public string Namespace { get; set; }

        public string ClassName { get; set; }

        public string InheritClassName { get; set; }

        public List<string> ImplementInterfaces { get; set; } = new List<string>();

        public DotNetModifierKeyword DotNetModifier { get; set; }

        /// <summary>
        /// Add default parameterless constructor.
        /// </summary>
        public bool AddConstructor { get; set; }

        public virtual List<PropertyCodeModel> Properties
        {
            get => _properties;
            set { _properties = value ?? new List<PropertyCodeModel>(); }
        }

        public ClassCodeModel AddProperty(PropertyCodeModel property)
        {
            Properties.Add(property);
            return this;
        }

        ///<summary>
        /// Attributes added to the class.
        ///</summary>
        public DotNetAttributeCollection Attributes { get; set; } = new DotNetAttributeCollection();

        public ClassCodeModel AddAttribute(DotNetAttribute attribute)
        {
            if (Attributes == null) { Attributes = new DotNetAttributeCollection(); }
            Attributes.Add(attribute);
            return this;
        }

        public ClassCodeModel RemoveAttribute(string attributeName)
        {
            Attributes?.Remove(attributeName);
            return this;
        }

        public ClassCodeModel AddNamespaceImport(string @namespace, string alias = null)
        {
            NamespaceImports.Add(new NamespaceImportCodeModel(@namespace, alias));
            return this;
        }

        public ClassCodeModel AddNamespaceImports(IEnumerable<string> namespaces)
        {
            NamespaceImports.AddRange(namespaces.Select(x => new NamespaceImportCodeModel(x)));
            return this;
        }

        public ClassCodeModel AddNamespaceImports(params string[] namespaces)
        {
            NamespaceImports.AddRange(namespaces.Select(x => new NamespaceImportCodeModel(x)));
            return this;
        }

        public ClassCodeModel Clone()
        {
            var clone = (ClassCodeModel)MemberwiseClone();
            clone.Attributes = Attributes.Clone();
            clone.ImplementInterfaces = new List<string>(ImplementInterfaces);
            clone.Properties = new List<PropertyCodeModel>(Properties.Select(x => x.Clone()));
            clone.NamespaceImports = new List<NamespaceImportCodeModel>(NamespaceImports.Select(x => x.Clone()));
            return clone;
        }

        /*
        public ClassGenerationConfiguration<T> IgnoreProperty(string propertyName)
        {
            ForeignKeys.Remove(ForeignKeys.Where(q => q.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault());
            ReferencingForeignKeys.Remove(ReferencingForeignKeys.Where(q => q.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault());
            Columns.Remove(Columns.Where(q => q.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault());
            return this;
        }

        public ClassGenerationConfiguration<T> RenameProperty(string propertyName, string renameTo)
        {
            foreach (var q in ForeignKeys.Where(q => q.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).ToList())
            {
                q.PropertyName = renameTo;
            }
            foreach (var q in ReferencingForeignKeys.Where(q => q.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).ToList())
            {
                q.PropertyName = renameTo;
            }
            foreach (var q in Columns.Where(q => q.PropertyName.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).ToList())
            {
                q.PropertyName = renameTo;
            }
            return this;
        }
        */
    }
}
