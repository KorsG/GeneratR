namespace GeneratR.Database.SqlServer.Templates
{
    public interface IStoredProcedureTemplate
    {
        string Generate(SqlServerStoredProcedureConfiguration obj); 
    }
}
