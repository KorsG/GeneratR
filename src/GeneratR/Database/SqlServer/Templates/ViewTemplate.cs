using GeneratR.DotNet;
using GeneratR.Templating;
using System.Linq;

namespace GeneratR.Database.SqlServer.Templates
{
    public class ViewTemplate : StringTemplateBase
    {
        private readonly SqlServerViewConfiguration _config;
        private readonly DotNetGenerator _dotNet;

        public ViewTemplate(SqlServerViewConfiguration config)
        {
            _config = config;
            _dotNet = _config.DotNetGenerator;
        }

        public string Generate()
        {
            WriteLine("using System;");
            WriteLine("using System.Collections.Generic;");
            if (_config.AddDataAnnotationAttributes)
            {
                WriteLine("using System.ComponentModel.DataAnnotations;");
                WriteLine("using System.ComponentModel.DataAnnotations.Schema;");
            }

            WriteLine();
            WriteLine(_dotNet.CreateNamespaceStart(_config.Namespace));
            using (IndentScope())
            {
                var classAsAbstract = _config.DotNetModifier.HasFlag(DotNetModifierKeyword.Abstract);
                var classAsPartial = _config.DotNetModifier.HasFlag(DotNetModifierKeyword.Partial);
                if (_config.Attributes.Any())
                {
                    Write(_config.Attributes.ToMultilineString());
                }
                WriteLine(_dotNet.CreateClassStart(_config.ClassName, classAsPartial, classAsAbstract, _config.InheritClassName, _config.ImplementInterfaces));
                using (IndentScope())
                {
                    if (_config.AddConstructor)
                    {
                        WriteLine(_dotNet.CreateConstructor(DotNetModifierKeyword.Public, _config.ClassName));
                    }

                    foreach (var col in _config.Columns.OrderBy(x => x.DbObject.Position))
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
                        WriteLine(_dotNet.CreateProperty(col.DotNetModifier, col.PropertyName, col.PropertyType, false));
                    }

                }
                WriteLine(_dotNet.CreateClassEnd());
            }
            WriteLine(_dotNet.CreateNamespaceEnd());

            return TemplateBuilder.ToString();
        }
    }
}
