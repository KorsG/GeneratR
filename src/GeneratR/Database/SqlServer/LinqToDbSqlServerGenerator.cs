using System;
using System.Collections.Generic;
using System.Linq;
using GeneratR.Database.SqlServer.Schema;
using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class LinqToDbSqlServerGenerator : SqlServerSchemaGenerator
    {
        private readonly LinqToDbSqlServerGeneratorSettings _settings;

        public LinqToDbSqlServerGenerator(LinqToDbSqlServerGeneratorSettings settings, DotNetLanguageType dotNetLanguage = DotNetLanguageType.CS)
           : base(settings, dotNetLanguage)
        {
            _settings = settings;
        }

        public override SqlServerSchemaCodeModels LoadCodeModels()
        {
            var schema = base.LoadCodeModels();
            EnrichCodeModel(schema);
            return schema;
        }

        public override IEnumerable<SourceCodeFile> GenerateCodeFiles(SqlServerSchemaCodeModels schemaModels)
        {
            var baseFiles = base.GenerateCodeFiles(schemaModels);

            if (!_settings.DataConnection.Generate)
            {
                return baseFiles;
            }

            var codeModel = BuildDataConnectionCodeModel(schemaModels);

            var codeFile = new SourceCodeFile()
            {
                FileName = $"{_settings.DataConnection.ClassName}.generated{DotNetGenerator.FileExtension}",
                FolderPath = BuildObjectOutputFolderPath(_settings.DataConnection.OutputFolderPath),
                Code = GenerateDataConnectionCode(codeModel),
            };

            return baseFiles.Concat(new[] { codeFile });
        }

        public override void WriteCodeFiles(IEnumerable<SourceCodeFile> codeFiles)
        {
            base.WriteCodeFiles(codeFiles);
        }

        protected virtual LinqToDbDataConnectionCodeModel BuildDataConnectionCodeModel(SqlServerSchemaCodeModels schemaModels)
        {
            var model = new LinqToDbDataConnectionCodeModel(DotNetGenerator, schemaModels)
            {
                ClassName = _settings.DataConnection.ClassName,
                Namespace = BuildObjectNamespace(_settings.DataConnection.Namespace, _settings.DataConnection.ClassName),
                InheritClassName = _settings.DataConnection.InheritClass,
                ImplementInterfaces = _settings.DataConnection.ImplementInterfaces ?? new List<string>(),
                AddConstructor = _settings.DataConnection.AddConstructor,
                DotNetModifier = _settings.DataConnection.Modifiers,
            };

            return model;
        }

        public Func<LinqToDbDataConnectionCodeModel, string> GenerateDataConnectionCodeFunc { get; set; } = null;

        public virtual string GenerateDataConnectionCode(LinqToDbDataConnectionCodeModel model)
        {
            var code = GenerateDataConnectionCodeFunc?.Invoke(model);
            if (code == null)
            {
                code = new LinqToDbDataConnectionTemplate(model).Generate();
            }
            return code;
        }

        private void EnrichCodeModel(SqlServerSchemaCodeModels model)
        {
            if (_settings.AddLinqToDbMappingAttributes)
            {
                foreach (var t in model.Tables)
                {
                    t.AddAttribute(DotNetGenerator.AttributeFactory.Create("LinqToDB.Mapping.Table")
                        .SetOptionalArg("Name", t.DbObject.Name, true)
                        .SetOptionalArg("Schema", t.DbObject.Schema, true)
                        );

                    foreach (var col in t.Columns)
                    {
                        AddColumnAttributes(col);
                    }

                    foreach (var fk in t.ForeignKeys)
                    {
                        var attr = DotNetGenerator.AttributeFactory.Create("LinqToDB.Mapping.Association")
                            .SetOptionalArg("ThisKey", string.Join(",", fk.DbObject.FromColumns.Select(c => c.ColumnName)), wrapValueInQuotes: true)
                            .SetOptionalArg("OtherKey", string.Join(",", fk.DbObject.ToColumns.Select(c => c.ColumnName)), wrapValueInQuotes: true)
                            .SetOptionalArg("CanBeNull", fk.DbObject.IsOptional)
                            .SetOptionalArg("KeyName", fk.DbObject.ForeignKeyName, wrapValueInQuotes: true);

                        if (fk.DbObject.RelationshipType == ForeignKeyRelationshipType.OneToOne)
                        {
                            attr = attr.SetOptionalArg("Relationship", "LinqToDB.Mapping.Relationship.OneToOne");
                        }
                        else if (fk.DbObject.RelationshipType == ForeignKeyRelationshipType.OneToMany)
                        {
                            attr = attr.SetOptionalArg("Relationship", "LinqToDB.Mapping.Relationship.OneToMany");
                        }

                        fk.Attributes.Add(attr);
                    }

                    foreach (var fk in t.ReferencingForeignKeys)
                    {
                        var attr = DotNetGenerator.AttributeFactory.Create("LinqToDB.Mapping.Association")
                            .SetOptionalArg("ThisKey", string.Join(",", fk.DbObject.ToColumns.Select(c => c.ColumnName)), wrapValueInQuotes: true)
                            .SetOptionalArg("OtherKey", string.Join(",", fk.DbObject.FromColumns.Select(c => c.ColumnName)), wrapValueInQuotes: true)
                            .SetOptionalArg("CanBeNull", fk.DbObject.IsOptional)
                            .SetOptionalArg("KeyName", fk.DbObject.ForeignKeyName, wrapValueInQuotes: true);

                        if (fk.DbObject.RelationshipType == ForeignKeyRelationshipType.OneToOne)
                        {
                            attr = attr.SetOptionalArg("Relationship", "LinqToDB.Mapping.Relationship.OneToOne");
                        }
                        else if (fk.DbObject.RelationshipType == ForeignKeyRelationshipType.OneToMany)
                        {
                            attr = attr.SetOptionalArg("Relationship", "LinqToDB.Mapping.Relationship.ManyToOne");
                        }

                        fk.Attributes.Add(attr);
                    }
                }

                foreach (var t in model.Views)
                {
                    t.AddAttribute(DotNetGenerator.AttributeFactory.Create("LinqToDB.Mapping.Table")
                        .SetOptionalArg("Name", t.DbObject.Name, true)
                        .SetOptionalArg("Schema", t.DbObject.Schema, true)
                        .SetOptionalArg("IsView", true)
                        );

                    foreach (var col in t.Columns)
                    {
                        AddColumnAttributes(col);
                    }
                }

                foreach (var t in model.TableFunctions)
                {
                    t.AddAttribute(DotNetGenerator.AttributeFactory.Create("LinqToDB.Mapping.Table")
                        .SetOptionalArg("Name", t.DbObject.Name, true)
                        .SetOptionalArg("Schema", t.DbObject.Schema, true)
                        );

                    foreach (var col in t.Columns)
                    {
                        AddColumnAttributes(col);
                    }
                }

                foreach (var t in model.TableTypes)
                {
                    t.AddAttribute(DotNetGenerator.AttributeFactory.Create("LinqToDB.Mapping.Table")
                        .SetOptionalArg("Name", t.DbObject.Name, true)
                        .SetOptionalArg("Schema", t.DbObject.Schema, true)
                        );

                    foreach (var col in t.Columns)
                    {
                        AddColumnAttributes(col);
                    }
                }
            }
        }

        private void AddColumnAttributes(ColumnCodeModel col)
        {
            var hasNameDiff = !string.Equals(col.DbObject.Name, col.PropertyName, StringComparison.OrdinalIgnoreCase);

            var columnAttr = DotNetGenerator.AttributeFactory.Create("LinqToDB.Mapping.Column");

            if (hasNameDiff)
            {
                columnAttr = columnAttr.SetOptionalArg("Name", col.DbObject.Name, true);
            }

            if (col.DbObject.IsPrimaryKey)
            {
                columnAttr = columnAttr
                    .SetOptionalArg("IsPrimaryKey", true)
                    .SetOptionalArg("PrimaryKeyOrder", col.DbObject.PrimaryKeyPosition);
            }

            if (col.DbObject.IsIdentity)
            {
                columnAttr = columnAttr.SetOptionalArg("IsIdentity", true);
            }

            columnAttr = columnAttr.SetOptionalArg("DataType", GetLinqToDbColumnDataType(col.DbObject));

            var dbType = TypeMapper.GetFullColumnDataType(col.DbObject);
            if (!string.IsNullOrWhiteSpace(dbType))
            {
                columnAttr = columnAttr.SetOptionalArg("DbType", dbType, true);
            }

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
            else if (TypeMapper.DataTypeIsDateTime(col.DbObject))
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
            var prefix = "LinqToDB.DataType.";

            switch (column.DataType.ToLowerInvariant())
            {
                case "image": return prefix + "Image";
                case "text": return prefix + "Text";
                case "binary": return prefix + "Binary";
                case "tinyint": return prefix + "Byte";
                case "date": return prefix + "Date";
                case "time": return prefix + "Time";
                case "bit": return prefix + "Boolean";
                case "smallint": return prefix + "Int16";
                case "decimal": return prefix + "Decimal";
                case "int": return prefix + "Int32";
                case "smalldatetime": return prefix + "SmallDateTime";
                case "real": return prefix + "Single";
                case "money": return prefix + "Money";
                case "datetime": return prefix + "DateTime";
                case "float": return prefix + "Double";
                case "numeric": return prefix + "Decimal";
                case "smallmoney": return prefix + "SmallMoney";
                case "datetime2": return prefix + "DateTime2";
                case "bigint": return prefix + "Int64";
                case "varbinary": return prefix + "VarBinary";
                case "timestamp": return prefix + "Timestamp";
                case "sysname": return prefix + "NVarChar";
                case "nvarchar": return prefix + "NVarChar";
                case "varchar": return prefix + "VarChar";
                case "ntext": return prefix + "NText";
                case "uniqueidentifier": return prefix + "Guid";
                case "datetimeoffset": return prefix + "DateTimeOffset";
                case "sql_variant": return prefix + "Variant";
                case "xml": return prefix + "Xml";
                case "char": return prefix + "Char";
                case "nchar": return prefix + "NChar";
                case "hierarchyid":
                case "geography":
                case "geometry": return prefix + "Udt";
                case "table type": return prefix + "Structured";
            }

            return prefix + "Undefined";
        }
    }
}
