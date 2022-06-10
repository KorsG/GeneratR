﻿using GeneratR.DotNet;
using GeneratR.Templating;
using System.Linq;

namespace GeneratR.Database.SqlServer.Templates
{
    public class TableTemplate : StringTemplateBase
    {
        private readonly SqlServerTableCodeModel _model;
        private readonly DotNetGenerator _dotNet;

        public TableTemplate(SqlServerTableCodeModel model)
        {
            _model = model;
            _dotNet = model.DotNetGenerator;
        }

        public virtual string Generate()
        {
            // TODO: Add as "using" write to "DotNetGenerator".
            WriteLine("using System;");
            WriteLine("using System.Collections.Generic;");
            if (_model.AddDataAnnotationAttributes)
            {
                WriteLine("using System.ComponentModel.DataAnnotations;");
                WriteLine("using System.ComponentModel.DataAnnotations.Schema;");
            }
            WriteLine();
            WriteLine(_dotNet.CreateNamespaceStart(_model.Namespace));
            using (IndentScope())
            {
                var classAsAbstract = _model.DotNetModifier.HasFlag(DotNetModifierKeyword.Abstract);
                var classAsPartial = _model.DotNetModifier.HasFlag(DotNetModifierKeyword.Partial);

                if (_model.Attributes.Any())
                {
                    Write(_model.Attributes.ToMultilineString());
                }
                WriteLine(_dotNet.CreateClassStart(_model.ClassName, classAsPartial, classAsAbstract, _model.InheritClassName, _model.ImplementInterfaces));
                using (IndentScope())
                {
                    if (_model.AddConstructor)
                    {
                        WriteLine(_dotNet.CreateConstructor(DotNetModifierKeyword.Public, _model.ClassName));
                    }

                    foreach (var col in _model.Columns.OrderBy(x => x.DbObject.Position))
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

                    if (_model.GenerateForeignKeys)
                    {
                        foreach (var fk in _model.ForeignKeys.OrderBy(x => x.DbObject.ForeignKeyID))
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

                    if (_model.GenerateReferencingForeignKeys)
                    {
                        var referencingKeys = _model.ReferencingForeignKeys.Where(x => x.DbObject.IsSelfReferencing == false);
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
