using GeneratR.DotNet;

namespace GeneratR.Database
{
    public class DbObjectPropertyCodeModel<T> : PropertyCodeModel where T : class
    {
        public DbObjectPropertyCodeModel(T dbObject)
            : base()
        {
            DbObject = dbObject;
        }

        public T DbObject { get; protected internal set; }
    }
}
