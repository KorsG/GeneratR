using GeneratR.Database.SqlServer.Schema;
using GeneratR.DotNet;
using GeneratR.Templating;
using System;
using System.Linq;

namespace GeneratR.Database.SqlServer.Templates
{
    public class StoredProcedureTemplate : StringTemplateBase
    {
        private readonly SqlServerStoredProcedureConfiguration _config;
        private readonly DotNetGenerator _dotNet;

        public StoredProcedureTemplate(SqlServerStoredProcedureConfiguration config)
        {
            _config = config;
            _dotNet = config.DotNetGenerator;
        }

        public virtual string Generate()
        {
            var classAsAbstract = _config.DotNetModifier.HasFlag(DotNetModifierKeyword.Abstract);
            var classAsPartial = _config.DotNetModifier.HasFlag(DotNetModifierKeyword.Partial);

            WriteLine(_dotNet.CreateNamespaceStart(_config.Namespace));
            using (IndentScope())
            {
                WriteLine("using System;");
                WriteLine("using System.Collections.Generic;");
                WriteLine("using System.ComponentModel;");
                if (_config.AddAttributes)
                {
                    WriteLine("using System.ComponentModel.DataAnnotations;");
                    WriteLine("using System.ComponentModel.DataAnnotations.Schema;");
                }
                WriteLine();

                if (_config.AddAttributes)
                {
                    var attributes = new DotNetAttributeCollection();
                    // Create Table attribute if ClassName is different than the database object name, or if the schema is different than the default.
                    if (!_config.DbObject.Name.Equals(_config.ClassName, StringComparison.Ordinal) || !_config.DbObject.Schema.Equals("dbo", StringComparison.Ordinal))
                    {
                        attributes.AddIfNotExists(_dotNet.AttributeFactory.CreateTableAttribute(_config.DbObject.Name, _config.DbObject.Schema));
                    }
                    attributes.AddRange(_config.IncludeAttributes);
                    attributes.RemoveList(_config.ExcludeAttributes);
                    if (attributes.Any())
                    {
                        Write(attributes.ToMultilineString());
                    }
                }

                // Generate result class that contains Return value, and if any, output parameters and column resultset.
                WriteLine(_dotNet.CreateClassStart(_config.ClassName, classAsPartial, classAsAbstract, _config.InheritClassName, _config.ImplementInterfaces.ToArray()));
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
                                if (_config.AddAttributes)
                                {
                                    // TODO: If propertyname is different from parameter name, then add somekind of original name attribute
                                    var attrColllection = new DotNetAttributeCollection();
                                    attrColllection.AddList(p.IncludeAttributes);
                                    attrColllection.RemoveList(p.ExcludeAttributes);
                                    if (attrColllection.Any())
                                    {
                                        Write(attrColllection.ToMultilineString());
                                    }
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
                                if (_config.AddAttributes)
                                {
                                    var attrCollection = new DotNetAttributeCollection();

                                    var hasNameDiff = !string.Equals(col.DbObject.Name, col.PropertyName, StringComparison.OrdinalIgnoreCase);
                                    if (hasNameDiff)
                                    {
                                        var attr = _dotNet.AttributeFactory.CreateColumnAttribute(col.DbObject.Name);
                                        attrCollection.AddIfNotExists(attr);
                                    }

                                    attrCollection.AddList(col.IncludeAttributes);
                                    attrCollection.RemoveList(col.ExcludeAttributes);
                                    if (attrCollection.Any())
                                    {
                                        Write(attrCollection.ToMultilineString());
                                    }
                                }
                                WriteLine($"public {col.PropertyType} {col.PropertyName} {{ get; set; }}");
                            }
                        }
                        WriteLine(_dotNet.CreateClassEnd());
                        WriteLine();
                    }

                }
                WriteLine(_dotNet.CreateClassEnd());
                WriteLine();
            }
            WriteLine(_dotNet.CreateNamespaceEnd());

            return TemplateBuilder.ToString();
        }
    }
}
