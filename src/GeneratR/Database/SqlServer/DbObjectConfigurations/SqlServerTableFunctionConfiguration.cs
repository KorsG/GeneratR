using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTableFunctionConfiguration : DbObjectClassConfiguration<Schema.TableFunction>
    {
        public SqlServerTableFunctionConfiguration(Schema.TableFunction dbObject, DotNet.DotNetGenerator dotNetGenerator, SqlServerTypeMapper typeMapper)
            : base(dbObject, dotNetGenerator)
        {
            Columns = new List<SqlServerColumnConfiguration>();
            Parameters = new List<SqlServerParameterConfiguration>();
            TypeMapper = typeMapper;
        }
        public SqlServerTypeMapper TypeMapper { get; }

        public List<SqlServerColumnConfiguration> Columns { get; set; }

        public List<SqlServerParameterConfiguration> Parameters { get; set; }

        public SqlServerColumnConfiguration GetColumn(string name)
        {
            return Columns?.FirstOrDefault(x => x.DbObject.Name == name);
        }

        public SqlServerTableFunctionConfiguration WithColumn(string name, Action<SqlServerColumnConfiguration> action)
        {
            if (action != null)
            {
                var column = GetColumn(name);
                if (column != null)
                {
                    action(column);
                }
            }
            return this;
        }
    }
}
