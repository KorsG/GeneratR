using GeneratR.DotNet;
using GeneratR.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer.Templates
{
    public class SqlServerTableFunctionTemplateCS : StringTemplateBase
    {
        private static readonly HashSet<string> _variableStringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "varchar", "nvarchar", };
        private static readonly HashSet<string> _fixedStringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "char", "nchar", };
        private static readonly HashSet<string> _ansiStringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "char", "varchar", };
        private static readonly HashSet<string> _unicodeStringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nchar", "nvarchar", };
        private static readonly HashSet<string> _allStringTypes = new HashSet<string>(_ansiStringTypes.Union(_unicodeStringTypes), StringComparer.OrdinalIgnoreCase);

        private readonly SqlServerTableFunctionConfiguration _obj;
        private readonly SqlServerSchemaGenerator _schemaGenerator;
        private readonly SqlServerTableFunctionSettings _objSettings;
        private readonly DotNetGenerator _dotNetGenerator;

        public SqlServerTableFunctionTemplateCS(SqlServerTableFunctionConfiguration dbObject, SqlServerSchemaGenerator schemaGenerator)
        {
            _obj = dbObject;
            _schemaGenerator = schemaGenerator;
            _dotNetGenerator = schemaGenerator.DotNetGenerator;
            _objSettings = schemaGenerator.Settings.TableFunction;
        }

        public string Generate()
        {
            var inheritClassName = !string.IsNullOrWhiteSpace(_obj.InheritClassName) ? _obj.InheritClassName : _objSettings.InheritClass;
            var classAsAbstract = _obj.DotNetModifier.HasFlag(DotNetModifierKeyword.Abstract);

            WriteLine(_dotNetGenerator.CreateNamespaceStart(_obj.Namespace));
            WriteLine();
            using (IndentScope())
            {
                WriteLine("using System;");
                WriteLine("using System.Collections.Generic;");
                if (_objSettings.AddAnnotations)
                {
                    WriteLine("using System.ComponentModel.DataAnnotations;");
                    WriteLine("using System.ComponentModel.DataAnnotations.Schema;");
                }
                WriteLine();

                if (_objSettings.AddAnnotations)
                {
                    var attributes = new DotNetAttributeCollection();
                    // Create Table attribute if ClassName is different than the database object name, or if the schema is different than the default.
                    if (!_obj.DbObject.Name.Equals(_obj.ClassName, StringComparison.Ordinal) || !_obj.DbObject.Schema.Equals("dbo", StringComparison.Ordinal))
                    {
                        attributes.AddIfNotExists(_dotNetGenerator.AttributeFactory.CreateTableAttribute(_obj.DbObject.Name, _obj.DbObject.Schema));
                    }
                    attributes.AddRange(_obj.IncludeAttributes);
                    attributes.RemoveList(_obj.ExcludeAttributes);
                    if (attributes.Any())
                    {
                        Write(attributes.ToMultilineString());
                    }
                }

                WriteLine(_dotNetGenerator.CreateClassStart(_obj.ClassName, _objSettings.ClassAsPartial, classAsAbstract, inheritClassName, _objSettings.ImplementInterface));
                using (IndentScope())
                {
                    if (_objSettings.AddConstructor)
                    {
                        WriteLine(_dotNetGenerator.CreateConstructor(DotNetModifierKeyword.Public, _obj.ClassName));
                    }
                    foreach (var col in _obj.Columns)
                    {
                        WriteLine();
                        if (_objSettings.AddAnnotations)
                        {
                            var attributes = new DotNetAttributeCollection();

                            if (col.DbObject.IsPrimaryKey)
                            {
                                var attr = _dotNetGenerator.AttributeFactory.CreateKeyAttribute();
                                attributes.AddIfNotExists(attr);
                            }

                            var hasNameDiff = !string.Equals(col.DbObject.Name, col.PropertyName, StringComparison.OrdinalIgnoreCase);
                            string columnAttributeTypeName = null;
                            if (_ansiStringTypes.Contains(col.DbObject.DataType, StringComparer.OrdinalIgnoreCase))
                            {
                                columnAttributeTypeName = col.DbObject.DataType.ToLower();
                            }
                            else if (col.DbObject.DataType.Equals("decimal", StringComparison.OrdinalIgnoreCase))
                            {
                                columnAttributeTypeName = $"decimal({col.DbObject.Precision}, {col.DbObject.Scale})";
                            }
                            else if (col.DbObject.DataType.Equals("timestamp", StringComparison.OrdinalIgnoreCase))
                            {
                                columnAttributeTypeName = "rowversion";
                            }

                            if (col.DbObject.IsPrimaryKey && hasNameDiff)
                            {
                                var attr = _dotNetGenerator.AttributeFactory.CreateColumnAttribute(col.DbObject.Name, col.DbObject.Position - 1, typeName: columnAttributeTypeName);
                                attributes.AddIfNotExists(attr);
                            }
                            else if (col.DbObject.IsPrimaryKey)
                            {
                                var attr = _dotNetGenerator.AttributeFactory.CreateColumnAttribute(col.DbObject.Position - 1, typeName: columnAttributeTypeName);
                                attributes.AddIfNotExists(attr);
                            }
                            else if (hasNameDiff)
                            {
                                var attr = _dotNetGenerator.AttributeFactory.CreateColumnAttribute(col.DbObject.Name, typeName: columnAttributeTypeName);
                                attributes.AddIfNotExists(attr);
                            }
                            else if (!string.IsNullOrWhiteSpace(columnAttributeTypeName))
                            {
                                var attr = _dotNetGenerator.AttributeFactory.CreateColumnAttribute(typeName: columnAttributeTypeName);
                                attributes.AddIfNotExists(attr);
                            }

                            if (_variableStringTypes.Contains(col.DbObject.DataType, StringComparer.OrdinalIgnoreCase))
                            {
                                DotNetAttribute attr;
                                if (col.DbObject.Length == -1)
                                {
                                    attr = _dotNetGenerator.AttributeFactory.CreateMaxLengthAttribute();
                                }
                                else
                                {
                                    attr = _dotNetGenerator.AttributeFactory.CreateStringLengthAttribute(col.DbObject.Length);
                                }
                                attributes.AddIfNotExists(attr);
                            }

                            if (_fixedStringTypes.Contains(col.DbObject.DataType, StringComparer.OrdinalIgnoreCase))
                            {
                                var attr = _dotNetGenerator.AttributeFactory.CreateStringLengthAttribute(col.DbObject.Length, col.DbObject.Length);
                                attributes.AddIfNotExists(attr);
                            }

                            if (_allStringTypes.Contains(col.DbObject.DataType, StringComparer.OrdinalIgnoreCase) && !col.DbObject.IsNullable)
                            {
                                var attr = _dotNetGenerator.AttributeFactory.Create("Required");
                                attributes.AddIfNotExists(attr);
                            }

                            attributes.AddList(col.IncludeAttributes);
                            attributes.RemoveList(col.ExcludeAttributes);
                            if (attributes.Any())
                            {
                                Write(attributes.ToMultilineString());
                            }
                        }

                        WriteLine(_dotNetGenerator.CreateProperty(col.DotNetModifier, col.PropertyName, col.PropertyType, false));
                    }

                }
                WriteLine(_dotNetGenerator.CreateClassEnd());
                WriteLine();
            }
            WriteLine(_dotNetGenerator.CreateNamespaceEnd());
            WriteLine();

            return TemplateBuilder.ToString();
        }
    }
}
