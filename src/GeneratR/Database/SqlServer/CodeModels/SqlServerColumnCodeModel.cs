namespace GeneratR.Database.SqlServer
{
    public class SqlServerColumnCodeModel : DbObjectPropertyCodeModel<Schema.Column>
    {
        public SqlServerColumnCodeModel(Schema.Column dbObject)
            : base(dbObject)
        {
        }
    }
}
