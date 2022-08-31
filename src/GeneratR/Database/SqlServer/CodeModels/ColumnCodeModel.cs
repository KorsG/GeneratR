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
    }
}
