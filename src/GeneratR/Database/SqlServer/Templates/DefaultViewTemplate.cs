﻿using GeneratR.DotNet;
using GeneratR.Templating;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratR.Database.SqlServer.Templates
{
    public class DefaultViewTemplate : StringTemplateBase, IViewTemplate
    {
        protected static readonly HashSet<string> _variableStringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "varchar", "nvarchar", };
        protected static readonly HashSet<string> _fixedStringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "char", "nchar", };
        protected static readonly HashSet<string> _ansiStringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "char", "varchar", };
        protected static readonly HashSet<string> _unicodeStringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nchar", "nvarchar", };
        protected static readonly HashSet<string> _allStringTypes = new HashSet<string>(_ansiStringTypes.Union(_unicodeStringTypes), StringComparer.OrdinalIgnoreCase);
        protected static readonly HashSet<string> _rowVersionTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "timestamp", "rowversion", };

        private readonly DotNetGenerator _dotNetGenerator;
        private readonly SqlServerViewSettings _objSettings;

        public DefaultViewTemplate(SqlServerSchemaGenerator schemaGenerator)
        {
            _dotNetGenerator = schemaGenerator.DotNetGenerator;
            _objSettings = schemaGenerator.Settings.View;
        }

        public string Generate(SqlServerViewConfiguration obj)
        {
            var inheritClassName = !string.IsNullOrWhiteSpace(obj.InheritClassName) ? obj.InheritClassName : _objSettings.InheritClass;
            var classAsAbstract = obj.DotNetModifier.HasFlag(DotNetModifierKeyword.Abstract);

            WriteLine(_dotNetGenerator.CreateNamespaceStart(obj.Namespace));
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

                WriteLine(_dotNetGenerator.CreateClassStart(obj.ClassName, _objSettings.ClassAsPartial, classAsAbstract, inheritClassName, _objSettings.ImplementInterface));
                using (IndentScope())
                {
                    if (_objSettings.AddConstructor)
                    {
                        WriteLine(_dotNetGenerator.CreateConstructor(DotNetModifierKeyword.Public, obj.ClassName));
                    }

                    foreach (var col in obj.Columns.OrderBy(x => x.DbObject.Position))
                    {
                        // Column and class name must not be equal.
                        if (string.Equals(col.PropertyName, obj.ClassName, StringComparison.OrdinalIgnoreCase))
                        {
                            col.PropertyName += "Column";
                        }

                        WriteLine();
                        if (!string.IsNullOrWhiteSpace(col.DbObject.Description))
                        {
                            WriteLine($@"/// <summary>{col.DbObject.Description}</summary>");
                        }

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
                                var colLength = col.DbObject.Length == -1 ? "max" : col.DbObject.Length.ToString();
                                columnAttributeTypeName = $"{col.DbObject.DataType.ToLower()}({colLength})";
                            }
                            else if (col.DbObject.DataType.Equals("decimal", StringComparison.OrdinalIgnoreCase))
                            {
                                columnAttributeTypeName = $"decimal({col.DbObject.Precision}, {col.DbObject.Scale})";
                            }
                            else if (_rowVersionTypes.Contains(col.DbObject.DataType, StringComparer.OrdinalIgnoreCase))
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
                                var attr = _dotNetGenerator.AttributeFactory.CreateRequiredAttribute();
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
