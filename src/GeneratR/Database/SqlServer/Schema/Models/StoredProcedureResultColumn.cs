namespace GeneratR.Database.SqlServer.Schema
{
    public class StoredProcedureResultColumn
    {
        public StoredProcedureResultColumn()
        {
        }

        public string Name { get; set; }
        public string ParentSchema { get; set; }
        public string ParentName { get; set; }
        public string DataType { get; set; }
        public short Length { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public int Position { get; set; }
        public bool IsNullable { get; set; }
        public bool IsComputed { get; set; }

        public override string ToString() => $"{Name}";
    }
}
