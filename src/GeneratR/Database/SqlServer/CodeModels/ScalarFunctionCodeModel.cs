using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer
{
    public class ScalarFunctionCodeModel : DbObjectClassCodeModel<Schema.ScalarFunction>
    {
        public ScalarFunctionCodeModel(Schema.ScalarFunction dbObject, DotNet.DotNetGenerator dotNetGenerator)
            : base(dbObject, dotNetGenerator)
        {
            Parameters = new List<ParameterCodeModel>();
        }

        public List<ParameterCodeModel> Parameters { get; set; }

        // TODO: Add result columns
    }
}
