using GeneratR.DotNet;
using GeneratR.Templating;
using System.Linq;

namespace GeneratR.Database.SqlServer.Templates
{
    public class ViewTemplate : StringTemplateBase
    {
        private readonly ViewCodeModel _model;
        private readonly DotNetGenerator _dotNet;

        public ViewTemplate(ViewCodeModel model)
        {
            _model = model;
            _dotNet = _model.DotNetGenerator;
        }

        public string Generate()
        {
            WriteLine("using System;");
            WriteLine("using System.Collections.Generic;");
            if (_model.AddDataAnnotationAttributes)
            {
                WriteLine("using System.ComponentModel.DataAnnotations;");
                WriteLine("using System.ComponentModel.DataAnnotations.Schema;");
            }

            WriteLine();
            WriteLine(_dotNet.CreateNamespaceStart(_model.Namespace));
            using (IndentScope())
            {
                var classAsAbstract = _model.DotNetModifier.HasFlag(DotNetModifierKeyword.Abstract);
                var classAsPartial = _model.DotNetModifier.HasFlag(DotNetModifierKeyword.Partial);
                if (_model.Attributes.Any())
                {
                    Write(_model.Attributes.ToMultilineString());
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
                        if (!string.IsNullOrWhiteSpace(col.DbObject.Description))
                        {
                            WriteLine($@"/// <summary>{col.DbObject.Description}</summary>");
                        }

                        if (col.Attributes.Any())
                        {
                            Write(col.Attributes.ToMultilineString());
                        }
                        WriteLine(_dotNet.CreateProperty(col.Modifier, col.PropertyName, col.PropertyType, false));
                    }

                }
                WriteLine(_dotNet.CreateClassEnd());
            }
            WriteLine(_dotNet.CreateNamespaceEnd());

            return TemplateBuilder.ToString();
        }
    }
}
