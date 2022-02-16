using System.Collections.Generic;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerStoredProcedureConfiguration : DbObjectClassConfiguration<Schema.StoredProcedure>
    {
        public SqlServerStoredProcedureConfiguration(Schema.StoredProcedure dbObject, DotNet.DotNetGenerator dotNetGenerator, SqlServerTypeMapper typeMapper)
            : base(dbObject, dotNetGenerator)
        {
            ResultColumns = new List<SqlServerStoredProcedureResultColumnConfiguration>();
            Parameters = new List<SqlServerParameterConfiguration>();
            TypeMapper = typeMapper;
        }

        public SqlServerTypeMapper TypeMapper { get; }

        public bool GenerateOutputParameters { get; set; }

        public bool GenerateResultSet { get; set; }

        public List<SqlServerStoredProcedureResultColumnConfiguration> ResultColumns { get; set; }

        public List<SqlServerParameterConfiguration> Parameters { get; set; }
    }
}
