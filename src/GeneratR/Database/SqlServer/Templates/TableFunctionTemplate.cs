using GeneratR.DotNet;
using System.Linq;

namespace GeneratR.Database.SqlServer.Templates
{
    public class TableFunctionTemplate : DotNetTemplate
    {
        private readonly TableFunctionCodeModel _model;
        private readonly DotNetGenerator _dotNet;

        public TableFunctionTemplate(TableFunctionCodeModel model, DotNetGenerator dotNetGenerator) 
            : base(dotNetGenerator)
        {
            _model = model;
            _dotNet = dotNetGenerator;
        }

        public string Generate()
        {
            WriteNamespaceImports(_model.NamespaceImports);
            WriteLine();
            WriteLine(_dotNet.CreateNamespaceStart(_model.Namespace));
            using (IndentScope())
            {
                var classAsAbstract = _model.DotNetModifier.HasFlag(DotNetModifierKeyword.Abstract);
                var classAsPartial = _model.DotNetModifier.HasFlag(DotNetModifierKeyword.Partial);
                if (_model.Attributes.Any())
                {
                    Write(_model.Attributes.Build());
                }
                WriteLine(_dotNet.CreateClassStart(_model.ClassName, classAsPartial, classAsAbstract, _model.InheritClassName, _model.ImplementInterfaces));
                using (IndentScope())
                {
                    if (_model.AddConstructor)
                    {
                        WriteLine(_dotNet.CreateConstructor(DotNetModifierKeyword.Public, _model.ClassName));
                    }

                    foreach (var col in _model.Columns.OrderBy(x => x.DbObject.Position))
                    {
                        WriteLine();
                        WriteProperty(col);
                    }

                    foreach (var p in _model.Properties)
                    {
                        WriteLine();
                        WriteProperty(p);
                    }

                }
                WriteLine(_dotNet.CreateClassEnd());
            }
            WriteLine(_dotNet.CreateNamespaceEnd());

            return base.ToString();
        }
    }
}
