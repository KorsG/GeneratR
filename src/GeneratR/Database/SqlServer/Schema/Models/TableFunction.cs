using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer.Schema
{
    public class TableFunction
    {
        public TableFunction()
        {
            Columns = new List<Column>();
            Parameters = new List<Parameter>();
        }

        public string Name { get; set; }

        public string Schema { get; set; }

        public string FullName => Schema + "." + Name;

        public List<Column> Columns { get; set; }

        public List<Parameter> Parameters { get; set; }

        public override string ToString() => $"{FullName}";
    }
}
