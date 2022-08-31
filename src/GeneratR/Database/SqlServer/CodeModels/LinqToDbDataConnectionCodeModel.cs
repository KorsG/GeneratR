using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class LinqToDbDataConnectionCodeModel : ClassCodeModel
    {
        public LinqToDbDataConnectionCodeModel(SqlServerSchemaCodeModels schemaModels)
        {
            SchemaModels = schemaModels;
        }

        public SqlServerSchemaCodeModels SchemaModels { get; }
    }
}
