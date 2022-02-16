namespace GeneratR.Database.SqlServer
{
    public class SqlServerColumnConfiguration : DbObjectPropertyConfiguration<Schema.Column>
    {
        public SqlServerColumnConfiguration(Schema.Column dbObject)
            : base(dbObject)
        {
        }
    }
}
