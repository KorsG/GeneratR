using GeneratR.DotNet;

namespace GeneratR.Database
{
    public class DbObjectPropertyConfiguration<T> where T : class
    {
        public DbObjectPropertyConfiguration(T dbObject)
        {
            DbObject = dbObject;
            Attributes = new DotNetAttributeCollection();
        }

        public T DbObject { get; }

        public string PropertyName { get; set; }

        public string PropertyType { get; set; }

        public DotNetModifierKeyword DotNetModifier { get; set; }

        ///<summary>
        /// List of attributes added to the property.
        ///</summary>
        public DotNetAttributeCollection Attributes { get; set; }

        public DbObjectPropertyConfiguration<T> AddAttribute(DotNetAttribute attribute)
        {
            if (Attributes == null) { Attributes = new DotNetAttributeCollection(); }
            Attributes.Add(attribute);
            return this;
        }
    }
}
