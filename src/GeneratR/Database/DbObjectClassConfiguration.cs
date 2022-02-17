using System;
using System.Collections.Generic;
using GeneratR.DotNet;

namespace GeneratR.Database
{
    public class DbObjectClassConfiguration<T> where T : class
    {
        public DbObjectClassConfiguration(T dbObject, DotNetGenerator dotNetGenerator)
        {
            DbObject = dbObject;
            DotNetGenerator = dotNetGenerator;
            Attributes = new DotNetAttributeCollection();
        }

        /// <summary>
        /// Function to Generate .NET type for this config.
        /// </summary>
        public Func<string> Generate { get; set; }

        /// <summary>
        /// The underlying database object.
        /// </summary>
        public T DbObject { get; }

        public DotNetGenerator DotNetGenerator { get; set; }

        public string Namespace { get; set; }

        public string ClassName { get; set; }

        public string InheritClassName { get; set; }

        public List<string> ImplementInterfaces { get; set; } = new List<string>();

        public DotNetModifierKeyword DotNetModifier { get; set; }

        public bool AddConstructor { get; set; }

        public bool AddDataAnnotationAttributes { get; set; }

        ///<summary>
        /// List of attributes added to the class.
        ///</summary>
        public DotNetAttributeCollection Attributes { get; set; }

        public DbObjectClassConfiguration<T> AddAttribute(DotNetAttribute attribute)
        {
            if (Attributes == null) { Attributes = new DotNetAttributeCollection(); }
            Attributes.Add(attribute);
            return this;
        }

        public DbObjectClassConfiguration<T> RemoveAttribute(string attributeName)
        {
            Attributes?.Remove(attributeName);
            return this;
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
