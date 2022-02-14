namespace GeneratR.Database.SqlServer.Templates
{
    public interface ITableFunctionTemplate
    {
        string Generate(SqlServerTableFunctionConfiguration obj); 
    }
}
