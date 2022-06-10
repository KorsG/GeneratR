using GeneratR.Database.SqlServer.Schema;
using GeneratR.DotNet;
using GeneratR.Templating;
using System;
using System.Linq;

namespace GeneratR.Database.SqlServer.Templates
{
    public class StoredProcedureTemplate : StringTemplateBase
    {
        private readonly SqlServerStoredProcedureCodeModel _config;
        private readonly DotNetGenerator _dotNet;

        public StoredProcedureTemplate(SqlServerStoredProcedureCodeModel config)
        {
            _config = config;
            _dotNet = config.DotNetGenerator;
        }

        public virtual string Generate()
        {
            WriteLine("using System;");
            WriteLine("using System.Collections.Generic;");
            WriteLine("using System.ComponentModel;");
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
                // Generate result class that contains Return value, and if any, output parameters and column resultset.
                WriteLine(_dotNet.CreateClassStart(_config.ClassName, classAsPartial, classAsAbstract, _config.InheritClassName, _config.ImplementInterfaces));
                using (IndentScope())
                {
                    var generateOutputParameters = _config.GenerateOutputParameters && _config.DbObject.HasOutputParameters;
                    var outputParameterClassName = "OutputParametersModel";

                    var generateResultSet = _config.GenerateResultSet && _config.DbObject.HasResultColumns;
                    var resultSetClassName = "ResultModel";

                    if (_config.AddConstructor)
                    {
                        WriteLine(@"public {0}()", _config.ClassName);
                        WriteLine("{");
                        if (generateOutputParameters)
                        {
                            using (IndentScope())
                            {
                                WriteLine(@"OutputParameters = new {0}();", outputParameterClassName);
                            }
                        }
                        WriteLine("}");
                        WriteLine();
                    }

                    WriteLine("public int ReturnValue { get; set; }");
                    WriteLine();

                    if (generateOutputParameters)
                    {
                        WriteLine("public {0} OutputParameters {{ get; set; }}", outputParameterClassName);
                        WriteLine();
                    }

                    if (generateResultSet)
                    {
                        WriteLine("public IEnumerable<{0}> Result {{ get; set; }}", resultSetClassName);
                        WriteLine();
                    }

                    // Output parameters class.
                    if (generateOutputParameters)
                    {
                        WriteLine("[EditorBrowsable(EditorBrowsableState.Never)]");
                        WriteLine(_dotNet.CreateClassStart(outputParameterClassName, false, false, string.Empty, string.Empty));
                        using (IndentScope())
                        {
                            if (_config.AddConstructor)
                            {
                                WriteLine("public {0}(){1}{{{1}}}{1}", outputParameterClassName, Environment.NewLine);
                            }

                            foreach (var p in _config.Parameters.Where(x => x.DbObject.Direction == ParameterDirection.InAndOutDirection || x.DbObject.Direction == ParameterDirection.OutDirection))
                            {
                                if (p.Attributes.Any())
                                {
                                    Write(p.Attributes.ToMultilineString());
                                }
                                WriteLine($"public {p.PropertyType} {p.PropertyName} {{ get; set; }}");
                            }
                        }
                        WriteLine(_dotNet.CreateClassEnd());
                        WriteLine();
                    }

                    // Result class.
                    if (generateResultSet)
                    {
                        WriteLine("[EditorBrowsable(EditorBrowsableState.Never)]");
                        WriteLine(_dotNet.CreateClassStart(resultSetClassName, false, false, string.Empty, string.Empty));
                        using (IndentScope())
                        {
                            if (_config.AddConstructor)
                            {
                                WriteLine("public {0}(){1}{{{1}}}{1}", resultSetClassName, Environment.NewLine);
                            }

                            foreach (var col in _config.ResultColumns)
                            {
                                if (col.Attributes.Any())
                                {
                                    Write(col.Attributes.ToMultilineString());
                                }
                                WriteLine($"public {col.PropertyType} {col.PropertyName} {{ get; set; }}");
                            }
                        }
                        WriteLine(_dotNet.CreateClassEnd());
                        WriteLine();
                    }
                }
                WriteLine(_dotNet.CreateClassEnd());
            }
            WriteLine(_dotNet.CreateNamespaceEnd());

            return TemplateBuilder.ToString();
        }
    }
}
