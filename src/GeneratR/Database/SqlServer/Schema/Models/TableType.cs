using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer.Schema
{
    public class TableType
    {
        public TableType()
        {
            Columns = new List<Column>();
        }

        public string Schema { get; set; }
        public string Name { get; set; }
        public string FullName => Schema + "." + Name;
        public List<Column> Columns { get; set; }
        public bool IsNullable { get; set; }

        public override string ToString() => $"{FullName}";
    }
}
