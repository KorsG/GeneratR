using GeneratR.DotNet;
using System.Linq;

namespace GeneratR.Database.SqlServer.Templates
{
    public class TableTemplate : DotNetTemplate
    {
        private readonly TableCodeModel _model;
        private readonly DotNetGenerator _dotNet;

        public TableTemplate(TableCodeModel model)
            : base(model.DotNetGenerator)
        {
            _model = model;
            _dotNet = model.DotNetGenerator;
        }

        public virtual string Generate()
        {
            WriteNamespaceImports(_model.NamespaceImports);
            WriteLine();

            WriteNamespaceStart(_model.Namespace);
            using (IndentScope())
            {
                if (_model.Attributes.Any())
                {
                    Write(_model.Attributes.Build());
                }
                WriteClassStart(_model.DotNetModifier, _model.ClassName, _model.InheritClassName, _model.ImplementInterfaces);
                using (IndentScope())
                {
                    if (_model.AddConstructor)
                    {
                        WriteConstructor(DotNetModifierKeyword.Public, _model.ClassName);
                    }

                    foreach (var col in _model.Columns.OrderBy(x => x.DbObject.Position))
                    {
                        WriteLine();
                        WriteProperty(col);
                    }

                    foreach (var fk in _model.ForeignKeys.Where(x => !x.IsInverse).OrderBy(x => x.DbObject.ForeignKeyID))
                    {
                        WriteLine();
                        WriteProperty(fk);
                    }

                    foreach (var fk in _model.ForeignKeys.Where(x => x.IsInverse).OrderBy(x => x.DbObject.ForeignKeyID))
                    {
                        WriteLine();
                        WriteProperty(fk);
                    }

                    foreach (var p in _model.Properties)
                    {
                        WriteLine();
                        WriteProperty(p);
                    }
                }
                WriteClassEnd();
            }
            WriteNamespaceEnd();

            return base.ToString();
        }
    }
}
