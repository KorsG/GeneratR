using System;
using System.Collections.Generic;
using System.Linq;

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

        public SqlServerColumnConfiguration GetColumn(string name)
        {
            return Columns?.FirstOrDefault(x => x.DbObject.Name == name);
        }

        public SqlServerViewConfiguration WithColumn(string name, Action<SqlServerColumnConfiguration> action)
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
