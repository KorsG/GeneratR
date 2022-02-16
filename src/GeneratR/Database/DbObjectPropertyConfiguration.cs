using GeneratR.DotNet;
using System.Collections.Generic;

namespace GeneratR.Database
{
    public class DbObjectPropertyConfiguration<T> where T : class
    {
        public DbObjectPropertyConfiguration(T dbObject)
        {
            DbObject = dbObject;
            IncludeAttributes = new DotNetAttributeCollection();
            ExcludeAttributes = new List<string>();
        }

        public T DbObject { get; }

        public string PropertyName { get; set; }

        public string PropertyType { get; set; }

        public DotNetModifierKeyword DotNetModifier { get; set; }

        ///<summary>
        /// List of attributes which will be included. Replaces any auto generated attributes. Exclude takes precedence over includes.
        ///</summary>
        public DotNetAttributeCollection IncludeAttributes { get; set; }

        ///<summary>
        /// List of attribute names which will be exluded. Exclude takes precedence over includes.
        ///</summary>
        public List<string> ExcludeAttributes { get; set; }

        public DbObjectPropertyConfiguration<T> AddIncludeAttribute(DotNetAttribute attribute)
        {
            IncludeAttributes.Add(attribute);
            return this;
        }

        public DbObjectPropertyConfiguration<T> AddExcludeAttribute(string attributeName)
        {
            if (!ExcludeAttributes.Contains(attributeName.ToLower()))
            {
                ExcludeAttributes.Add(attributeName.ToLower());
            }
            return this;
        }
    }
}
