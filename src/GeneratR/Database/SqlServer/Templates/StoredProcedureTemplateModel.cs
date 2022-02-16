namespace GeneratR.Database.SqlServer.Templates
{
    public class StoredProcedureTemplateModel
    {
        public StoredProcedureTemplateModel(SqlServerSchemaGenerator generator, SqlServerStoredProcedureConfiguration storedProcedure)
        {
            Generator = generator;
            StoredProcedure = storedProcedure;
        }

        public SqlServerSchemaGenerator Generator { get; }

        public SqlServerStoredProcedureConfiguration StoredProcedure { get; }
    }
}
