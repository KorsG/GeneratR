namespace GeneratR.Database.SqlServer.Templates
{
    public class TableTypeTemplateModel
    {
        public TableTypeTemplateModel(SqlServerSchemaGenerator generator, SqlServerTableTypeConfiguration tableType)
        {
            Generator = generator;
            TableType = tableType;
        }

        public SqlServerSchemaGenerator Generator { get; }

        public SqlServerTableTypeConfiguration TableType { get; }
    }
}
