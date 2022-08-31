using System.Collections.Generic;

namespace GeneratR.Database.SqlServer
{
    public class StoredProcedureCodeModel : DbObjectClassCodeModel<Schema.StoredProcedure>
    {
        public StoredProcedureCodeModel(Schema.StoredProcedure dbObject, SqlServerTypeMapper typeMapper)
            : base(dbObject)
        {
            ResultColumns = new List<StoredProcedureResultColumnCodeModel>();
            Parameters = new List<ParameterCodeModel>();
            TypeMapper = typeMapper;
        }

        public SqlServerTypeMapper TypeMapper { get; }

        public bool GenerateOutputParameters { get; set; }

        public bool GenerateResultSet { get; set; }

        public List<StoredProcedureResultColumnCodeModel> ResultColumns { get; set; }

        public List<ParameterCodeModel> Parameters { get; set; }
    }
}
