using GeneratR.DotNet;

namespace GeneratR.Database
{
    public class DbObjectClassCodeModel<T> : ClassCodeModel where T : class
    {
        public DbObjectClassCodeModel(T dbObject, DotNetGenerator dotNetGenerator)
            : base(dotNetGenerator)
        {
            DbObject = dbObject;
        }

        /// <summary>
        /// The underlying database object.
        /// </summary>
        public T DbObject { get; }

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
