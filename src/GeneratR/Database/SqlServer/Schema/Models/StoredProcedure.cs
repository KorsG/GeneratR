using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer.Schema
{
    public class StoredProcedure
    {
        public StoredProcedure()
        {
            Parameters = new List<Parameter>();
            ResultColumns = new List<StoredProcedureResultColumn>();
        }

        public int ObjectID { get; set; }

        public string Schema { get; set; }
        public string Name { get; set; }
        public string FullName => $"{Schema}.{Name}";

        public List<Parameter> Parameters { get; set; }

        public List<StoredProcedureResultColumn> ResultColumns { get; set; }

        public bool HasResultColumns => ResultColumns.Any();

        public bool HasOutputParameters => Parameters.Any(q => q.Direction == ParameterDirection.InAndOutDirection || q.Direction == ParameterDirection.OutDirection);

        public override string ToString() => $"{FullName}";
    }
}
