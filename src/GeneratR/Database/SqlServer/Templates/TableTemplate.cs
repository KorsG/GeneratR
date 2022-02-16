using GeneratR.DotNet;
using GeneratR.Templating;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace GeneratR.Database.SqlServer.Templates
{
    public class TableTemplate : StringTemplateBase
    {
        private readonly SqlServerTableConfiguration _config;
        private readonly DotNetGenerator _dotNet;
        private readonly SqlServerTypeMapper _typeMapper;

        public TableTemplate(SqlServerTableConfiguration config)
        {
            _config = config;
            _dotNet = config.DotNetGenerator;
            _typeMapper = config.TypeMapper;
        }

        public virtual string Generate()
        {
            var classAsAbstract = _config.DotNetModifier.HasFlag(DotNetModifierKeyword.Abstract);
            var classAsPartial = _config.DotNetModifier.HasFlag(DotNetModifierKeyword.Partial);

            WriteLine(_dotNet.CreateNamespaceStart(_config.Namespace));
            WriteLine();
            using (IndentScope())
            {
                // TODO: Add as "using" write to "DotNetGenerator".
                WriteLine("using System;");
                WriteLine("using System.Collections.Generic;");
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

                WriteLine(_dotNet.CreateClassStart(_config.ClassName, classAsPartial, classAsAbstract, _config.InheritClassName, _config.ImplementInterfaces.ToArray()));
                using (IndentScope())
                {
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
                            var columnAttributeTypeName = _typeMapper.GetFullColumnDataType(col.DbObject);
                            /*
                            if (_typeMapper.DataTypeIsAnsiString(col.DbObject))
                            {
                                var colLength = col.DbObject.Length == -1 ? "max" : col.DbObject.Length.ToString();
                                columnAttributeTypeName = $"{col.DbObject.DataType.ToLower()}({colLength})";
                            }
                            else if (_typeMapper.DataTypeIsDecimal(col.DbObject))
                            {
                                columnAttributeTypeName = $"{col.DbObject.DataType.ToLower()}({col.DbObject.Precision}, {col.DbObject.Scale})";
                            }
                            else if (_typeMapper.DataTypeIsDateTime(col.DbObject))
                            {
                                if (_typeMapper.DataTypeIsDateTimeWithoutScale(col.DbObject))
                                {
                                    columnAttributeTypeName = $"{col.DbObject.DataType.ToLower()}";
                                }
                                else
                                {
                                    columnAttributeTypeName = $"{col.DbObject.DataType.ToLower()}({col.DbObject.Scale})";
                                }
                            }
                            else if (_typeMapper.DataTypeIsRowVersion(col.DbObject.DataType))
                            {
                                columnAttributeTypeName = "rowversion";
                            }
                            */

                            if (col.DbObject.IsPrimaryKey && hasNameDiff)
                            {
                                var keyAttr = _dotNet.AttributeFactory.CreateKeyAttribute();
                                attributes.AddIfNotExists(keyAttr);

                                var attr = _dotNet.AttributeFactory.CreateColumnAttribute(col.DbObject.Name, col.DbObject.Position - 1, typeName: columnAttributeTypeName);
                                attributes.AddIfNotExists(attr);
                            }
                            else if (col.DbObject.IsPrimaryKey)
                            {
                                var keyAttr = _dotNet.AttributeFactory.CreateKeyAttribute();
                                attributes.AddIfNotExists(keyAttr);

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

                            // Computed/Identity.
                            if (col.DbObject.IsIdentity)
                            {
                                attributes.AddIfNotExists(_dotNet.AttributeFactory.CreateDatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity));
                            }
                            else if (col.DbObject.IsComputed || _typeMapper.DataTypeIsRowVersion(col.DbObject))
                            {
                                attributes.AddIfNotExists(_dotNet.AttributeFactory.CreateDatabaseGeneratedAttribute(DatabaseGeneratedOption.Computed));
                            }
                            else if (col.DbObject.IsPrimaryKey)
                            {
                                // Explicitly set none when primary key is not autogenerated.
                                attributes.AddIfNotExists(_dotNet.AttributeFactory.CreateDatabaseGeneratedAttribute(DatabaseGeneratedOption.None));
                            }

                            if (_typeMapper.DataTypeIsVariableStringLength(col.DbObject))
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
                            else if (_typeMapper.DataTypeIsFixedStringLength(col.DbObject))
                            {
                                var attr = _dotNet.AttributeFactory.CreateStringLengthAttribute(col.DbObject.Length, col.DbObject.Length);
                                attributes.AddIfNotExists(attr);
                            }

                            if (_typeMapper.DataTypeIsString(col.DbObject) && !col.DbObject.IsNullable)
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

                    if (_config.GenerateForeignKeys)
                    {
                        foreach (var fk in _config.ForeignKeys.OrderBy(x => x.DbObject.ForeignKeyID))
                        {
                            WriteLine();
                            if (_config.AddAttributes)
                            {
                                var attributes = new DotNetAttributeCollection();

                                //var attr = DotNetGenerator.AttributeFactory.Create("Required");
                                //attributes.AddIfNotExists(attr);
                                //Foreign key names must be names of properties
                                //var attr = DotNetGenerator.AttributeFactory.Create("ForeignKey").SetArg(string.Join(",", fk.ToColumns.Select(x=> x.ColumnName)));
                                //attributes.AddIfNotExists(attr);

                                attributes.AddList(fk.IncludeAttributes);
                                attributes.RemoveList(fk.ExcludeAttributes);
                                if (attributes.Any())
                                {
                                    Write(attributes.ToMultilineString());
                                }
                            }

                            WriteLine($"{_dotNet.CommentOperator} FK - [FromTable]: {fk.DbObject.FromFullName}, [ToTable]: {fk.DbObject.ToFullName}, [FromColumns]: {string.Join(",", fk.DbObject.FromColumns.Select(x => x.ColumnName))}, [ToColumns]: {string.Join(",", fk.DbObject.ToColumns.Select(x => x.ColumnName))}, [Name]: {fk.DbObject.ForeignKeyName}, [IsOptional]: {fk.DbObject.IsOptional}");
                            WriteLine(_dotNet.CreateProperty(fk.DotNetModifier, fk.PropertyName, fk.PropertyType, false));
                        }
                    }

                    if (_config.GenerateReferencingForeignKeys)
                    {
                        var referencingKeys = _config.ReferencingForeignKeys.Where(x => x.DbObject.IsSelfReferencing == false);
                        foreach (var fk in referencingKeys)
                        {
                            WriteLine();
                            if (_config.AddAttributes)
                            {
                                var attributes = new DotNetAttributeCollection();

                                // Foreign key names must be names of properties
                                // var attr = DotNetGenerator.AttributeFactory.Create("ForeignKey").SetArg(string.Join(",", fk.FromColumns.Select(x=> x.ColumnName)));
                                // attrCol.AddIfNotExists(attr);

                                attributes.AddList(fk.IncludeAttributes);
                                attributes.RemoveList(fk.ExcludeAttributes);
                                if (attributes.Any())
                                {
                                    Write(attributes.ToMultilineString());
                                }
                            }

                            WriteLine($"{_dotNet.CommentOperator} FK(reverse) - [FromTable]: {fk.DbObject.FromFullName}, [ToTable]: {fk.DbObject.ToFullName}, [FromColumns]: {string.Join(",", fk.DbObject.FromColumns.Select(x => x.ColumnName))}, [ToColumns]: {string.Join(",", fk.DbObject.ToColumns.Select(x => x.ColumnName))}, [Name]: {fk.DbObject.ForeignKeyName}, [IsOptional]: {fk.DbObject.IsOptional}");
                            WriteLine(_dotNet.CreateProperty(fk.DotNetModifier, fk.PropertyName, fk.PropertyType, false));
                        }
                    }
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
