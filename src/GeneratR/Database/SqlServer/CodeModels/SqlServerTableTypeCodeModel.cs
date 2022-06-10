using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTableTypeCodeModel : DbObjectClassCodeModel<Schema.TableType>
    {
        public SqlServerTableTypeCodeModel(Schema.TableType dbObject, DotNet.DotNetGenerator dotNetGenerator, SqlServerTypeMapper typeMapper)
            : base(dbObject, dotNetGenerator)
        {
            Columns = new List<SqlServerColumnCodeModel>();
            TypeMapper = typeMapper;
        }

        public SqlServerTypeMapper TypeMapper { get; }

        public List<SqlServerColumnCodeModel> Columns { get; set; }

        public bool AddSqlDataRecordMappings { get; set; }

        public SqlServerColumnCodeModel GetColumn(string name)
        {
            return Columns?.FirstOrDefault(x => x.DbObject.Name == name);
        }

        public SqlServerTableTypeCodeModel WithColumn(string name, Action<SqlServerColumnCodeModel> action)
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
