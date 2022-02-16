using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerScalarFunctionConfiguration : DbObjectClassConfiguration<Schema.ScalarFunction>
    {
        public SqlServerScalarFunctionConfiguration(Schema.ScalarFunction dbObject)
            : base(dbObject)
        {
            Parameters = new List<SqlServerParameterConfiguration>();
        }

        public List<SqlServerParameterConfiguration> Parameters { get; set; }

        // TODO: Add result columns
    }
}
