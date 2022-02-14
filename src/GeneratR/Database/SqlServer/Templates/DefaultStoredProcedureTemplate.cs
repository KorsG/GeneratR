using GeneratR.Database.SqlServer.Schema;
using GeneratR.DotNet;
using GeneratR.Templating;
using System;
using System.Linq;

namespace GeneratR.Database.SqlServer.Templates
{
    public class DefaultStoredProcedureTemplate : StringTemplateBase, IStoredProcedureTemplate
    {
        private readonly SqlServerStoredProcedureSettings _settings;
        private readonly DotNetGenerator _dotNetGenerator;

        public DefaultStoredProcedureTemplate(SqlServerSchemaGenerator schemaGenerator)
        {
            _dotNetGenerator = schemaGenerator.DotNetGenerator;
            _settings = schemaGenerator.Settings.StoredProcedure;
        }

        public virtual string Generate(SqlServerStoredProcedureConfiguration obj)
        {
            var inheritClassName = !string.IsNullOrWhiteSpace(obj.InheritClassName) ? obj.InheritClassName : _settings.InheritClass;
            var classAsAbstract = obj.DotNetModifier.HasFlag(DotNetModifierKeyword.Abstract);

            WriteLine(_dotNetGenerator.CreateNamespaceStart(obj.Namespace));
            using (IndentScope())
            {
                WriteLine("using System;");
                WriteLine("using System.Collections.Generic;");
                WriteLine("using System.ComponentModel;");
                if (_settings.AddAnnotations)
                {
                    WriteLine("using System.ComponentModel.DataAnnotations;");
                    WriteLine("using System.ComponentModel.DataAnnotations.Schema;");
                }
                WriteLine();

                if (_settings.AddAnnotations)
                {
                    var attributes = new DotNetAttributeCollection();
                    // Create Table attribute if ClassName is different than the database object name, or if the schema is different than the default.
                    if (!obj.DbObject.Name.Equals(obj.ClassName, StringComparison.Ordinal) || !obj.DbObject.Schema.Equals("dbo", StringComparison.Ordinal))
                    {
                        attributes.AddIfNotExists(_dotNetGenerator.AttributeFactory.CreateTableAttribute(obj.DbObject.Name, obj.DbObject.Schema));
                    }
                    attributes.AddRange(obj.IncludeAttributes);
                    attributes.RemoveList(obj.ExcludeAttributes);
                    if (attributes.Any())
                    {
                        Write(attributes.ToMultilineString());
                    }
                }

                // Generate result class that contains Return value, and if any, output parameters and column resultset.
                WriteLine(_dotNetGenerator.CreateClassStart(obj.ClassName, _settings.ClassAsPartial, classAsAbstract, inheritClassName, _settings.ImplementInterface));
                using (IndentScope())
                {
                    var generateOutputParameters = _settings.GenerateOutputParameters && obj.DbObject.HasOutputParameters;
                    var outputParameterClassName = "OutputParametersModel";

                    var generateResultSet = _settings.GenerateResultSet && obj.DbObject.HasResultColumns;
                    var resultSetClassName = "ResultModel";

                    if (_settings.AddConstructor)
                    {
                        WriteLine(@"public {0}()", obj.ClassName);
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
                        WriteLine(_dotNetGenerator.CreateClassStart(outputParameterClassName, false, false, string.Empty, string.Empty));
                        using (IndentScope())
                        {
                            if (_settings.AddConstructor)
                            {
                                WriteLine("public {0}(){1}{{{1}}}{1}", outputParameterClassName, Environment.NewLine);
                            }

                            foreach (var p in obj.Parameters.Where(x => x.DbObject.Direction == ParameterDirection.InAndOutDirection || x.DbObject.Direction == ParameterDirection.OutDirection))
                            {
                                if (_settings.AddAnnotations)
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
                        WriteLine(_dotNetGenerator.CreateClassEnd());
                        WriteLine();
                    }

                    // Result class.
                    if (generateResultSet)
                    {
                        WriteLine("[EditorBrowsable(EditorBrowsableState.Never)]");
                        WriteLine(_dotNetGenerator.CreateClassStart(resultSetClassName, false, false, string.Empty, string.Empty));
                        using (IndentScope())
                        {
                            if (_settings.AddConstructor)
                            {
                                WriteLine("public {0}(){1}{{{1}}}{1}", resultSetClassName, Environment.NewLine);
                            }

                            foreach (var col in obj.ResultColumns)
                            {
                                if (_settings.AddAnnotations)
                                {
                                    var attrCollection = new DotNetAttributeCollection();

                                    var hasNameDiff = !string.Equals(col.DbObject.Name, col.PropertyName, StringComparison.OrdinalIgnoreCase);
                                    if (hasNameDiff)
                                    {
                                        var attr = _dotNetGenerator.AttributeFactory.CreateColumnAttribute(col.DbObject.Name);
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
                        WriteLine(_dotNetGenerator.CreateClassEnd());
                        WriteLine();
                    }

                }
                WriteLine(_dotNetGenerator.CreateClassEnd());
                WriteLine();
            }
            WriteLine(_dotNetGenerator.CreateNamespaceEnd());

            return TemplateBuilder.ToString();
        }
    }
}
