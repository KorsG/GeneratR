using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerStoredProcedureResultColumnConfiguration : DbObjectPropertyConfiguration<Schema.StoredProcedureResultColumn>
    {
        public SqlServerStoredProcedureResultColumnConfiguration(Schema.StoredProcedureResultColumn dbObject)
            : base(dbObject)
        {
        }
    }
}
