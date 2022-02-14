namespace GeneratR.Database.SqlServer.Templates
{
    public interface ITableTemplate
    {
        string Generate(SqlServerTableConfiguration obj); 
    }
}
