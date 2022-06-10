using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerScalarFunctionCodeModel : DbObjectClassCodeModel<Schema.ScalarFunction>
    {
        public SqlServerScalarFunctionCodeModel(Schema.ScalarFunction dbObject, DotNet.DotNetGenerator dotNetGenerator)
            : base(dbObject, dotNetGenerator)
        {
            Parameters = new List<SqlServerParameterCodeModel>();
        }

        public List<SqlServerParameterCodeModel> Parameters { get; set; }

        // TODO: Add result columns
    }
}
