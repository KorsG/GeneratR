namespace GeneratR.Database.SqlServer.Templates
{
    public interface ITableTypeTemplate
    {
        string Generate(SqlServerTableTypeConfiguration obj); 
    }
}
