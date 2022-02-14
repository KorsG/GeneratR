using System.Collections.Generic;
using GeneratR.DotNet;

namespace GeneratR.Database
{
    public class DbObjectClassConfiguration<T> where T : class
    {
        public DbObjectClassConfiguration(T dbObject)
        {
            DbObject = dbObject;
            IncludeAttributes = new DotNetAttributeCollection();
            ExcludeAttributes = new List<string>();
        }

        public T DbObject { get; }

        public string Namespace { get; set; }

        public string ClassName { get; set; }

        public string InheritClassName { get; set; }

        public DotNetModifierKeyword DotNetModifier { get; set; }

        ///<summary>
        /// List of attributes which will be included. Replaces any auto generated attributes. Exclude takes precedence over includes.
        ///</summary>
        public DotNetAttributeCollection IncludeAttributes { get; set; }

        ///<summary>
        /// List of attribute names which will be exluded. Exclude takes precedence over includes.
        ///</summary>
        public List<string> ExcludeAttributes { get; set; }

        public DbObjectClassConfiguration<T> AddIncludeAttribute(DotNetAttribute attribute)
        {
            IncludeAttributes.Add(attribute);
            return this;
        }

        public DbObjectClassConfiguration<T> AddExcludeAttribute(string attributeName)
        {
            if (!ExcludeAttributes.Contains(attributeName.ToLower()))
            {
                ExcludeAttributes.Add(attributeName.ToLower());
            }
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
