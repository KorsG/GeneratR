using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerColumnConfiguration : DbObjectPropertyConfiguration<Schema.Column>
    {
        public SqlServerColumnConfiguration(Schema.Column dbObject)
            : base(dbObject)
        {
        }
    }
}
