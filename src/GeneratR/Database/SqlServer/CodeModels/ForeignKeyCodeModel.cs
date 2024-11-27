namespace GeneratR.Database.SqlServer
{
    public class ForeignKeyCodeModel : DbObjectPropertyCodeModel<Schema.ForeignKey>
    {
        public ForeignKeyCodeModel(Schema.ForeignKey dbObject)
            : base(dbObject)
        {
        }

        public bool IsInverse { get; set; }

        public new ForeignKeyCodeModel Clone()
        {
            var baseClone = base.Clone();
            var clone = (ForeignKeyCodeModel)MemberwiseClone();
            clone.DbObject = DbObject.Clone();
            clone.Attributes = baseClone.Attributes;
            clone.XmlDocumentation = baseClone.XmlDocumentation;
            return clone;
        }
    }
}
