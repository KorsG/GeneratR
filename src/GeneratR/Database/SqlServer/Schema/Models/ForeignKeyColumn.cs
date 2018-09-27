using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer.Schema
{
    public class ForeignKeyColumn
    {
        public int ForeignKeyID { get; set; }
        public int ColumnID { get; set; }
        public string ColumnName { get; set; }
        public int OrdinalPosition { get; set; }
        public bool IsNullable { get; set; }

        public override string ToString()
        {
            return $"{ColumnName}";
        }
    }
}
