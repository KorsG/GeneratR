using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerViewConfiguration : DbObjectClassConfiguration<Schema.View>
    {
        public SqlServerViewConfiguration(Schema.View dbObject)
            : base(dbObject)
        {
            Columns = new List<SqlServerColumnConfiguration>();
        }

        public List<SqlServerColumnConfiguration> Columns { get; set; }
    }
}
