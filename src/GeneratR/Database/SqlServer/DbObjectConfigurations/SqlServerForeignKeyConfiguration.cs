namespace GeneratR.Database.SqlServer
{
    public class SqlServerForeignKeyConfiguration : DbObjectPropertyConfiguration<Schema.ForeignKey>
    {
        public SqlServerForeignKeyConfiguration(Schema.ForeignKey dbObject)
            : base(dbObject)
        {
        }
    }
}
