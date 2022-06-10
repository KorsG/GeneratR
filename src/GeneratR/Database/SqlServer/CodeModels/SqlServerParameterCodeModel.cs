using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerParameterCodeModel : DbObjectPropertyCodeModel<Schema.Parameter>
    {
        public SqlServerParameterCodeModel(Schema.Parameter dbObject)
            : base(dbObject)
        {
        }
    }
}
