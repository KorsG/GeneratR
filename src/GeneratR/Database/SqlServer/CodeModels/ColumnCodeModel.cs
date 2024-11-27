namespace GeneratR.Database.SqlServer
{
    public class ColumnCodeModel : DbObjectPropertyCodeModel<Schema.Column>
    {
        public ColumnCodeModel(Schema.Column dbObject)
            : base(dbObject)
        {
            if (!string.IsNullOrWhiteSpace(dbObject.Description))
            {
                XmlDocumentation.AddSummaryLine(dbObject.Description.Trim());
            }
        }

        public new ColumnCodeModel Clone()
        {
            var baseClone = base.Clone();
            var clone = (ColumnCodeModel)MemberwiseClone();
            clone.DbObject = DbObject.Clone();
            clone.Attributes = baseClone.Attributes;
            clone.XmlDocumentation = baseClone.XmlDocumentation;
            return clone;
        }
    }
}
