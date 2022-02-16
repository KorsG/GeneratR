namespace GeneratR.Database.SqlServer.Templates
{
    public class TableTemplateModel
    {
        public TableTemplateModel(SqlServerSchemaGenerator generator, SqlServerTableConfiguration table)
        {
            Generator = generator;
            Table = table;
        }

        public SqlServerSchemaGenerator Generator { get; }

        public SqlServerTableConfiguration Table { get; }
    }
}
