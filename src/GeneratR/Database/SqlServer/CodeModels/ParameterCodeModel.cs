namespace GeneratR.Database.SqlServer
{
    public class ParameterCodeModel : DbObjectPropertyCodeModel<Schema.Parameter>
    {
        public ParameterCodeModel(Schema.Parameter dbObject)
            : base(dbObject)
        {
        }

        public new ParameterCodeModel Clone()
        {
            var baseClone = base.Clone();
            var clone = (ParameterCodeModel)MemberwiseClone();
            clone.DbObject = DbObject.Clone();
            clone.Attributes = baseClone.Attributes;
            clone.XmlDocumentation = baseClone.XmlDocumentation;
            return clone;
        }
    }
}
