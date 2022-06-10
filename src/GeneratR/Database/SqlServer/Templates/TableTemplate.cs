using GeneratR.DotNet;
using GeneratR.Templating;
using System.Linq;

namespace GeneratR.Database.SqlServer.Templates
{
    public class TableTemplate : StringTemplateBase
    {
        private readonly SqlServerTableCodeModel _config;
        private readonly DotNetGenerator _dotNet;

        public TableTemplate(SqlServerTableCodeModel config)
        {
            _config = config;
            _dotNet = config.DotNetGenerator;
        }

        public virtual string Generate()
        {
            // TODO: Add as "using" write to "DotNetGenerator".
            WriteLine("using System;");
            WriteLine("using System.Collections.Generic;");
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
                WriteLine(_dotNet.CreateClassStart(_config.ClassName, classAsPartial, classAsAbstract, _config.InheritClassName, _config.ImplementInterfaces));
                using (IndentScope())
                {
                    if (_config.AddConstructor)
                    {
                        WriteLine(_dotNet.CreateConstructor(DotNetModifierKeyword.Public, _config.ClassName));
                    }

                    foreach (var col in _config.Columns.OrderBy(x => x.DbObject.Position))
                    {
                        WriteLine();
                        if (!string.IsNullOrWhiteSpace(col.DbObject.Description))
                        {
                            // TODO: Add summary/describe function to DotNetGenerator.
                            WriteLine($@"/// <summary>{col.DbObject.Description}</summary>");
                        }

                        if (col.Attributes.Any())
                        {
                            Write(col.Attributes.ToMultilineString());
                        }
                        WriteLine(_dotNet.CreateProperty(col.DotNetModifier, col.PropertyName, col.PropertyType, false));
                    }

                    if (_config.GenerateForeignKeys)
                    {
                        foreach (var fk in _config.ForeignKeys.OrderBy(x => x.DbObject.ForeignKeyID))
                        {
                            WriteLine();

                            if (fk.Attributes.Any())
                            {
                                Write(fk.Attributes.ToMultilineString());
                            }

                            // TODO: Make it configurable if this comment should be made.
                            //WriteLine($"{_dotNet.CommentOperator} FK - [FromTable]: {fk.DbObject.FromFullName}, [ToTable]: {fk.DbObject.ToFullName}, [FromColumns]: {string.Join(",", fk.DbObject.FromColumns.Select(x => x.ColumnName))}, [ToColumns]: {string.Join(",", fk.DbObject.ToColumns.Select(x => x.ColumnName))}, [Name]: {fk.DbObject.ForeignKeyName}, [IsOptional]: {fk.DbObject.IsOptional}");
                            WriteLine(_dotNet.CreateProperty(fk.DotNetModifier, fk.PropertyName, fk.PropertyType, false));
                        }
                    }

                    if (_config.GenerateReferencingForeignKeys)
                    {
                        var referencingKeys = _config.ReferencingForeignKeys.Where(x => x.DbObject.IsSelfReferencing == false);
                        foreach (var fk in referencingKeys)
                        {
                            WriteLine();

                            if (fk.Attributes.Any())
                            {
                                Write(fk.Attributes.ToMultilineString());
                            }

                            // TODO: Make it configurable if this comment should be made.
                            //WriteLine($"{_dotNet.CommentOperator} FK(reverse) - [FromTable]: {fk.DbObject.FromFullName}, [ToTable]: {fk.DbObject.ToFullName}, [FromColumns]: {string.Join(",", fk.DbObject.FromColumns.Select(x => x.ColumnName))}, [ToColumns]: {string.Join(",", fk.DbObject.ToColumns.Select(x => x.ColumnName))}, [Name]: {fk.DbObject.ForeignKeyName}, [IsOptional]: {fk.DbObject.IsOptional}");
                            WriteLine(_dotNet.CreateProperty(fk.DotNetModifier, fk.PropertyName, fk.PropertyType, false));
                        }
                    }
                }
                WriteLine(_dotNet.CreateClassEnd());
            }
            WriteLine(_dotNet.CreateNamespaceEnd());

            return TemplateBuilder.ToString();
        }
    }
}
