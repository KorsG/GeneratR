using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTableFunctionCodeModel : DbObjectClassCodeModel<Schema.TableFunction>
    {
        public SqlServerTableFunctionCodeModel(Schema.TableFunction dbObject, DotNet.DotNetGenerator dotNetGenerator, SqlServerTypeMapper typeMapper)
            : base(dbObject, dotNetGenerator)
        {
            Columns = new List<SqlServerColumnCodeModel>();
            Parameters = new List<SqlServerParameterCodeModel>();
            TypeMapper = typeMapper;
        }
        public SqlServerTypeMapper TypeMapper { get; }

        public List<SqlServerColumnCodeModel> Columns { get; set; }

        public List<SqlServerParameterCodeModel> Parameters { get; set; }

        public SqlServerColumnCodeModel GetColumn(string name)
        {
            return Columns?.FirstOrDefault(x => x.DbObject.Name == name);
        }

        public SqlServerTableFunctionCodeModel WithColumn(string name, Action<SqlServerColumnCodeModel> action)
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
