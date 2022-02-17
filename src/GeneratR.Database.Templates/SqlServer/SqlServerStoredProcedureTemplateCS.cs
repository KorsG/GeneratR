using GeneratR.Database.SqlServer;
using GeneratR.Database.SqlServer.Schema;
using GeneratR.DotNet;
using GeneratR.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer.Templates
{
    public class SqlServerStoredProcedureTemplateCS : StringTemplateBase
    {
        private readonly SqlServerStoredProcedureConfiguration _obj;
        private readonly SqlServerSchemaGenerator _schemaGenerator;
        private readonly SqlServerStoredProcedureSettings _settings;
        private readonly DotNetGenerator _dotNetGenerator;

        public SqlServerStoredProcedureTemplateCS(SqlServerStoredProcedureConfiguration dbObject, SqlServerSchemaGenerator schemaGenerator)
        {
            _obj = dbObject;
            _schemaGenerator = schemaGenerator;
            _dotNetGenerator = schemaGenerator.DotNetGenerator;
            _settings = schemaGenerator.Settings.StoredProcedure;
        }

        public string Generate()
        {
            var inheritClassName = !string.IsNullOrWhiteSpace(_obj.InheritClassName) ? _obj.InheritClassName : _settings.InheritClass;
            var classAsAbstract = _obj.DotNetModifier.HasFlag(DotNetModifierKeyword.Abstract);
            var classAsPartial = _obj.DotNetModifier.HasFlag(DotNetModifierKeyword.Partial);

            WriteLine(_dotNetGenerator.CreateNamespaceStart(_obj.Namespace));
            using (IndentScope())
            {
                WriteLine("using System;");
                WriteLine("using System.Collections.Generic;");
                WriteLine("using System.ComponentModel;");
                if (_settings.AddDataAnnotationAttributes)
                {
                    WriteLine("using System.ComponentModel.DataAnnotations;");
                    WriteLine("using System.ComponentModel.DataAnnotations.Schema;");
                }
                WriteLine();

                if (_settings.AddDataAnnotationAttributes)
                {
                    var attributes = new DotNetAttributeCollection();
                    // Create Table attribute if ClassName is different than the database object name, or if the schema is different than the default.
                    if (!_obj.DbObject.Name.Equals(_obj.ClassName, StringComparison.Ordinal) || !_obj.DbObject.Schema.Equals("dbo", StringComparison.Ordinal))
                    {
                        attributes.AddIfNotExists(_dotNetGenerator.AttributeFactory.CreateTableAttribute(_obj.DbObject.Name, _obj.DbObject.Schema));
                    }
                    attributes.AddRange(_obj.Attributes);
                    if (attributes.Any())
                    {
                        Write(attributes.ToMultilineString());
                    }
                }

                // Generate result class that contains Return value, and if any, output parameters and column resultset.
                WriteLine(_dotNetGenerator.CreateClassStart(_obj.ClassName, classAsPartial, classAsAbstract, inheritClassName, _settings.ImplementInterface));
                using (IndentScope())
                {
                    var generateOutputParameters = _settings.GenerateOutputParameters && _obj.DbObject.HasOutputParameters;
                    var outputParameterClassName = "OutputParametersModel";

                    var generateResultSet = _settings.GenerateResultSet && _obj.DbObject.HasResultColumns;
                    var resultSetClassName = "ResultModel";

                    if (_settings.AddConstructor)
                    {
                        WriteLine(@"public {0}()", _obj.ClassName);
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

                            foreach (var p in _obj.Parameters.Where(x => x.DbObject.Direction == ParameterDirection.InAndOutDirection || x.DbObject.Direction == ParameterDirection.OutDirection))
                            {
                                if (_settings.AddDataAnnotationAttributes)
                                {
                                    // TODO: If propertyname is different from parameter name, then add somekind of original name attribute
                                    var attrColllection = new DotNetAttributeCollection();
                                    attrColllection.AddList(p.Attributes);
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

                            foreach (var col in _obj.ResultColumns)
                            {
                                if (_settings.AddDataAnnotationAttributes)
                                {
                                    var attrCollection = new DotNetAttributeCollection();

                                    var hasNameDiff = !string.Equals(col.DbObject.Name, col.PropertyName, StringComparison.OrdinalIgnoreCase);
                                    if (hasNameDiff)
                                    {
                                        var attr = _dotNetGenerator.AttributeFactory.CreateColumnAttribute(col.DbObject.Name);
                                        attrCollection.AddIfNotExists(attr);
                                    }

                                    attrCollection.AddList(col.Attributes);
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
