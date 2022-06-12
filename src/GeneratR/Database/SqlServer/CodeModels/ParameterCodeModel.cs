using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer
{
    public class ParameterCodeModel : DbObjectPropertyCodeModel<Schema.Parameter>
    {
        public ParameterCodeModel(Schema.Parameter dbObject)
            : base(dbObject)
        {
        }
    }
}
