using GeneratR.DotNet;
using GeneratR.Templating;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratR.Database.SqlServer.Templates
{
    public class DefaultTableTypeTemplate : StringTemplateBase, ITableTypeTemplate
    {
        protected static readonly HashSet<string> _variableStringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "varchar", "nvarchar", };
        protected static readonly HashSet<string> _fixedStringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "char", "nchar", };
        protected static readonly HashSet<string> _ansiStringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "char", "varchar", };
        protected static readonly HashSet<string> _unicodeStringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "nchar", "nvarchar", };
        protected static readonly HashSet<string> _allStringTypes = new HashSet<string>(_ansiStringTypes.Union(_unicodeStringTypes), StringComparer.OrdinalIgnoreCase);
        protected static readonly HashSet<string> _rowVersionTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "timestamp", "rowversion", };

        protected readonly SqlServerSchemaGenerator SchemaGenerator;
        protected readonly DotNetGenerator DotNetGenerator;
        protected readonly SqlServerTableTypeSettings ObjSettings;

        public DefaultTableTypeTemplate(SqlServerSchemaGenerator schemaGenerator)
        {
            SchemaGenerator = schemaGenerator;
            DotNetGenerator = schemaGenerator.DotNetGenerator;
            ObjSettings = schemaGenerator.Settings.TableType;
        }

        public virtual string Generate(SqlServerTableTypeConfiguration obj)
        {
            var inheritClassName = !string.IsNullOrWhiteSpace(obj.InheritClassName) ? obj.InheritClassName : ObjSettings.InheritClass;
            var classAsAbstract = obj.DotNetModifier.HasFlag(DotNetModifierKeyword.Abstract);

            WriteLine(DotNetGenerator.CreateNamespaceStart(obj.Namespace));
            WriteLine();
            using (IndentScope())
            {
                WriteLine("using System;");
                WriteLine("using System.Collections.Generic;");
                if (ObjSettings.AddAnnotations)
                {
                    WriteLine("using System.ComponentModel.DataAnnotations;");
                    WriteLine("using System.ComponentModel.DataAnnotations.Schema;");
                }
                WriteLine("using System.Data;");
                WriteLine("using Microsoft.SqlServer.Server;");
                WriteLine();

                if (ObjSettings.AddAnnotations)
                {
                    var attributes = new DotNetAttributeCollection();
                    // Create Table attribute if ClassName is different than the database object name, or if the schema is different than the default.
                    if (!obj.DbObject.Name.Equals(obj.ClassName, StringComparison.Ordinal) || !obj.DbObject.Schema.Equals("dbo", StringComparison.Ordinal))
                    {
                        attributes.AddIfNotExists(DotNetGenerator.AttributeFactory.CreateTableAttribute(obj.DbObject.Name, obj.DbObject.Schema));
                    }
                    attributes.AddRange(obj.IncludeAttributes);
                    attributes.RemoveList(obj.ExcludeAttributes);
                    if (attributes.Any())
                    {
                        Write(attributes.ToMultilineString());
                    }
                }

                WriteLine(DotNetGenerator.CreateClassStart(obj.ClassName, ObjSettings.ClassAsPartial, classAsAbstract, inheritClassName, ObjSettings.ImplementInterface));
                using (IndentScope())
                {
                    WriteLine($@"public static string SqlName => ""{obj.DbObject.FullName}"";");
                    WriteLine();

                    if (ObjSettings.AddConstructor)
                    {
                        WriteLine(DotNetGenerator.CreateConstructor(DotNetModifierKeyword.Public, obj.ClassName));
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

                        if (ObjSettings.AddAnnotations)
                        {
                            var attributes = new DotNetAttributeCollection();

                            if (col.DbObject.IsPrimaryKey)
                            {
                                var attr = DotNetGenerator.AttributeFactory.CreateKeyAttribute();
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
                                var attr = DotNetGenerator.AttributeFactory.CreateColumnAttribute(col.DbObject.Name, col.DbObject.Position - 1, typeName: columnAttributeTypeName);
                                attributes.AddIfNotExists(attr);
                            }
                            else if (col.DbObject.IsPrimaryKey)
                            {
                                var attr = DotNetGenerator.AttributeFactory.CreateColumnAttribute(col.DbObject.Position - 1, typeName: columnAttributeTypeName);
                                attributes.AddIfNotExists(attr);
                            }
                            else if (hasNameDiff)
                            {
                                var attr = DotNetGenerator.AttributeFactory.CreateColumnAttribute(col.DbObject.Name, typeName: columnAttributeTypeName);
                                attributes.AddIfNotExists(attr);
                            }
                            else if (!string.IsNullOrWhiteSpace(columnAttributeTypeName))
                            {
                                var attr = DotNetGenerator.AttributeFactory.CreateColumnAttribute(typeName: columnAttributeTypeName);
                                attributes.AddIfNotExists(attr);
                            }

                            if (_variableStringTypes.Contains(col.DbObject.DataType, StringComparer.OrdinalIgnoreCase))
                            {
                                DotNetAttribute attr;
                                if (col.DbObject.Length == -1)
                                {
                                    attr = DotNetGenerator.AttributeFactory.CreateMaxLengthAttribute();
                                }
                                else
                                {
                                    attr = DotNetGenerator.AttributeFactory.CreateStringLengthAttribute(col.DbObject.Length);
                                }
                                attributes.AddIfNotExists(attr);
                            }

                            if (_fixedStringTypes.Contains(col.DbObject.DataType, StringComparer.OrdinalIgnoreCase))
                            {
                                var attr = DotNetGenerator.AttributeFactory.CreateStringLengthAttribute(col.DbObject.Length, col.DbObject.Length);
                                attributes.AddIfNotExists(attr);
                            }

                            if (_allStringTypes.Contains(col.DbObject.DataType, StringComparer.OrdinalIgnoreCase) && !col.DbObject.IsNullable)
                            {
                                var attr = DotNetGenerator.AttributeFactory.CreateRequiredAttribute();
                                attributes.AddIfNotExists(attr);
                            }

                            attributes.AddList(col.IncludeAttributes);
                            attributes.RemoveList(col.ExcludeAttributes);
                            if (attributes.Any())
                            {
                                Write(attributes.ToMultilineString());
                            }
                        }

                        WriteLine(DotNetGenerator.CreateProperty(col.DotNetModifier, col.PropertyName, col.PropertyType, false));
                    }
                    WriteLine();

                    WriteLine("public SqlDataRecord ToSqlDataRecord()");
                    WriteLine("{");
                    using (IndentScope())
                    {
                        WriteLine("var record = new SqlDataRecord(_sqlColumnMetaData);");

                        foreach (var col in obj.Columns.OrderBy(x => x.DbObject.Position))
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
                        foreach (var col in obj.Columns.OrderBy(x => x.DbObject.Position))
                        {
                            var sqlDataType = SchemaGenerator.ConvertDataTypeToSqlDbType(col.DbObject.DataType);
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
                WriteLine(DotNetGenerator.CreateClassEnd());
                WriteLine();
            }
            WriteLine(DotNetGenerator.CreateNamespaceEnd());
            WriteLine();

            return TemplateBuilder.ToString();
        }
    }
}
