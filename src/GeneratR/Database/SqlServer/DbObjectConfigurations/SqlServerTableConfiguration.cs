using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTableConfiguration : DbObjectClassConfiguration<Schema.Table>
    {
        public SqlServerTableConfiguration(Schema.Table dbObject)
            : base(dbObject)
        {
            Columns = new List<SqlServerColumnConfiguration>();
            ForeignKeys = new List<SqlServerForeignKeyConfiguration>();
            ReferencingForeignKeys = new List<SqlServerForeignKeyConfiguration>();
        }

        public List<SqlServerColumnConfiguration> Columns { get; set; }

        public List<SqlServerForeignKeyConfiguration> ForeignKeys { get; set; }

        public List<SqlServerForeignKeyConfiguration> ReferencingForeignKeys { get; set; }
    }
}
