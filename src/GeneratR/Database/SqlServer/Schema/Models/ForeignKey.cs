using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer.Schema
{
    public class ForeignKey
    {
        public ForeignKey()
        {
            FromColumns = new List<ForeignKeyColumn>();
            ToColumns = new List<ForeignKeyColumn>();
        }

        public int ForeignKeyID { get; set; }
        public string ForeignKeyName { get; set; }

        public int FromObjectID { get; set; }
        public string FromSchema { get; set; }
        public string FromName { get; set; }
        public string FromFullName => FromSchema + "." + FromName;
        public List<ForeignKeyColumn> FromColumns { get; set; }

        public int ToObjectID { get; set; }
        public string ToSchema { get; set; }
        public string ToName { get; set; }
        public string ToFullName => ToSchema + "." + ToName;
        public List<ForeignKeyColumn> ToColumns { get; set; }

        public bool IsDisabled { get; set; }
        public bool IsNotForReplication { get; set; }
        public bool IsNotTrusted { get; set; }

        public bool IsOptional { get; set; }
        public ForeignKeyRelationshipType RelationshipType { get; set; }

        public bool IsSelfReferencing => FromFullName.Equals(ToFullName, StringComparison.OrdinalIgnoreCase) ? true : false;

        public override string ToString() => $"{ForeignKeyName}";
    }
}
