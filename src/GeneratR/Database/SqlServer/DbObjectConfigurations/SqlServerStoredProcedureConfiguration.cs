using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerStoredProcedureConfiguration : DbObjectClassConfiguration<Schema.StoredProcedure>
    {
        public SqlServerStoredProcedureConfiguration(Schema.StoredProcedure dbObject)
            : base(dbObject)
        {
            ResultColumns = new List<SqlServerStoredProcedureResultColumnConfiguration>();
            Parameters = new List<SqlServerParameterConfiguration>();
        }

        public List<SqlServerStoredProcedureResultColumnConfiguration> ResultColumns { get; set; }

        public List<SqlServerParameterConfiguration> Parameters { get; set; }
    }
}
