using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratR.Database.SqlServer
{
    public class TableFunctionCodeModel : DbObjectClassCodeModel<Schema.TableFunction>
    {
        public TableFunctionCodeModel(Schema.TableFunction dbObject, SqlServerTypeMapper typeMapper)
            : base(dbObject)
        {
            Columns = new List<ColumnCodeModel>();
            Parameters = new List<ParameterCodeModel>();
            TypeMapper = typeMapper;
        }
        public SqlServerTypeMapper TypeMapper { get; }

        public List<ColumnCodeModel> Columns { get; set; }

        public List<ParameterCodeModel> Parameters { get; set; }

        public ColumnCodeModel GetColumn(string name)
        {
            return Columns?.FirstOrDefault(x => x.DbObject.Name == name);
        }

        public TableFunctionCodeModel WithColumn(string name, Action<ColumnCodeModel> action)
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
