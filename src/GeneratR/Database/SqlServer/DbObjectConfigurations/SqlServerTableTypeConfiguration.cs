using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTableTypeConfiguration : DbObjectClassConfiguration<Schema.TableType>
    {
        public SqlServerTableTypeConfiguration(Schema.TableType dbObject)
            : base(dbObject)
        {
            Columns = new List<SqlServerColumnConfiguration>();
        }

        public List<SqlServerColumnConfiguration> Columns { get; set; }
    }
}
