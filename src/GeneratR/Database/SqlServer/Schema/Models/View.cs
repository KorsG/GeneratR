using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer.Schema
{
    public class View
    {
        public View()
        {
            Columns = new List<Column>();
        }

        public int ObjectID { get; set; }

        public string Name { get; set; }
        public string Schema { get; set; }
        public string FullName => $"{Schema}.{Name}";

        public List<Column> Columns { get; set; }

        public override string ToString() => $"{FullName}";

        public View Clone()
        {
            var clone = (View)MemberwiseClone();
            clone.Columns = new List<Column>(Columns.Select(x => x.Clone()));
            return clone;
        }
    }
}
