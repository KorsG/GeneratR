using GeneratR.Database.SqlServer.Schema;
using GeneratR.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratR.Database.SqlServer
{
    public class LinqToDbSqlServerGenerator : SqlServerSchemaGenerator
    {
        public LinqToDbSqlServerGenerator(LinqToDbSqlServerGeneratorSettings settings, DotNetLanguageType dotNetLanguage = DotNetLanguageType.CS)
            : base(settings, DotNetGenerator.Create(dotNetLanguage), new LinqToDbTypeMapper(DotNetGenerator.Create(dotNetLanguage)))
        {
        }

        public LinqToDbSqlServerGenerator(LinqToDbSqlServerGeneratorSettings settings, DotNetGenerator dotNetGenerator)
            : base(settings, dotNetGenerator, new LinqToDbTypeMapper(dotNetGenerator))
        {
        }

#if NET5_0_OR_GREATER
        public override LinqToDbSqlServerGeneratorSettings Settings => (LinqToDbSqlServerGeneratorSettings)base.Settings;
        public override LinqToDbTypeMapper TypeMapper => (LinqToDbTypeMapper)base.TypeMapper;
#else
        public new LinqToDbSqlServerGeneratorSettings Settings => (LinqToDbSqlServerGeneratorSettings)base.Settings;
        public new LinqToDbTypeMapper TypeMapper => (LinqToDbTypeMapper)base.TypeMapper;
#endif

        public override SqlServerSchema LoadSchema()
        {
            var schema = base.LoadSchema();
            return schema;
        }

        public override SqlServerSchemaCodeModels BuildCodeModel(SqlServerSchema schema)
        {
            var codeModels = base.BuildCodeModel(schema);
            EnrichCodeModel(codeModels);
            return codeModels;
        }

        public override IEnumerable<SourceCodeFile> GenerateCodeFiles(SqlServerSchemaCodeModels codeModels)
        {
            var baseFiles = base.GenerateCodeFiles(codeModels);

            if (!Settings.DataConnection.Generate)
            {
                return baseFiles;
            }

            var codeModel = BuildDataConnectionCodeModel(codeModels);

            var codeFile = new SourceCodeFile()
            {
                FileName = $"{Settings.DataConnection.ClassName}.generated{DotNetGenerator.FileExtension}",
                FolderPath = BuildObjectOutputFolderPath(Settings.DataConnection),
                Code = GenerateDataConnectionCode(codeModel),
            };

            return baseFiles.Concat(new[] { codeFile });
        }

        public override void WriteCodeFiles(IEnumerable<SourceCodeFile> codeFiles)
        {
            base.WriteCodeFiles(codeFiles);
        }

        public virtual LinqToDbDataConnectionCodeModel BuildDataConnectionCodeModel(SqlServerSchemaCodeModels codeModels)
        {
            var model = new LinqToDbDataConnectionCodeModel(codeModels)
            {
                ClassName = Settings.DataConnection.ClassName,
                Namespace = BuildObjectNamespace(Settings.DataConnection.Namespace, Settings.DataConnection.ClassName),
                InheritClassName = Settings.DataConnection.InheritClass,
                ImplementInterfaces = Settings.DataConnection.ImplementInterfaces ?? new List<string>(),
                AddConstructor = Settings.DataConnection.AddConstructor,
                DotNetModifier = Settings.DataConnection.Modifiers,
            };

            model.AddNamespaceImports("System", "System.Collections.Generic", "System.Reflection", "System.Threading", "System.Threading.Tasks", "LinqToDB", "LinqToDB.Configuration", "LinqToDB.Data");
            model.AddNamespaceImports(codeModels.GetNamespaces());

            return model;
        }

        public Func<LinqToDbDataConnectionCodeModel, string> GenerateDataConnectionCodeFunc { get; set; } = null;

        public virtual string GenerateDataConnectionCode(LinqToDbDataConnectionCodeModel model)
        {
            var code = GenerateDataConnectionCodeFunc?.Invoke(model);
            if (code == null)
            {
                code = new LinqToDbDataConnectionTemplate(model, TypeMapper, DotNetGenerator).Generate();
            }
            return code;
        }

        private void EnrichCodeModel(SqlServerSchemaCodeModels model)
        {
            if (Settings.AddLinqToDbMappingAttributes)
            {
                foreach (var t in model.Tables)
                {
                    var attributeNames = PrepareLinqToDbAttributeNames(t);

                    t.AddAttribute(DotNetGenerator.AttributeFactory.Create(attributeNames.Table)
                        .SetOptionalArg("Name", t.DbObject.Name, true)
                        .SetOptionalArg("Schema", t.DbObject.Schema, true)
                        .SetOptionalArg("IsColumnAttributeRequired", false)
                        );

                    foreach (var col in t.Columns)
                    {
                        AddColumnAttributes(col, attributeNames.Column);
                    }

                    foreach (var fk in t.ForeignKeys.Where(x => !x.IsInverse))
                    {
                        var attr = DotNetGenerator.AttributeFactory.Create(attributeNames.Association)
                            .SetOptionalArg("ThisKey", string.Join(",", fk.DbObject.FromColumns.Select(c => c.ColumnName)), wrapValueInQuotes: true)
                            .SetOptionalArg("OtherKey", string.Join(",", fk.DbObject.ToColumns.Select(c => c.ColumnName)), wrapValueInQuotes: true)
                            .SetOptionalArg("CanBeNull", fk.DbObject.IsOptional);

                        fk.Attributes.Add(attr);
                    }

                    foreach (var fk in t.ForeignKeys.Where(x => x.IsInverse))
                    {
                        var attr = DotNetGenerator.AttributeFactory.Create(attributeNames.Association)
                            .SetOptionalArg("ThisKey", string.Join(",", fk.DbObject.ToColumns.Select(c => c.ColumnName)), wrapValueInQuotes: true)
                            .SetOptionalArg("OtherKey", string.Join(",", fk.DbObject.FromColumns.Select(c => c.ColumnName)), wrapValueInQuotes: true)
                            .SetOptionalArg("CanBeNull", fk.DbObject.IsOptional);

                        fk.Attributes.Add(attr);
                    }
                }

                foreach (var t in model.Views)
                {
                    var attributeNames = PrepareLinqToDbAttributeNames(t);

                    t.AddAttribute(DotNetGenerator.AttributeFactory.Create(attributeNames.Table)
                        .SetOptionalArg("Name", t.DbObject.Name, true)
                        .SetOptionalArg("Schema", t.DbObject.Schema, true)
                        .SetOptionalArg("IsView", true)
                        );

                    foreach (var col in t.Columns)
                    {
                        AddColumnAttributes(col, attributeNames.Column);
                    }
                }

                foreach (var t in model.TableFunctions)
                {
                    var attributeNames = PrepareLinqToDbAttributeNames(t);

                    t.AddAttribute(DotNetGenerator.AttributeFactory.Create(attributeNames.Table)
                        .SetOptionalArg("Name", t.DbObject.Name, true)
                        .SetOptionalArg("Schema", t.DbObject.Schema, true)
                        );

                    foreach (var col in t.Columns)
                    {
                        AddColumnAttributes(col, attributeNames.Column);
                    }
                }

                foreach (var t in model.TableTypes)
                {
                    var attributeNames = PrepareLinqToDbAttributeNames(t);

                    t.AddAttribute(DotNetGenerator.AttributeFactory.Create(attributeNames.Table)
                        .SetOptionalArg("Name", t.DbObject.Name, true)
                        .SetOptionalArg("Schema", t.DbObject.Schema, true)
                        );

                    foreach (var col in t.Columns)
                    {
                        AddColumnAttributes(col, attributeNames.Column);
                    }
                }
            }
        }

        private (string Table, string Column, string Association) PrepareLinqToDbAttributeNames(ClassCodeModel codeModel)
        {
            const string mappingNs = "LinqToDB.Mapping";
            if (codeModel.NamespaceImports.Any(x => DataAnnotationNamespaces.Contains(x.Namespace)))
            {
                return ($"{mappingNs}.Table", $"{mappingNs}.Column", $"{mappingNs}.Association");
            }
            else
            {
                codeModel.AddNamespaceImport(mappingNs);
                return ("Table", "Column", "Association");
            }
        }

        private void AddColumnAttributes(ColumnCodeModel col, string columnAttributeName)
        {
            var hasNameDiff = !string.Equals(col.DbObject.Name, col.PropertyName, StringComparison.OrdinalIgnoreCase);

            var columnAttr = DotNetGenerator.AttributeFactory.Create(columnAttributeName);

            if (hasNameDiff)
            {
                columnAttr = columnAttr.SetOptionalArg("Name", col.DbObject.Name, true);
            }

            if (col.DbObject.IsPrimaryKey)
            {
                columnAttr = columnAttr
                    .SetOptionalArg("IsPrimaryKey", true)
                    .SetOptionalArg("PrimaryKeyOrder", col.DbObject.PrimaryKeyPosition);

                if (!col.DbObject.IsIdentity && TypeMapper.DataTypeIsInteger(col.DbObject))
                {
                    columnAttr = columnAttr.SetOptionalArg("IsIdentity", false);
                }
            }

            if (col.DbObject.IsIdentity)
            {
                columnAttr = columnAttr.SetOptionalArg("IsIdentity", true);
            }

            var dbType = TypeMapper.GetFullColumnDataType(col.DbObject);
            if (!string.IsNullOrWhiteSpace(dbType))
            {
                columnAttr = columnAttr.SetOptionalArg("DbType", dbType, true);
            }

            columnAttr = columnAttr.SetOptionalArg("DataType", GetLinqToDbColumnDataType(col.DbObject));

            if (TypeMapper.DataTypeIsString(col.DbObject))
            {
                columnAttr = columnAttr
                    .SetOptionalArg("Length", col.DbObject.Length)
                    .SetOptionalArg("CanBeNull", col.DbObject.IsNullable);
            }
            else if (TypeMapper.DataTypeIsDecimal(col.DbObject))
            {
                columnAttr = columnAttr
                    .SetOptionalArg("Precision", col.DbObject.Precision)
                    .SetOptionalArg("Scale", col.DbObject.Scale);
            }
            else if (TypeMapper.DataTypeIsDateTimeWithScale(col.DbObject))
            {
                columnAttr = columnAttr.SetOptionalArg("Scale", col.DbObject.Scale);
            }

            if (col.DbObject.IsComputed || TypeMapper.DataTypeIsRowVersion(col.DbObject))
            {
                columnAttr = columnAttr
                    .SetOptionalArg("SkipOnInsert", true)
                    .SetOptionalArg("SkipOnUpdate", true);
            }

            col.AddAttribute(columnAttr);
        }

        public virtual string GetLinqToDbColumnDataType(Column column)
        {
            return TypeMapper.GetLinqToDbColumnDataType(column.DataType);
        }
    }
}
