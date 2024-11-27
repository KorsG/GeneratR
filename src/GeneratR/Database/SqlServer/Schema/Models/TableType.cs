using System.Collections.Generic;
using System.Linq;

namespace GeneratR.Database.SqlServer.Schema
{
    public class TableType
    {
        public TableType()
        {
            Columns = new List<Column>();
        }

        public int ObjectID { get; set; }

        public string Schema { get; set; }
        public string Name { get; set; }
        public string FullName => $"{Schema}.{Name}";
        public bool IsNullable { get; set; }

        public List<Column> Columns { get; set; }

        public override string ToString() => $"{FullName}";

        public TableType Clone()
        {
            var clone = (TableType)MemberwiseClone();
            clone.Columns = new List<Column>(Columns.Select(x => x.Clone()));
            return clone;
        }
    }
}
