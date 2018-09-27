using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTableFunctionConfiguration : DbObjectClassConfiguration<Schema.TableFunction>
    {
        public SqlServerTableFunctionConfiguration(Schema.TableFunction dbObject)
            : base(dbObject)
        {
            Columns = new List<SqlServerColumnConfiguration>();
            Parameters = new List<SqlServerParameterConfiguration>();
        }

        public List<SqlServerColumnConfiguration> Columns { get; set; }

        public List<SqlServerParameterConfiguration> Parameters { get; set; }
    }
}
