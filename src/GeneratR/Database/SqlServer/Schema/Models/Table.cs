using System.Collections.Generic;
using System.Linq;

namespace GeneratR.Database.SqlServer.Schema
{
    public class Table
    {
        public Table()
        {
            Columns = new List<Column>();
            ForeignKeys = new List<ForeignKey>();
            ReferencingForeignKeys = new List<ForeignKey>();
            Indexes = new List<Index>();
        }

        public int ObjectID { get; set; }

        public string Schema { get; set; }
        public string Name { get; set; }
        public string FullName => $"{Schema}.{Name}";

        public List<Column> Columns { get; set; }
        public List<ForeignKey> ForeignKeys { get; set; }
        public List<ForeignKey> ReferencingForeignKeys { get; set; }
        public List<Index> Indexes { get; set; }

        public override string ToString() => $"{FullName}";

        public Table Clone()
        {
            var clone = (Table)MemberwiseClone();

            clone.Columns = new List<Column>(Columns.Select(x => x.Clone()));
            clone.ForeignKeys = new List<ForeignKey>(ForeignKeys.Select(x => x.Clone()));
            clone.ReferencingForeignKeys = new List<ForeignKey>(ReferencingForeignKeys.Select(x => x.Clone()));
            clone.Indexes = new List<Index>(Indexes.Select(x => x.Clone()));

            return clone;
        }
    }
}
