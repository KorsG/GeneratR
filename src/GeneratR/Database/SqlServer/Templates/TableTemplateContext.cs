using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer.Templates
{
    public class TableTemplateContext
    {
        public TableTemplateContext(DotNetGenerator dotNetGenerator, SqlServerTableSettings settings, SqlServerTableConfiguration table)
        {
            DotNetGenerator = dotNetGenerator;
            Settings = settings;
            Table = table;
        }

        public DotNetGenerator DotNetGenerator { get; }
        public SqlServerTableSettings Settings { get; }
        public SqlServerTableConfiguration Table { get; }
    }
}
