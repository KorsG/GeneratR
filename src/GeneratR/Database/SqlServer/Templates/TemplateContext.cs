using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer.Templates
{
    public class TemplateContext<TSettings, TObject>
    {
        public TemplateContext(DotNetGenerator dotNetGenerator, TSettings settings, TObject @object)
        {
            DotNetGenerator = dotNetGenerator;
            Settings = settings;
            Object = @object;
        }

        public DotNetGenerator DotNetGenerator { get; }
        public TSettings Settings { get; }
        public TObject Object { get; }
    }
}
