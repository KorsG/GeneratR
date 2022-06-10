namespace GeneratR.Database.SqlServer
{
    public class SqlServerForeignKeyCodeModel : DbObjectPropertyCodeModel<Schema.ForeignKey>
    {
        public SqlServerForeignKeyCodeModel(Schema.ForeignKey dbObject)
            : base(dbObject)
        {
        }
    }
}
