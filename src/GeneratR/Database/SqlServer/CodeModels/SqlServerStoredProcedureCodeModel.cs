using System.Collections.Generic;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerStoredProcedureCodeModel : DbObjectClassCodeModel<Schema.StoredProcedure>
    {
        public SqlServerStoredProcedureCodeModel(Schema.StoredProcedure dbObject, DotNet.DotNetGenerator dotNetGenerator, SqlServerTypeMapper typeMapper)
            : base(dbObject, dotNetGenerator)
        {
            ResultColumns = new List<SqlServerStoredProcedureResultColumnCodeModel>();
            Parameters = new List<SqlServerParameterCodeModel>();
            TypeMapper = typeMapper;
        }

        public SqlServerTypeMapper TypeMapper { get; }

        public bool GenerateOutputParameters { get; set; }

        public bool GenerateResultSet { get; set; }

        public List<SqlServerStoredProcedureResultColumnCodeModel> ResultColumns { get; set; }

        public List<SqlServerParameterCodeModel> Parameters { get; set; }
    }
}
