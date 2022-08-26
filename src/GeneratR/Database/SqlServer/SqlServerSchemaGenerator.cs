using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GeneratR.Database.SqlServer.Schema;
using GeneratR.Database.SqlServer.Templates;
using GeneratR.DotNet;
using GeneratR.ExtensionMethods;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerSchemaGenerator
    {
        protected static Regex RemoveRelationalColumnSuffixRegex = new Regex(@"(Id|No|Key|Code)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly SqlConnectionStringBuilder _connectionStringBuilder;
        protected readonly SqlServerSchemaContext _schemaContext;

        public SqlServerSchemaGenerator(SqlServerSchemaGenerationSettings settings, DotNetLanguageType dotNetLanguage = DotNetLanguageType.CS)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            DotNetGenerator = DotNetGenerator.Create(dotNetLanguage);
            TypeMapper = new SqlServerTypeMapper(DotNetGenerator);
            _connectionStringBuilder = new SqlConnectionStringBuilder(Settings.ConnectionString);
            if (!string.IsNullOrWhiteSpace(settings.DatabaseName))
            {
                _connectionStringBuilder.InitialCatalog = settings.DatabaseName;
            }

            DatabaseName = _connectionStringBuilder.InitialCatalog;

            _schemaContext = new SqlServerSchemaContext(_connectionStringBuilder.ConnectionString)
            {
                IncludeSchemas = Settings.IncludeSchemas ?? new HashSet<string>(),
                ExcludeSchemas = Settings.ExcludeSchemas ?? new HashSet<string>(),
            };
        }

        public DotNetGenerator DotNetGenerator { get; }

        public virtual SqlServerTypeMapper TypeMapper { get; }

        public virtual SqlServerSchemaGenerationSettings Settings { get; }

        public string DatabaseName { get; }

        /// <summary>
        /// Execute generator steps: 1. LoadSchema -> 2. BuildCodeModels -> 3. GenerateSourceCode -> 4. WriteCodeFiles
        /// </summary>
        public virtual void Execute()
        {
            var schema = LoadSchema();
            var model = BuildCodeModel(schema);
            var files = GenerateCodeFiles(model);
            WriteCodeFiles(files);
        }

        public virtual SqlServerSchema LoadSchema()
        {
            var schema = _schemaContext.GetSchema(
                includeTables: Settings.Table.Generate,
                includeViews: Settings.View.Generate,
                includeTableFunctions: Settings.TableFunction.Generate,
                includeScalarFunctions: Settings.ScalarFunction.Generate,
                includeStoredProcedures: Settings.StoredProcedure.Generate,
                includeStoredProcedureResultColumns: Settings.StoredProcedure.GenerateResultSet,
                includeTableTypes: Settings.TableType.Generate
                );

            OnSchemaLoadedFunc?.Invoke(schema);

            return schema;
        }

        public virtual SqlServerSchemaCodeModels BuildCodeModel(SqlServerSchema schema)
        {
            var codeModels = new SqlServerSchemaCodeModels();

            // Tables.
            if (schema.Tables.Any())
            {
                codeModels.Tables.AddRange(BuildTables(schema.Tables));
            }

            // Views.
            if (schema.Views.Any())
            {
                codeModels.Views.AddRange(BuildViews(schema.Views));
            }

            // TableFunctions.
            if (schema.TableFunctions.Any())
            {
                codeModels.TableFunctions.AddRange(BuildTableFunctions(schema.TableFunctions));
            }

            // StoredProcedures.
            if (schema.StoredProcedures.Any())
            {
                codeModels.StoredProcedures.AddRange(BuildStoredProcedures(schema.StoredProcedures));
            }

            // TableTypes.
            if (schema.TableTypes.Any())
            {
                codeModels.TableTypes.AddRange(BuildTableTypes(schema.TableTypes));
            }

            OnCodeModelsLoadedFunc?.Invoke(codeModels);

            return codeModels;
        }

        public virtual IEnumerable<SourceCodeFile> GenerateCodeFiles(SqlServerSchemaCodeModels codeModels)
        {
            var files = GenerateCodeFilesInternal(codeModels);

            if (OnCodeFilesGeneratedFunc != null)
            {
                var context = new CodeFilesGeneratedContext(this, codeModels, files.ToList());
                OnCodeFilesGeneratedFunc.Invoke(context);
                return context.CodeFiles;
            }

            return files;
        }

        private IEnumerable<SourceCodeFile> GenerateCodeFilesInternal(SqlServerSchemaCodeModels codeModels)
        {
            // TODO: FileName generation should be configurable/overrideable (func?)

            foreach (var obj in codeModels.Tables)
            {
                yield return new SourceCodeFile()
                {
                    FileName = $"{obj.ClassName}.generated{DotNetGenerator.FileExtension}",
                    FolderPath = obj.OutputFolderPath,
                    Code = GenerateTableCode(obj),
                };
            }

            foreach (var obj in codeModels.Views)
            {
                yield return new SourceCodeFile()
                {
                    FileName = $"{obj.ClassName}.generated{DotNetGenerator.FileExtension}",
                    FolderPath = obj.OutputFolderPath,
                    Code = GenerateViewCode(obj),
                };
            }

            foreach (var obj in codeModels.TableFunctions)
            {
                yield return new SourceCodeFile()
                {
                    FileName = $"{obj.ClassName}.generated{DotNetGenerator.FileExtension}",
                    FolderPath = obj.OutputFolderPath,
                    Code = GenerateTableFunctionCode(obj),
                };
            }

            foreach (var obj in codeModels.TableTypes)
            {
                yield return new SourceCodeFile()
                {
                    FileName = $"{obj.ClassName}.generated{DotNetGenerator.FileExtension}",
                    FolderPath = obj.OutputFolderPath,
                    Code = GenerateTableTypeCode(obj),
                };
            }

            foreach (var obj in codeModels.StoredProcedures)
            {
                yield return new SourceCodeFile()
                {
                    FileName = $"{obj.ClassName}.generated{DotNetGenerator.FileExtension}",
                    FolderPath = obj.OutputFolderPath,
                    Code = GenerateStoredProcedureCode(obj),
                };
            }
        }

        public virtual void WriteCodeFiles(IEnumerable<SourceCodeFile> codeFiles)
        {
            foreach (var cf in codeFiles)
            {
                var outputFolderPath = cf.FolderPath;
                var outputFilePath = Path.Combine(outputFolderPath, cf.FileName);

                // TODO: Optimize folder creation by finding all folders first and grouping etc.,
                // or cache which folder have been checked/created.
                if (!Directory.Exists(outputFolderPath))
                {
                    Directory.CreateDirectory(outputFolderPath);
                }

                File.WriteAllText(outputFilePath, cf.Code);
            }
        }

        public Action<SqlServerSchema> OnSchemaLoadedFunc { get; set; } = null;

        public Action<SqlServerSchemaCodeModels> OnCodeModelsLoadedFunc { get; set; } = null;

        public Action<CodeFilesGeneratedContext> OnCodeFilesGeneratedFunc { get; set; } = null;

        public class CodeFilesGeneratedContext
        {
            public CodeFilesGeneratedContext(SqlServerSchemaGenerator generator, SqlServerSchemaCodeModels codeModels, List<SourceCodeFile> codeFiles)
            {
                Generator = generator;
                CodeModels = codeModels;
                CodeFiles = codeFiles;
            }

            public SqlServerSchemaGenerator Generator { get; }
            public SqlServerSchemaCodeModels CodeModels { get; }
            public List<SourceCodeFile> CodeFiles { get; }
        }

        #region GenerateCode functions

        public Func<TableCodeModel, string> GenerateTableCodeFunc { get; set; } = null;

        protected virtual string GenerateTableCode(TableCodeModel model)
        {
            var code = GenerateTableCodeFunc?.Invoke(model);
            if (code == null)
            {
                code = new TableTemplate(model).Generate();
            }
            return code;
        }

        public Func<ViewCodeModel, string> GenerateViewCodeFunc { get; set; } = null;

        protected virtual string GenerateViewCode(ViewCodeModel model)
        {
            var code = GenerateViewCodeFunc?.Invoke(model);
            if (code == null)
            {
                code = new ViewTemplate(model).Generate();
            }
            return code;
        }

        public Func<TableFunctionCodeModel, string> GenerateTableFunctionCodeFunc { get; set; } = null;

        protected virtual string GenerateTableFunctionCode(TableFunctionCodeModel model)
        {
            var code = GenerateTableFunctionCodeFunc?.Invoke(model);
            if (code == null)
            {
                code = new TableFunctionTemplate(model).Generate();
            }
            return code;
        }

        public Func<TableTypeCodeModel, string> GenerateTableTypeCodeFunc { get; set; } = null;

        protected virtual string GenerateTableTypeCode(TableTypeCodeModel model)
        {
            var code = GenerateTableTypeCodeFunc?.Invoke(model);
            if (code == null)
            {
                code = new TableTypeTemplate(model).Generate();
            }
            return code;
        }

        public Func<StoredProcedureCodeModel, string> GenerateStoredProcedureCodeFunc { get; set; } = null;

        protected virtual string GenerateStoredProcedureCode(StoredProcedureCodeModel model)
        {
            var code = GenerateStoredProcedureCodeFunc?.Invoke(model);
            if (code == null)
            {
                code = new StoredProcedureTemplate(model).Generate();
            }
            return code;
        }

        #endregion

        #region Apply/Get settings

        /// <summary>Apply additional per object settings.</summary>
        public Action<TableSettings, Table> ApplyTableSettings { get; set; } = null;

        protected virtual TableSettings GetTableSettings(Table obj)
        {
            var objSettings = Settings.Table;
            if (ApplyTableSettings != null)
            {
                objSettings = objSettings.Clone();
                ApplyTableSettings(objSettings, obj);
            }
            return objSettings;
        }

        /// <summary>Apply additional per object settings.</summary>
        public Action<ViewSettings, View> ApplyViewSettings { get; set; } = null;

        protected virtual ViewSettings GetViewSettings(View obj)
        {
            var objSettings = Settings.View;
            if (ApplyViewSettings != null)
            {
                objSettings = objSettings.Clone();
                ApplyViewSettings(objSettings, obj);
            }
            return objSettings;
        }

        /// <summary>Apply additional per object settings.</summary>
        public Action<TableFunctionSettings, TableFunction> ApplyTableFunctionSettings { get; set; } = null;

        protected virtual TableFunctionSettings GetTableFunctionSettings(TableFunction obj)
        {
            var objSettings = Settings.TableFunction;
            if (ApplyTableFunctionSettings != null)
            {
                objSettings = objSettings.Clone();
                ApplyTableFunctionSettings(objSettings, obj);
            }
            return objSettings;
        }

        /// <summary>Apply additional per object settings.</summary>
        public Action<TableTypeSettings, TableType> ApplyTableTypeSettings { get; set; } = null;

        protected virtual TableTypeSettings GetTableTypeSettings(TableType obj)
        {
            var objSettings = Settings.TableType;
            if (ApplyTableTypeSettings != null)
            {
                objSettings = objSettings.Clone();
                ApplyTableTypeSettings(objSettings, obj);
            }
            return objSettings;
        }

        /// <summary>Apply additional per object settings.</summary>
        public Action<StoredProcedureSettings, StoredProcedure> ApplyStoredProcedureSettings { get; set; } = null;

        protected virtual StoredProcedureSettings GetStoredProcedureSettings(StoredProcedure obj)
        {
            var objSettings = Settings.StoredProcedure;
            if (ApplyStoredProcedureSettings != null)
            {
                objSettings = objSettings.Clone();
                ApplyStoredProcedureSettings(objSettings, obj);
            }
            return objSettings;
        }

        #endregion

        #region Build functions

        protected virtual List<TableCodeModel> BuildTables(IEnumerable<Table> dbTypes)
        {
            var models = new List<TableCodeModel>();

            if (Settings.IncludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => Settings.IncludeObjects.Any(i => x.FullName.Like(i)));
            }

            if (Settings.ExcludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => !Settings.ExcludeObjects.Any(i => x.FullName.Like(i)));
            }

            foreach (var t in dbTypes)
            {
                var objSettings = GetTableSettings(t);
                if (!objSettings.Generate) { continue; }

                var o = new TableCodeModel(t, DotNetGenerator, TypeMapper)
                {
                    DotNetModifier = objSettings.Modifiers,
                    GenerateForeignKeys = objSettings.GenerateForeignKeys,
                    GenerateReferencingForeignKeys = objSettings.GenerateReferencingForeignKeys,
                    AddDataAnnotationAttributes = objSettings.AddDataAnnotationAttributes,
                    AddConstructor = objSettings.AddConstructor,
                    InheritClassName = objSettings.InheritClass,
                    ImplementInterfaces = objSettings.ImplementInterfaces != null ? new List<string>(objSettings.ImplementInterfaces) : new List<string>(),
                };

                o.ClassName = BuildObjectClassName(objSettings, t.Name);
                o.Namespace = BuildObjectNamespace(objSettings, o.ClassName, t.Schema);
                o.OutputFolderPath = BuildObjectOutputFolderPath(objSettings, o.ClassName, t.Schema);

                // Table columns.
                foreach (var col in t.Columns)
                {
                    var c = new ColumnCodeModel(col);

                    var propertyName = c.DbObject.Name;
                    if (string.Equals(propertyName, o.ClassName, StringComparison.OrdinalIgnoreCase))
                    {
                        propertyName += "Column";
                    }
                    c.PropertyName = DotNetGenerator.GetAsValidDotNetName(propertyName);
                    c.PropertyType = TypeMapper.ConvertDataTypeToDotNetType(c.DbObject.DataType, c.DbObject.IsNullable);
                    c.DotNetModifier = objSettings.ColumnModifiers;
                    o.Columns.Add(c);
                }

                // Table foreign keys.
                foreach (var fk in t.ForeignKeys)
                {
                    var f = new ForeignKeyCodeModel(fk)
                    {
                        DotNetModifier = objSettings.ForeignKeyModifiers,
                    };
                    o.ForeignKeys.Add(f);
                }

                foreach (var fk in t.ReferencingForeignKeys)
                {
                    var f = new ForeignKeyCodeModel(fk)
                    {
                        DotNetModifier = objSettings.ForeignKeyModifiers,
                    };
                    o.ReferencingForeignKeys.Add(f);
                }

                models.Add(o);
            }

            // Foreign key properties and relations must be handled after tables have been parsed, because it needs info from all tables in the collection to parse correctly.
            SetTableCollectionForeignKeyProperties(models);

            // Add after foreign key parsing.
            foreach (var o in models.Where(x => x.AddDataAnnotationAttributes))
            {
                AddTableDataAnnotationAttributes(o);
            }

            return models;
        }

        protected virtual List<ViewCodeModel> BuildViews(IEnumerable<View> dbTypes)
        {
            var configs = new List<ViewCodeModel>();

            if (Settings.IncludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => Settings.IncludeObjects.Any(i => x.FullName.Like(i)));
            }

            if (Settings.ExcludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => !Settings.ExcludeObjects.Any(i => x.FullName.Like(i)));
            }

            foreach (var t in dbTypes)
            {
                var objSettings = GetViewSettings(t);
                if (!objSettings.Generate) { continue; }

                var o = new ViewCodeModel(t, DotNetGenerator, TypeMapper)
                {
                    DotNetModifier = objSettings.Modifiers,
                    AddDataAnnotationAttributes = objSettings.AddDataAnnotationAttributes,
                    AddConstructor = objSettings.AddConstructor,
                    InheritClassName = objSettings.InheritClass,
                    ImplementInterfaces = objSettings.ImplementInterfaces != null ? new List<string>(objSettings.ImplementInterfaces) : new List<string>(),
                };

                if (objSettings.NamingStrategy == NamingStrategy.Pluralize)
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakePlural(t.Name));
                }
                else if (objSettings.NamingStrategy == NamingStrategy.Singularize)
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakeSingular(t.Name));
                }
                else
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(t.Name);
                }

                o.Namespace = BuildObjectNamespace(objSettings, o.ClassName, t.Schema);
                o.OutputFolderPath = BuildObjectOutputFolderPath(objSettings, o.ClassName, t.Schema);

                // View columns.
                foreach (var col in t.Columns)
                {
                    var c = new ColumnCodeModel(col);

                    var propertyName = c.DbObject.Name;
                    if (string.Equals(propertyName, o.ClassName, StringComparison.OrdinalIgnoreCase))
                    {
                        propertyName += "Column";
                    }
                    c.PropertyName = DotNetGenerator.GetAsValidDotNetName(propertyName);
                    c.PropertyType = TypeMapper.ConvertDataTypeToDotNetType(c.DbObject.DataType, c.DbObject.IsNullable);
                    c.DotNetModifier = objSettings.ColumnModifiers;
                    o.Columns.Add(c);
                }

                if (objSettings.AddDataAnnotationAttributes)
                {
                    AddViewDataAnnotationAttributes(o);
                }

                configs.Add(o);
            }

            return configs;
        }

        protected virtual List<TableTypeCodeModel> BuildTableTypes(IEnumerable<TableType> dbTypes)
        {
            var configs = new List<TableTypeCodeModel>();

            if (Settings.IncludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => Settings.IncludeObjects.Any(i => x.FullName.Like(i)));
            }

            if (Settings.ExcludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => !Settings.ExcludeObjects.Any(i => x.FullName.Like(i)));
            }

            foreach (var t in dbTypes)
            {
                var objSettings = GetTableTypeSettings(t);
                if (!objSettings.Generate) { continue; }

                var o = new TableTypeCodeModel(t, DotNetGenerator, TypeMapper)
                {
                    DotNetModifier = objSettings.Modifiers,
                    AddDataAnnotationAttributes = objSettings.AddDataAnnotationAttributes,
                    AddConstructor = objSettings.AddConstructor,
                    InheritClassName = objSettings.InheritClass,
                    AddSqlDataRecordMappings = objSettings.AddSqlDataRecordMappings,
                    ImplementInterfaces = objSettings.ImplementInterfaces != null ? new List<string>(objSettings.ImplementInterfaces) : new List<string>(),
                };

                o.ClassName = BuildObjectClassName(objSettings, t.Name);
                o.Namespace = BuildObjectNamespace(objSettings, o.ClassName, t.Schema);
                o.OutputFolderPath = BuildObjectOutputFolderPath(objSettings, o.ClassName, t.Schema);

                // Columns.
                foreach (var col in t.Columns)
                {
                    var c = new ColumnCodeModel(col);

                    var propertyName = c.DbObject.Name;
                    if (string.Equals(propertyName, o.ClassName, StringComparison.OrdinalIgnoreCase))
                    {
                        propertyName += "Column";
                    }
                    c.PropertyName = DotNetGenerator.GetAsValidDotNetName(propertyName);
                    c.PropertyType = TypeMapper.ConvertDataTypeToDotNetType(c.DbObject.DataType, c.DbObject.IsNullable);
                    c.DotNetModifier = objSettings.ColumnModifiers;
                    o.Columns.Add(c);
                }

                if (objSettings.AddDataAnnotationAttributes)
                {
                    AddTableTypeDataAnnotationAttributes(o);
                }

                configs.Add(o);
            }

            return configs;
        }

        protected virtual List<TableFunctionCodeModel> BuildTableFunctions(IEnumerable<TableFunction> dbTypes)
        {
            var configs = new List<TableFunctionCodeModel>();

            if (Settings.IncludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => Settings.IncludeObjects.Any(i => x.FullName.Like(i)));
            }

            if (Settings.ExcludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => !Settings.ExcludeObjects.Any(i => x.FullName.Like(i)));
            }

            foreach (var t in dbTypes)
            {
                var objSettings = GetTableFunctionSettings(t);
                if (!objSettings.Generate) { continue; }

                var o = new TableFunctionCodeModel(t, DotNetGenerator, TypeMapper)
                {
                    DotNetModifier = objSettings.Modifiers,
                    AddDataAnnotationAttributes = objSettings.AddDataAnnotationAttributes,
                    AddConstructor = objSettings.AddConstructor,
                    InheritClassName = objSettings.InheritClass,
                    ImplementInterfaces = objSettings.ImplementInterfaces != null ? new List<string>(objSettings.ImplementInterfaces) : new List<string>(),
                };

                o.ClassName = BuildObjectClassName(objSettings, t.Name);
                o.Namespace = BuildObjectNamespace(objSettings, o.ClassName, t.Schema);
                o.OutputFolderPath = BuildObjectOutputFolderPath(objSettings, o.ClassName, t.Schema);

                // Function columns.
                foreach (var col in t.Columns)
                {
                    var c = new ColumnCodeModel(col);

                    var propertyName = c.DbObject.Name;
                    if (string.Equals(propertyName, o.ClassName, StringComparison.OrdinalIgnoreCase))
                    {
                        propertyName += "Column"; // TODO: Make this configurable.
                    }
                    c.PropertyName = DotNetGenerator.GetAsValidDotNetName(propertyName);
                    c.PropertyType = TypeMapper.ConvertDataTypeToDotNetType(c.DbObject.DataType, c.DbObject.IsNullable);
                    c.DotNetModifier = Settings.TableFunction.ColumnModifiers;
                    o.Columns.Add(c);
                }

                // Function parameters.
                foreach (var param in t.Parameters)
                {
                    var p = new ParameterCodeModel(param);
                    p.PropertyName = DotNetGenerator.GetAsValidDotNetName(p.DbObject.Name);
                    p.PropertyType = TypeMapper.ConvertDbParameterToDotNetType(p.DbObject);
                    o.Parameters.Add(p);
                }

                if (objSettings.AddDataAnnotationAttributes)
                {
                    AddTableFunctionDataAnnotationAttributes(o);
                }

                configs.Add(o);
            }

            return configs;
        }

        protected virtual List<StoredProcedureCodeModel> BuildStoredProcedures(IEnumerable<StoredProcedure> dbTypes)
        {
            var configs = new List<StoredProcedureCodeModel>();

            if (Settings.IncludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => Settings.IncludeObjects.Any(i => x.FullName.Like(i)));
            }

            if (Settings.ExcludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => !Settings.ExcludeObjects.Any(i => x.FullName.Like(i)));
            }

            foreach (var t in dbTypes)
            {
                var objSettings = GetStoredProcedureSettings(t);
                if (!objSettings.Generate) { continue; }

                var o = new StoredProcedureCodeModel(t, DotNetGenerator, TypeMapper)
                {
                    DotNetModifier = objSettings.Modifiers,
                    AddDataAnnotationAttributes = objSettings.AddDataAnnotationAttributes,
                    AddConstructor = objSettings.AddConstructor,
                    InheritClassName = objSettings.InheritClass,
                    GenerateOutputParameters = objSettings.GenerateOutputParameters,
                    GenerateResultSet = objSettings.GenerateResultSet,
                    ImplementInterfaces = objSettings.ImplementInterfaces != null ? new List<string>(objSettings.ImplementInterfaces) : new List<string>(),
                };

                o.ClassName = BuildObjectClassName(objSettings, t.Name);
                o.Namespace = BuildObjectNamespace(objSettings, o.ClassName, t.Schema);
                o.OutputFolderPath = BuildObjectOutputFolderPath(objSettings, o.ClassName, t.Schema);

                // StoredProcedure ResultColumns.
                if (objSettings.GenerateResultSet)
                {
                    foreach (var colr in t.ResultColumns)
                    {
                        var c = new StoredProcedureResultColumnCodeModel(colr);
                        c.PropertyName = DotNetGenerator.GetAsValidDotNetName(c.DbObject.Name);
                        c.PropertyType = TypeMapper.ConvertDataTypeToDotNetType(c.DbObject.DataType, c.DbObject.IsNullable);
                        c.DotNetModifier = objSettings.ColumnModifiers;
                        o.ResultColumns.Add(c);
                    }
                }

                // StoredProcedure parameters
                foreach (var param in t.Parameters)
                {
                    var p = new ParameterCodeModel(param);
                    p.PropertyName = DotNetGenerator.GetAsValidDotNetName(p.DbObject.Name);
                    p.PropertyType = TypeMapper.ConvertDbParameterToDotNetType(p.DbObject);
                    o.Parameters.Add(p);
                }

                if (objSettings.AddDataAnnotationAttributes)
                {
                    AddStoredProcedureDataAnnotationAttributes(o);
                }

                configs.Add(o);
            }

            return configs;
        }

        #endregion

        #region AddDataAnnotations functions

        protected virtual void AddTableDataAnnotationAttributes(TableCodeModel config)
        {
            if (!config.DbObject.Name.Equals(config.ClassName, StringComparison.Ordinal) || !config.DbObject.Schema.Equals("dbo", StringComparison.Ordinal))
            {
                config.Attributes.Add(DotNetGenerator.AttributeFactory.CreateTableAttribute(config.DbObject.Name, config.DbObject.Schema));
            }

            foreach (var col in config.Columns)
            {
                AddColumnDataAnnotationAttributes(col);
            }
        }

        protected virtual void AddViewDataAnnotationAttributes(ViewCodeModel config)
        {
            if (!config.DbObject.Name.Equals(config.ClassName, StringComparison.Ordinal) || !config.DbObject.Schema.Equals("dbo", StringComparison.Ordinal))
            {
                config.Attributes.Add(DotNetGenerator.AttributeFactory.CreateTableAttribute(config.DbObject.Name, config.DbObject.Schema));
            }

            foreach (var col in config.Columns)
            {
                AddColumnDataAnnotationAttributes(col);
            }
        }

        protected virtual void AddTableFunctionDataAnnotationAttributes(TableFunctionCodeModel config)
        {
            if (!config.DbObject.Name.Equals(config.ClassName, StringComparison.Ordinal) || !config.DbObject.Schema.Equals("dbo", StringComparison.Ordinal))
            {
                config.Attributes.Add(DotNetGenerator.AttributeFactory.CreateTableAttribute(config.DbObject.Name, config.DbObject.Schema));
            }

            foreach (var col in config.Columns)
            {
                AddColumnDataAnnotationAttributes(col);
            }
        }

        protected virtual void AddTableTypeDataAnnotationAttributes(TableTypeCodeModel config)
        {
            if (!config.DbObject.Name.Equals(config.ClassName, StringComparison.Ordinal) || !config.DbObject.Schema.Equals("dbo", StringComparison.Ordinal))
            {
                config.Attributes.Add(DotNetGenerator.AttributeFactory.CreateTableAttribute(config.DbObject.Name, config.DbObject.Schema));
            }

            foreach (var col in config.Columns)
            {
                AddColumnDataAnnotationAttributes(col);
            }
        }

        protected virtual void AddStoredProcedureDataAnnotationAttributes(StoredProcedureCodeModel config)
        {
            if (!config.DbObject.Name.Equals(config.ClassName, StringComparison.Ordinal) || !config.DbObject.Schema.Equals("dbo", StringComparison.Ordinal))
            {
                config.Attributes.Add(DotNetGenerator.AttributeFactory.CreateTableAttribute(config.DbObject.Name, config.DbObject.Schema));
            }

            foreach (var x in config.Parameters)
            {
                AddStoredProcedureParameterDataAnnotationAttributes(x);
            }

            foreach (var x in config.ResultColumns)
            {
                AddStoredProcedureColumnDataAnnotationAttributes(x);
            }
        }

        protected virtual void AddColumnDataAnnotationAttributes(ColumnCodeModel config)
        {
            var col = config.DbObject;

            if (col.IsPrimaryKey)
            {
                var attr = DotNetGenerator.AttributeFactory.CreateKeyAttribute();
                config.Attributes.Add(attr);
            }

            var hasNameDiff = !string.Equals(col.Name, config.PropertyName, StringComparison.OrdinalIgnoreCase);

            // Column+Key.
            var columnAttributeTypeName = TypeMapper.GetFullColumnDataType(col);
            if (col.IsPrimaryKey && hasNameDiff)
            {
                var keyAttr = DotNetGenerator.AttributeFactory.CreateKeyAttribute();
                config.Attributes.Add(keyAttr);

                var attr = DotNetGenerator.AttributeFactory.CreateColumnAttribute(col.Name, col.Position - 1, typeName: columnAttributeTypeName);
                config.Attributes.Add(attr);
            }
            else if (col.IsPrimaryKey)
            {
                var keyAttr = DotNetGenerator.AttributeFactory.CreateKeyAttribute();
                config.Attributes.Add(keyAttr);

                var attr = DotNetGenerator.AttributeFactory.CreateColumnAttribute(col.Position - 1, typeName: columnAttributeTypeName);
                config.Attributes.Add(attr);
            }
            else if (hasNameDiff)
            {
                var attr = DotNetGenerator.AttributeFactory.CreateColumnAttribute(col.Name, typeName: columnAttributeTypeName);
                config.Attributes.Add(attr);
            }
            else if (!string.IsNullOrWhiteSpace(columnAttributeTypeName))
            {
                var attr = DotNetGenerator.AttributeFactory.CreateColumnAttribute(typeName: columnAttributeTypeName);
                config.Attributes.Add(attr);
            }

            // DatabaseGenerated (Computed/Identity).
            if (col.IsIdentity)
            {
                config.Attributes.Add(DotNetGenerator.AttributeFactory.CreateDatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity));
            }
            else if (col.IsComputed || TypeMapper.DataTypeIsRowVersion(col))
            {
                config.Attributes.Add(DotNetGenerator.AttributeFactory.CreateDatabaseGeneratedAttribute(DatabaseGeneratedOption.Computed));
            }
            else if (col.IsPrimaryKey)
            {
                // Explicitly set none when primary key is not autogenerated.
                config.Attributes.Add(DotNetGenerator.AttributeFactory.CreateDatabaseGeneratedAttribute(DatabaseGeneratedOption.None));
            }

            // StringLenght/MaxLength
            if (TypeMapper.DataTypeIsVariableStringLength(col))
            {
                DotNetAttribute attr;
                if (col.Length == -1)
                {
                    attr = DotNetGenerator.AttributeFactory.CreateMaxLengthAttribute();
                }
                else
                {
                    attr = DotNetGenerator.AttributeFactory.CreateStringLengthAttribute(col.Length);
                }
                config.Attributes.Add(attr);
            }
            else if (TypeMapper.DataTypeIsFixedStringLength(col))
            {
                var attr = DotNetGenerator.AttributeFactory.CreateStringLengthAttribute(col.Length, col.Length);
                config.Attributes.Add(attr);
            }

            // Required.
            if (!col.IsNullable && TypeMapper.DataTypeIsString(col))
            {
                var attr = DotNetGenerator.AttributeFactory.CreateRequiredAttribute();
                config.Attributes.Add(attr);
            }
        }

        protected virtual void AddStoredProcedureParameterDataAnnotationAttributes(ParameterCodeModel col)
        {
        }

        protected virtual void AddStoredProcedureColumnDataAnnotationAttributes(StoredProcedureResultColumnCodeModel col)
        {
            var hasNameDiff = !string.Equals(col.DbObject.Name, col.PropertyName, StringComparison.OrdinalIgnoreCase);
            if (hasNameDiff)
            {
                var attr = DotNetGenerator.AttributeFactory.CreateColumnAttribute(col.DbObject.Name);
                col.Attributes.Add(attr);
            }
        }

        #endregion

        protected virtual string RemoveRelationalColumnSuffix(string columnName)
        {
            return RemoveRelationalColumnSuffixRegex.Replace(columnName, string.Empty);
        }

        private void SetTableCollectionForeignKeyProperties(IEnumerable<TableCodeModel> tables)
        {
            foreach (var t in tables)
            {
                // TODO: Find a way to reuse possible per-object settings. Maybe set ForeignKeyCollectionType and ForeignKeyNamingStrategy on the table config?
                var objSettings = GetTableSettings(t.DbObject);

                var foreignKeyCollectionType = objSettings.ForeignKeyCollectionType;
                var foreignKeyNamingStrategy = objSettings.ForeignKeyNamingStrategy;

                // Remove all foreign keys from the collection if the table they reference to/from does not exist in the provided table collection.
                var nonExistingFkRelations = new List<ForeignKeyCodeModel>();
                var nonExistingReferencingFkRelations = new List<ForeignKeyCodeModel>();
                foreach (var fk in t.ForeignKeys)
                {
                    if (!tables.Where(x => x.DbObject.ObjectID == fk.DbObject.ToObjectID).Any())
                    {
                        nonExistingFkRelations.Add(fk);
                    }
                }
                foreach (var fk in t.ReferencingForeignKeys)
                {
                    if (!tables.Where(x => x.DbObject.ObjectID == fk.DbObject.FromObjectID).Any())
                    {
                        nonExistingReferencingFkRelations.Add(fk);
                    }
                }
                t.ForeignKeys.RemoveAll(q => nonExistingFkRelations.Contains(q));
                t.ReferencingForeignKeys.RemoveAll(q => nonExistingReferencingFkRelations.Contains(q));

                if (foreignKeyNamingStrategy == ForeignKeyNamingStrategy.ForeignKeyName)
                {
                    foreach (var fk in t.ForeignKeys)
                    {
                        fk.PropertyName = fk.DbObject.ForeignKeyName;
                        fk.PropertyType = tables.Where(x => x.DbObject.ObjectID == fk.DbObject.ToObjectID).Single().ClassName;
                    }
                    foreach (var fk in t.ReferencingForeignKeys)
                    {
                        fk.PropertyName = fk.DbObject.ForeignKeyName;
                        fk.PropertyType = tables.Where(x => x.DbObject.ObjectID == fk.DbObject.FromObjectID).Single().ClassName;
                    }
                }
                else if (foreignKeyNamingStrategy == ForeignKeyNamingStrategy.ReferencingTableName)
                {
                    foreach (var fk in t.ForeignKeys)
                    {
                        fk.PropertyName = DotNetGenerator.GetAsValidDotNetName(fk.DbObject.ToName);
                        if (fk.DbObject.IsSelfReferencing)
                        {
                            fk.PropertyName = DotNetGenerator.GetAsValidDotNetName(ParseSelfReferencingForeignKeyName(fk.DbObject));
                        }
                        fk.PropertyType = tables.Where(x => x.DbObject.ObjectID == fk.DbObject.ToObjectID).Single().ClassName;
                    }

                    foreach (var fk in t.ReferencingForeignKeys)
                    {
                        if (fk.DbObject.RelationshipType == ForeignKeyRelationshipType.OneToOne)
                        {
                            fk.PropertyName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakeSingular(fk.DbObject.FromName));
                            fk.PropertyType = tables.Where(x => x.DbObject.ObjectID == fk.DbObject.FromObjectID).Single().ClassName;
                        }
                        else
                        {
                            fk.PropertyName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakePlural(fk.DbObject.FromName));
                            fk.PropertyType = CreateForeignKeyCollectionTypeDotNetString(foreignKeyCollectionType, tables.Where(x => x.DbObject.ObjectID == fk.DbObject.FromObjectID).Single().ClassName);
                        }
                    }

                    // Handle relational properties with same name.
                    var multiples = t.ForeignKeys.GroupBy(x => x.PropertyName).Where(x => x.Count() > 1).SelectMany(x => x).ToList();
                    multiples.AddRange(t.ReferencingForeignKeys.GroupBy(x => x.PropertyName).Where(x => x.Count() > 1).SelectMany(x => x).ToList());
                    foreach (var nm in multiples.Select(x => x.PropertyName).Distinct().ToList())
                    {
                        var counter = 1;
                        foreach (var fk in multiples.Where(x => x.PropertyName == nm))
                        {
                            fk.PropertyName += counter.ToString();
                            counter++;
                        }
                    }

                }
                else if (foreignKeyNamingStrategy == ForeignKeyNamingStrategy.Intelligent)
                {
                    foreach (var tblName in t.ForeignKeys.Select(x => x.DbObject.FromName).Distinct().ToList())
                    {
                        var fks = t.ForeignKeys.Where(x => x.DbObject.FromName == tblName).ToList();

                        // Handle scenarios where this table only have one fk to a table.
                        var singleRelations = fks.GroupBy(x => x.DbObject.ToName).Where(x => x.Count() == 1).SelectMany(x => x).ToList();
                        foreach (var fk in singleRelations)
                        {
                            fk.PropertyName = fk.DbObject.ToName;
                            if (fk.DbObject.IsSelfReferencing)
                            {
                                fk.PropertyName = ParseSelfReferencingForeignKeyName(fk.DbObject);
                            }
                            fk.PropertyType = tables.Where(x => x.DbObject.ObjectID == fk.DbObject.ToObjectID).Single().ClassName;
                        }

                        // Handle scenarios where this table have multiple fk's to a table.
                        var multiRelations = fks.GroupBy(x => x.DbObject.ToName).Where(x => x.Count() > 1).SelectMany(x => x).ToList();
                        foreach (var fk in multiRelations)
                        {
                            var propName = "";
                            foreach (var col in fk.DbObject.FromColumns)
                            {
                                // TODO: Handle selfreferencing
                                // Remove ID/No/Key/Code etc. suffixes.
                                var nm = RemoveRelationalColumnSuffix(col.ColumnName);
                                propName += nm;
                            }
                            fk.PropertyName = propName;
                            fk.PropertyType = tables.Where(x => x.DbObject.ObjectID == fk.DbObject.ToObjectID).Single().ClassName;
                        }

                        // Clean names.
                        foreach (var fk in fks)
                        {
                            fk.PropertyName = this.DotNetGenerator.GetAsValidDotNetName(fk.PropertyName);
                        }

                    }
                    // Referencing/Reverse.
                    foreach (var tblName in t.ReferencingForeignKeys.Select(x => x.DbObject.ToName).Distinct().ToList())
                    {
                        var fks = t.ReferencingForeignKeys.Where(x => x.DbObject.ToName == tblName).ToList();

                        // Handle scenarios where this table only have one fk to a table.
                        var singleRelations = fks.GroupBy(x => x.DbObject.FromName).Where(x => x.Count() == 1).SelectMany(x => x).ToList();
                        foreach (var fk in singleRelations)
                        {
                            fk.PropertyName = fk.DbObject.FromName;
                            fk.PropertyType = tables.Where(x => x.DbObject.ObjectID == fk.DbObject.FromObjectID).Single().ClassName;
                        }

                        // Handle scenarios where this table have multiple fk's to a table.
                        var multiRelations = fks.GroupBy(x => x.DbObject.FromName).Where(x => x.Count() > 1).SelectMany(x => x).ToList();
                        foreach (var fk in multiRelations)
                        {
                            var propName = "";
                            foreach (var col in fk.DbObject.FromColumns)
                            {
                                // Remove ID/No/Key/Code etc. suffixes.
                                var nm = RemoveRelationalColumnSuffix(col.ColumnName);
                                propName += nm;
                            }
                            fk.PropertyName = propName;
                            fk.PropertyType = tables.Where(x => x.DbObject.ObjectID == fk.DbObject.FromObjectID).Single().ClassName;
                        }

                        // Handle PropertyName/Type depending on type of relationship + clean names.
                        foreach (var fk in fks)
                        {
                            if (fk.DbObject.RelationshipType == ForeignKeyRelationshipType.OneToOne)
                            {
                                fk.PropertyName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakeSingular(fk.PropertyName));
                            }
                            else
                            {
                                fk.PropertyName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakePlural(fk.PropertyName));
                                fk.PropertyType = CreateForeignKeyCollectionTypeDotNetString(foreignKeyCollectionType, tables.Where(x => x.DbObject.ObjectID == fk.DbObject.FromObjectID).Single().ClassName);
                            }
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        private string ParseSelfReferencingForeignKeyName(ForeignKey fk)
        {
            var parsed = string.Concat(fk.FromColumns.Select(x => RemoveRelationalColumnSuffix(x.ColumnName)));
            parsed += fk.ToName;
            return parsed;
        }

        protected virtual string CreateForeignKeyCollectionTypeDotNetString(ForeignKeyCollectionType collectionType, string elementType)
        {
            // TODO: Create vb compatible method.
            switch (collectionType)
            {
                case ForeignKeyCollectionType.IEnumerable:
                    return string.Format("IEnumerable<{0}>", elementType);
                case ForeignKeyCollectionType.ICollection:
                    return string.Format("ICollection<{0}>", elementType);
                case ForeignKeyCollectionType.IList:
                    return string.Format("IList<{0}>", elementType);
                case ForeignKeyCollectionType.List:
                    return string.Format("List<{0}>", elementType);
                default:
                    throw new NotImplementedException();
            }
        }

        protected string BuildObjectClassName(CodeModelSettingsBase objectSettings, string objectName)
        {
            if (objectSettings.NamingStrategy == NamingStrategy.Pluralize)
            {
                return DotNetGenerator.GetAsValidDotNetName(Inflector.MakePlural(objectName));
            }
            else if (objectSettings.NamingStrategy == NamingStrategy.Singularize)
            {
                return DotNetGenerator.GetAsValidDotNetName(Inflector.MakeSingular(objectName));
            }
            else
            {
                return DotNetGenerator.GetAsValidDotNetName(objectName);
            }
        }

        /// <summary>
        /// Build the namespace of an object which will combine it with the RootNamespace and other formatting.
        /// </summary>
        public virtual string BuildObjectNamespace(CodeModelSettingsBase objectSettings, string className = null, string schema = null)
            => BuildObjectNamespace(objectSettings.Namespace, className, schema);

        protected string BuildObjectNamespace(string objNamespace, string className = null, string schema = null)
        {
            var ns = Settings.RootNamespace?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(objNamespace))
            {
                if (!objNamespace.StartsWith(".", StringComparison.OrdinalIgnoreCase))
                {
                    ns += ".";
                }
                ns += objNamespace.Trim();
            }

            if (className != null)
            {
                ns = ns.Replace("{object}", className);
            }

            if (schema != null)
            {
                ns = ns.Replace("{schema}", schema);
            }

            return ns;
        }

        /// <summary>
        /// Build the output folder path of an object, which will combine it with the RootOutputFolderPath and other formatting.
        /// </summary>
        public virtual string BuildObjectOutputFolderPath(CodeModelSettingsBase objectSettings, string className = null, string schema = null)
            => BuildObjectOutputFolderPath(objectSettings.OutputFolderPath, className, schema);

        private string BuildObjectOutputFolderPath(string outputFolderPath, string className = null, string schema = null)
        {
            // TODO: Consider supporting "rooted/absoluted" objSettings.OutputFolderPath
            var path = Settings.RootOutputFolderPath?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(outputFolderPath))
            {
                path = Path.Combine(path, outputFolderPath);
            }

            if (className != null)
            {
                path = path.Replace("{object}", className);
            }

            if (schema != null)
            {
                path = path.Replace("{schema}", schema);
            }

            return path;
        }
    }
}
