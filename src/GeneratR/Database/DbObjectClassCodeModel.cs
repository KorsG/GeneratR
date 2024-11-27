using GeneratR.DotNet;

namespace GeneratR.Database
{
    public class DbObjectClassCodeModel<T> : ClassCodeModel where T : class
    {
        public DbObjectClassCodeModel(T dbObject)
        {
            DbObject = dbObject;
        }

        /// <summary>
        /// The underlying database object.
        /// </summary>
        public T DbObject { get; protected internal set; }

        public new DbObjectClassCodeModel<T> AddProperty(PropertyCodeModel property)
        {
            base.AddProperty(property);
            return this;
        }

        public new DbObjectClassCodeModel<T> AddAttribute(DotNetAttribute attribute)
        {
            base.AddAttribute(attribute);
            return this;
        }

        public new DbObjectClassCodeModel<T> RemoveAttribute(string attributeName)
        {
            base.RemoveAttribute(attributeName);
            return this;
        }
    }
}
