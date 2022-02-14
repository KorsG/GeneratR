namespace GeneratR.Database.SqlServer.Templates
{
    public interface IViewTemplate
    {
        string Generate(SqlServerViewConfiguration obj); 
    }
}
