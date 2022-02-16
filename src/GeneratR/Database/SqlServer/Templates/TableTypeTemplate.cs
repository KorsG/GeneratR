using GeneratR.DotNet;
using GeneratR.Templating;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratR.Database.SqlServer.Templates
{
    public class TableTypeTemplate : StringTemplateBase
    {
        protected static readonly HashSet<string> _variableStringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "varchar", "nvarchar", };
        protected static readonly HashSet<string> _fixedStringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "char", "nchar", };
        protected static readonly HashSet<string> _ansiStringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "char", "varchar", };
        protected static readonly HashSet<string> _unicodeStringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nchar", "nvarchar", };
        protected static readonly HashSet<string> _allStringTypes = new HashSet<string>(_ansiStringTypes.Union(_unicodeStringTypes), StringComparer.OrdinalIgnoreCase);
        protected static readonly HashSet<string> _rowVersionTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "timestamp", "rowversion", };

        private readonly SqlServerTableTypeConfiguration _config;
        private readonly DotNetGenerator _dotNet;
        private readonly SqlServerTypeMapper _typeMapper;

        public TableTypeTemplate(SqlServerTableTypeConfiguration config)
        {
            _config = config;
            _dotNet = _config.DotNetGenerator;
            _typeMapper = _config.TypeMapper;
        }

        public virtual string Generate()
        {
            var classAsAbstract = _config.DotNetModifier.HasFlag(DotNetModifierKeyword.Abstract);
            var classAsPartial = _config.DotNetModifier.HasFlag(DotNetModifierKeyword.Partial);

            WriteLine(_dotNet.CreateNamespaceStart(_config.Namespace));
            WriteLine();
            using (IndentScope())
            {
                WriteLine("using System;");
                WriteLine("using System.Collections.Generic;");
                if (_config.AddAttributes)
                {
                    WriteLine("using System.ComponentModel.DataAnnotations;");
                    WriteLine("using System.ComponentModel.DataAnnotations.Schema;");
                }
                WriteLine("using System.Data;");
                WriteLine("using Microsoft.SqlServer.Server;");
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

                WriteLine(_dotNet.CreateClassStart(_config.ClassName, classAsPartial, classAsAbstract, _config.InheritClassName, _config.ImplementInterfaces.ToArray()));
                using (IndentScope())
                {
                    WriteLine($@"public static string SqlName => ""{_config.DbObject.FullName}"";");
                    WriteLine();

                    if (_config.AddConstructor)
                    {
                        WriteLine(_dotNet.CreateConstructor(DotNetModifierKeyword.Public, _config.ClassName));
                    }

                    foreach (var col in _config.Columns.OrderBy(x => x.DbObject.Position))
                    {
                        // Column and class name must not be equal.
                        if (string.Equals(col.PropertyName, _config.ClassName, StringComparison.OrdinalIgnoreCase))
                        {
                            col.PropertyName += "Column";
                        }

                        WriteLine();
                        if (!string.IsNullOrWhiteSpace(col.DbObject.Description))
                        {
                            WriteLine($@"/// <summary>{col.DbObject.Description}</summary>");
                        }

                        if (_config.AddAttributes)
                        {
                            var attributes = new DotNetAttributeCollection();

                            if (col.DbObject.IsPrimaryKey)
                            {
                                var attr = _dotNet.AttributeFactory.CreateKeyAttribute();
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
                                var attr = _dotNet.AttributeFactory.CreateColumnAttribute(col.DbObject.Name, col.DbObject.Position - 1, typeName: columnAttributeTypeName);
                                attributes.AddIfNotExists(attr);
                            }
                            else if (col.DbObject.IsPrimaryKey)
                            {
                                var attr = _dotNet.AttributeFactory.CreateColumnAttribute(col.DbObject.Position - 1, typeName: columnAttributeTypeName);
                                attributes.AddIfNotExists(attr);
                            }
                            else if (hasNameDiff)
                            {
                                var attr = _dotNet.AttributeFactory.CreateColumnAttribute(col.DbObject.Name, typeName: columnAttributeTypeName);
                                attributes.AddIfNotExists(attr);
                            }
                            else if (!string.IsNullOrWhiteSpace(columnAttributeTypeName))
                            {
                                var attr = _dotNet.AttributeFactory.CreateColumnAttribute(typeName: columnAttributeTypeName);
                                attributes.AddIfNotExists(attr);
                            }

                            if (_variableStringTypes.Contains(col.DbObject.DataType, StringComparer.OrdinalIgnoreCase))
                            {
                                DotNetAttribute attr;
                                if (col.DbObject.Length == -1)
                                {
                                    attr = _dotNet.AttributeFactory.CreateMaxLengthAttribute();
                                }
                                else
                                {
                                    attr = _dotNet.AttributeFactory.CreateStringLengthAttribute(col.DbObject.Length);
                                }
                                attributes.AddIfNotExists(attr);
                            }

                            if (_fixedStringTypes.Contains(col.DbObject.DataType, StringComparer.OrdinalIgnoreCase))
                            {
                                var attr = _dotNet.AttributeFactory.CreateStringLengthAttribute(col.DbObject.Length, col.DbObject.Length);
                                attributes.AddIfNotExists(attr);
                            }

                            if (_allStringTypes.Contains(col.DbObject.DataType, StringComparer.OrdinalIgnoreCase) && !col.DbObject.IsNullable)
                            {
                                var attr = _dotNet.AttributeFactory.CreateRequiredAttribute();
                                attributes.AddIfNotExists(attr);
                            }

                            attributes.AddList(col.IncludeAttributes);
                            attributes.RemoveList(col.ExcludeAttributes);
                            if (attributes.Any())
                            {
                                Write(attributes.ToMultilineString());
                            }
                        }

                        WriteLine(_dotNet.CreateProperty(col.DotNetModifier, col.PropertyName, col.PropertyType, false));
                    }
                    WriteLine();

                    WriteLine("public SqlDataRecord ToSqlDataRecord()");
                    WriteLine("{");
                    using (IndentScope())
                    {
                        WriteLine("var record = new SqlDataRecord(_sqlColumnMetaData);");

                        foreach (var col in _config.Columns.OrderBy(x => x.DbObject.Position))
                        {
                            WriteLine($@"record.SetValue({col.DbObject.Position - 1}, {col.PropertyName});");
                        }

                        WriteLine("return record;");
                    }
                    WriteLine("}");
                    WriteLine();

                    WriteLine("private static readonly SqlMetaData[] _sqlColumnMetaData = new[]");
                    WriteLine("{");
                    using (IndentScope())
                    {
                        foreach (var col in _config.Columns.OrderBy(x => x.DbObject.Position))
                        {
                            var sqlDataType = _typeMapper.ConvertDataTypeToSqlDbType(col.DbObject.DataType);
                            if (_allStringTypes.Contains(col.DbObject.DataType, StringComparer.OrdinalIgnoreCase))
                            {
                                WriteLine($@"new SqlMetaData(""{col.PropertyName}"", {sqlDataType}, {col.DbObject.Length}),");
                            }
                            else if (col.DbObject.DataType.Equals("decimal", StringComparison.OrdinalIgnoreCase))
                            {
                                WriteLine($@"new SqlMetaData(""{col.PropertyName}"", {sqlDataType}, {col.DbObject.Precision}, {col.DbObject.Scale}),");
                            }
                            else
                            {
                                WriteLine($@"new SqlMetaData(""{col.PropertyName}"", {sqlDataType}),");
                            }
                        }
                    }
                    WriteLine("};");
                }
                WriteLine(_dotNet.CreateClassEnd());
                WriteLine();
            }
            WriteLine(_dotNet.CreateNamespaceEnd());
            WriteLine();

            return TemplateBuilder.ToString();
        }
    }
}
