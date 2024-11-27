namespace GeneratR.Database.SqlServer
{
    public class StoredProcedureResultColumnCodeModel : DbObjectPropertyCodeModel<Schema.StoredProcedureResultColumn>
    {
        public StoredProcedureResultColumnCodeModel(Schema.StoredProcedureResultColumn dbObject)
            : base(dbObject)
        {
        }

        public new StoredProcedureResultColumnCodeModel Clone()
        {
            var baseClone = base.Clone();
            var clone = (StoredProcedureResultColumnCodeModel)MemberwiseClone();
            clone.DbObject = DbObject.Clone();
            clone.Attributes = baseClone.Attributes;
            clone.XmlDocumentation = baseClone.XmlDocumentation;
            return clone;
        }
    }
}
