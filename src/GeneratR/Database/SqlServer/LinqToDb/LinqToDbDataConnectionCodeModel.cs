using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class LinqToDbDataConnectionCodeModel : ClassCodeModel
    {
        public LinqToDbDataConnectionCodeModel(DotNetGenerator dotNetGenerator, SqlServerSchemaCodeModels schemaModels)
            : base(dotNetGenerator)
        {
            SchemaModels = schemaModels;
        }

        public SqlServerSchemaCodeModels SchemaModels { get; }
    }
}
