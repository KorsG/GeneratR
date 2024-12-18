﻿namespace GeneratR.Database.SqlServer.Schema
{
    public class Parameter
    {
        public Parameter()
        {
        }

        public string Name { get; set; }

        public int ParentObjectID { get; set; }
        public string ParentName { get; set; }
        public string ParentSchema { get; set; }
        public string ParentFullName => $"{ParentSchema}.{ParentName}";
        public string ParentType { get; set; }

        public string DataTypeSchema { get; set; }

        public string DataType { get; set; }

        public short Length { get; set; }

        public byte Precision { get; set; }

        public byte Scale { get; set; }

        public int Position { get; set; }

        public bool IsNullable { get; set; }

        public bool IsReadonly { get; set; }

        public bool IsTableType { get; set; }

        public ParameterDirection Direction { get; set; }

        public override string ToString() => $"{Name}";

        public Parameter Clone()
        {
            var clone = (Parameter)MemberwiseClone();
            return clone;
        }
    }
}
