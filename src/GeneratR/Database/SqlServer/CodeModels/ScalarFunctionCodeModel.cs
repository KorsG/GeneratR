using System.Collections.Generic;

namespace GeneratR.Database.SqlServer
{
    public class ScalarFunctionCodeModel : DbObjectClassCodeModel<Schema.ScalarFunction>
    {
        public ScalarFunctionCodeModel(Schema.ScalarFunction dbObject)
            : base(dbObject)
        {
            Parameters = new List<ParameterCodeModel>();
        }

        public List<ParameterCodeModel> Parameters { get; set; }

        // TODO: Add result columns
    }
}
