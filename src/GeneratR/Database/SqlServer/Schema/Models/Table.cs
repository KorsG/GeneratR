using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
