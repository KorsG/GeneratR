namespace GeneratR.Database.SqlServer
{
    public class ForeignKeyCodeModel : DbObjectPropertyCodeModel<Schema.ForeignKey>
    {
        public ForeignKeyCodeModel(Schema.ForeignKey dbObject)
            : base(dbObject)
        {
        }

        public bool IsInverse { get; set; }
    }
}
