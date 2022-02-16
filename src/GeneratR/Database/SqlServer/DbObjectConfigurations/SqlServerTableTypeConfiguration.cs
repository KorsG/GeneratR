using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTableTypeConfiguration : DbObjectClassConfiguration<Schema.TableType>
    {
        public SqlServerTableTypeConfiguration(Schema.TableType dbObject, DotNet.DotNetGenerator dotNetGenerator, SqlServerTypeMapper typeMapper)
            : base(dbObject, dotNetGenerator)
        {
            Columns = new List<SqlServerColumnConfiguration>();
            TypeMapper = typeMapper;
        }

        public SqlServerTypeMapper TypeMapper { get; }

        public List<SqlServerColumnConfiguration> Columns { get; set; }

        public SqlServerColumnConfiguration GetColumn(string name)
        {
            return Columns?.FirstOrDefault(x => x.DbObject.Name == name);
        }

        public SqlServerTableTypeConfiguration WithColumn(string name, Action<SqlServerColumnConfiguration> action)
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
