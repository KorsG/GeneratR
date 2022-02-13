using System;
using System.Collections.Generic;
using System.Linq;

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

        public SqlServerColumnConfiguration GetColumn(string name)
        {
            return Columns?.FirstOrDefault(x => x.DbObject.Name == name);
        }

        public SqlServerTableConfiguration WithColumn(string name, Action<SqlServerColumnConfiguration> action)
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
