namespace GeneratR.Database.SqlServer
{
    public class ColumnCodeModel : DbObjectPropertyCodeModel<Schema.Column>
    {
        public ColumnCodeModel(Schema.Column dbObject)
            : base(dbObject)
        {
        }
    }
}
