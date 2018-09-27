using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer.Schema
{
    public class Column
    {
        public Column()
        {
        }

        public string Name { get; set; }
        public string ParentName { get; set; }
        public string ParentSchema { get; set; }
        public string DataType { get; set; }
        public short Length { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public int Position { get; set; }
        public bool IsPrimaryKey { get; set; }
        public short PrimaryKeyPosition { get; set; }
        public long IdentitySeed { get; set; }
        public long IdentityIncrement { get; set; }
        public bool IsNullable { get; set; }
        public bool IsComputed { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsRowGuid { get; set; }
        public string DefaultValueDefinition { get; set; }
        public string Description { get; set; }

        public override string ToString() => $"{Name}";
    }
}
