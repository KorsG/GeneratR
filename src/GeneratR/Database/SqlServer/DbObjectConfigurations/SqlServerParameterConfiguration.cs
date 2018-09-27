using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerParameterConfiguration : DbObjectPropertyConfiguration<Schema.Parameter>
    {
        public SqlServerParameterConfiguration(Schema.Parameter dbObject)
            : base(dbObject)
        {
        }
    }
}
