using GeneratR.Database.SqlServer.Schema;
using GeneratR.DotNet;
using System;
using System.Linq;

namespace GeneratR.Database.SqlServer.Templates
{
    public class StoredProcedureTemplate : DotNetTemplate
    {
        private readonly StoredProcedureCodeModel _model;
        private readonly DotNetGenerator _dotNet;

        public StoredProcedureTemplate(StoredProcedureCodeModel model)
            : base(model.DotNetGenerator)
        {
            _model = model;
            _dotNet = model.DotNetGenerator;
        }

        public virtual string Generate()
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
                // Generate result class that contains Return value, and if any, output parameters and column resultset.
                WriteLine(_dotNet.CreateClassStart(_model.ClassName, classAsPartial, classAsAbstract, _model.InheritClassName, _model.ImplementInterfaces));
                using (IndentScope())
                {
                    var generateOutputParameters = _model.GenerateOutputParameters && _model.DbObject.HasOutputParameters;
                    var outputParameterClassName = "OutputParametersModel";

                    var generateResultSet = _model.GenerateResultSet && _model.DbObject.HasResultColumns;
                    var resultSetClassName = "ResultModel";

                    if (_model.AddConstructor)
                    {
                        WriteLine(@"public {0}()", _model.ClassName);
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
                            if (_model.AddConstructor)
                            {
                                WriteLine("public {0}(){1}{{{1}}}{1}", outputParameterClassName, Environment.NewLine);
                            }

                            foreach (var p in _model.Parameters.Where(x => x.DbObject.Direction == ParameterDirection.InAndOutDirection || x.DbObject.Direction == ParameterDirection.OutDirection))
                            {
                                if (p.Attributes.Any())
                                {
                                    Write(p.Attributes.Build());
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
                            if (_model.AddConstructor)
                            {
                                WriteLine("public {0}(){1}{{{1}}}{1}", resultSetClassName, Environment.NewLine);
                            }

                            foreach (var col in _model.ResultColumns)
                            {
                                WriteProperty(col);
                            }
                        }
                        WriteLine(_dotNet.CreateClassEnd());
                        WriteLine();
                    }
                }
                WriteLine(_dotNet.CreateClassEnd());
            }
            WriteLine(_dotNet.CreateNamespaceEnd());

            return base.ToString();
        }
    }
}
