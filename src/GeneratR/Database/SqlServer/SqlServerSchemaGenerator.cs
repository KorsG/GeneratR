using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerSchemaGenerator
    {
        protected static Regex RemoveRelationalColumnSuffixRegex = new Regex(@"(Id|No|Key|Code)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly SqlConnectionStringBuilder _connectionStringBuilder;

        public SqlServerSchemaGenerator(SqlServerSchemaGeneratorSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _connectionStringBuilder = new SqlConnectionStringBuilder(Settings.ConnectionString);
            if (!string.IsNullOrWhiteSpace(settings.DatabaseName))
            {
                _connectionStringBuilder.InitialCatalog = settings.DatabaseName;
            }

            DatabaseName = _connectionStringBuilder.InitialCatalog;
        }

        public DotNetGenerator DotNetGenerator => Settings.DotNetGenerator;

        public SqlServerTypeMapper TypeMapper => Settings.TypeMapper;

        public SqlServerSchemaGeneratorSettings Settings { get; }

        public string DatabaseName { get; }

        public virtual SqlServerDbSchema LoadSqlServerDbSchema()
        {
            var schema = new SqlServerDbSchema();

            var schemaContext = new Schema.SqlServerSchemaContext(_connectionStringBuilder.ConnectionString)
            {
                IncludeSchemas = Settings.IncludeSchemas ?? new HashSet<string>(),
                ExcludeSchemas = Settings.ExcludeSchemas ?? new HashSet<string>(),
            };

            // Tables.
            if (Settings.Table.Generate)
            {
                var dbTables = schemaContext.Tables.GetAll();
                if (dbTables.Any())
                {
                    schema.Tables.AddRange(BuildTables(dbTables));
                }
            }

            // Views.
            if (Settings.View.Generate)
            {
                var dbViews = schemaContext.Views.GetAll();
                if (dbViews.Any())
                {
                    schema.Views.AddRange(BuildViews(dbViews));
                }
            }

            // TableFunctions.
            if (Settings.TableFunction.Generate)
            {
                var dbFuncs = schemaContext.TableFunctions.GetAll();
                if (dbFuncs.Any())
                {
                    schema.TableFunctions.AddRange(BuildTableFunctions(dbFuncs));
                }
            }

            // StoredProcedures.
            if (Settings.StoredProcedure.Generate)
            {
                var dbProcs = schemaContext.StoredProcedures.GetAll(Settings.StoredProcedure.GenerateResultSet);
                if (dbProcs.Any())
                {
                    schema.StoredProcedures.AddRange(BuildStoredProcedures(dbProcs));
                }
            }

            // TableTypes.
            if (Settings.TableType.Generate)
            {
                var dbTypes = schemaContext.TableTypes.GetAll();
                if (dbTypes.Any())
                {
                    schema.TableTypes.AddRange(BuildTableTypes(dbTypes));
                }
            }

            return schema;
        }

        protected virtual List<SqlServerTableConfiguration> BuildTables(IEnumerable<Schema.Table> dbTypes)
        {
            var configs = new List<SqlServerTableConfiguration>();

            if (Settings.IncludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => Settings.IncludeObjects.Any(i => StringLike(x.FullName, i)));
            }

            if (Settings.ExcludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => !Settings.ExcludeObjects.Any(i => StringLike(x.FullName, i)));
            }

            var objSettings = Settings.Table;
            foreach (var t in dbTypes)
            {
                if (!objSettings.ShouldInclude(t)) { continue; }

                var o = new SqlServerTableConfiguration(t, DotNetGenerator, TypeMapper)
                {
                    DotNetModifier = objSettings.DefaultClassDotNetModifier,
                    GenerateForeignKeys = objSettings.GenerateForeignKeys,
                    GenerateReferencingForeignKeys = objSettings.GenerateReferencingForeignKeys,
                    AddDataAnnotationAttributes = objSettings.AddDataAnnotationAttributes,
                    AddConstructor = objSettings.AddConstructor,
                    InheritClassName = objSettings.InheritClass,
                };

                o.Generate = () => objSettings.GenerateFactory(o);
                o.Namespace = objSettings.Namespace.Replace("{schema}", o.DbObject.Schema).Replace("{object}", o.ClassName);

                if (!string.IsNullOrWhiteSpace(objSettings.ImplementInterface))
                {
                    o.ImplementInterfaces.Add(objSettings.ImplementInterface);
                }

                if (objSettings.NamingStrategy == NamingStrategy.Pluralize)
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakePlural(o.DbObject.Name));
                }
                else if (objSettings.NamingStrategy == NamingStrategy.Singularize)
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakeSingular(o.DbObject.Name));
                }
                else
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(o.DbObject.Name);
                }

                // Table columns.
                foreach (var col in t.Columns)
                {
                    var c = new SqlServerColumnConfiguration(col);

                    var propertyName = c.DbObject.Name;
                    if (string.Equals(propertyName, o.ClassName, StringComparison.OrdinalIgnoreCase))
                    {
                        propertyName += "Column";
                    }
                    c.PropertyName = DotNetGenerator.GetAsValidDotNetName(propertyName);
                    c.PropertyType = TypeMapper.ConvertDataTypeToDotNetType(c.DbObject.DataType, c.DbObject.IsNullable);
                    c.DotNetModifier = objSettings.DefaultColumnDotNetModifier;
                    o.Columns.Add(c);
                }

                // Table foreign keys.
                foreach (var fk in t.ForeignKeys)
                {
                    var f = new SqlServerForeignKeyConfiguration(fk)
                    {
                        DotNetModifier = objSettings.DefaultForeignKeyDotNetModifier,
                    };
                    o.ForeignKeys.Add(f);
                }
                foreach (var fk in t.ReferencingForeignKeys)
                {
                    var f = new SqlServerForeignKeyConfiguration(fk)
                    {
                        DotNetModifier = objSettings.DefaultForeignKeyDotNetModifier
                    };
                    o.ReferencingForeignKeys.Add(f);
                }

                configs.Add(o);
            }

            // Foreign key properties and relations must be handled after tables have been parsed, because it needs info from all tables in the collection to parse correctly.
            SetTableCollectionForeignKeyProperties(configs);

            // Add after foreign key parsing.
            if (objSettings.AddDataAnnotationAttributes)
            {
                foreach (var o in configs)
                {
                    AddTableDataAnnotationAttributes(o);
                }
            }

            return configs;
        }

        protected virtual List<SqlServerViewConfiguration> BuildViews(IEnumerable<Schema.View> dbTypes)
        {
            var configs = new List<SqlServerViewConfiguration>();

            if (Settings.IncludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => Settings.IncludeObjects.Any(i => StringLike(x.FullName, i)));
            }

            if (Settings.ExcludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => !Settings.ExcludeObjects.Any(i => StringLike(x.FullName, i)));
            }

            var objSettings = Settings.View;
            foreach (var t in dbTypes)
            {
                if (!objSettings.ShouldInclude(t)) { continue; }

                var o = new SqlServerViewConfiguration(t, DotNetGenerator, TypeMapper)
                {
                    DotNetModifier = objSettings.DefaultClassDotNetModifier,
                    AddDataAnnotationAttributes = objSettings.AddDataAnnotationAttributes,
                    AddConstructor = objSettings.AddConstructor,
                    InheritClassName = objSettings.InheritClass,
                };

                o.Generate = () => objSettings.GenerateFactory(o);
                o.Namespace = objSettings.Namespace.Replace("{schema}", o.DbObject.Schema).Replace("{object}", o.ClassName);

                if (!string.IsNullOrWhiteSpace(objSettings.ImplementInterface))
                {
                    o.ImplementInterfaces.Add(objSettings.ImplementInterface);
                }

                if (objSettings.NamingStrategy == NamingStrategy.Pluralize)
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakePlural(o.DbObject.Name));
                }
                else if (objSettings.NamingStrategy == NamingStrategy.Singularize)
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakeSingular(o.DbObject.Name));
                }
                else
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(o.DbObject.Name);
                }

                // View columns.
                foreach (var col in t.Columns)
                {
                    var c = new SqlServerColumnConfiguration(col);

                    var propertyName = c.DbObject.Name;
                    if (string.Equals(propertyName, o.ClassName, StringComparison.OrdinalIgnoreCase))
                    {
                        propertyName += "Column";
                    }
                    c.PropertyName = DotNetGenerator.GetAsValidDotNetName(propertyName);
                    c.PropertyType = TypeMapper.ConvertDataTypeToDotNetType(c.DbObject.DataType, c.DbObject.IsNullable);
                    c.DotNetModifier = objSettings.DefaultColumnDotNetModifier;
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

        protected virtual List<SqlServerTableTypeConfiguration> BuildTableTypes(IEnumerable<Schema.TableType> dbTypes)
        {
            var configs = new List<SqlServerTableTypeConfiguration>();

            if (Settings.IncludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => Settings.IncludeObjects.Any(i => StringLike(x.FullName, i)));
            }

            if (Settings.ExcludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => !Settings.ExcludeObjects.Any(i => StringLike(x.FullName, i)));
            }

            var objSettings = Settings.TableType;
            foreach (var t in dbTypes)
            {
                if (!objSettings.ShouldInclude(t)) { continue; }

                var o = new SqlServerTableTypeConfiguration(t, DotNetGenerator, TypeMapper)
                {
                    DotNetModifier = objSettings.DefaultClassDotNetModifier,
                    AddDataAnnotationAttributes = objSettings.AddDataAnnotationAttributes,
                    AddConstructor = objSettings.AddConstructor,
                    InheritClassName = objSettings.InheritClass,
                };

                o.Generate = () => objSettings.GenerateFactory(o);
                o.Namespace = objSettings.Namespace.Replace("{schema}", o.DbObject.Schema).Replace("{object}", o.ClassName);

                if (!string.IsNullOrWhiteSpace(objSettings.ImplementInterface))
                {
                    o.ImplementInterfaces.Add(objSettings.ImplementInterface);
                }

                if (objSettings.NamingStrategy == NamingStrategy.Pluralize)
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakePlural(o.DbObject.Name));
                }
                else if (objSettings.NamingStrategy == NamingStrategy.Singularize)
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakeSingular(o.DbObject.Name));
                }
                else
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(o.DbObject.Name);
                }

                // Columns.
                foreach (var col in t.Columns)
                {
                    var c = new SqlServerColumnConfiguration(col);

                    var propertyName = c.DbObject.Name;
                    if (string.Equals(propertyName, o.ClassName, StringComparison.OrdinalIgnoreCase))
                    {
                        propertyName += "Column";
                    }
                    c.PropertyName = DotNetGenerator.GetAsValidDotNetName(propertyName);
                    c.PropertyType = TypeMapper.ConvertDataTypeToDotNetType(c.DbObject.DataType, c.DbObject.IsNullable);
                    c.DotNetModifier = objSettings.DefaultColumnDotNetModifier;
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

        protected virtual List<SqlServerTableFunctionConfiguration> BuildTableFunctions(IEnumerable<Schema.TableFunction> dbTypes)
        {
            var configs = new List<SqlServerTableFunctionConfiguration>();

            if (Settings.IncludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => Settings.IncludeObjects.Any(i => StringLike(x.FullName, i)));
            }

            if (Settings.ExcludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => !Settings.ExcludeObjects.Any(i => StringLike(x.FullName, i)));
            }

            var objSettings = Settings.TableFunction;
            foreach (var t in dbTypes)
            {
                if (!objSettings.ShouldInclude(t)) { continue; }

                var o = new SqlServerTableFunctionConfiguration(t, DotNetGenerator, TypeMapper)
                {
                    DotNetModifier = objSettings.DefaultClassDotNetModifier,
                    AddDataAnnotationAttributes = objSettings.AddDataAnnotationAttributes,
                    AddConstructor = objSettings.AddConstructor,
                    InheritClassName = objSettings.InheritClass,
                };

                o.Generate = () => objSettings.GenerateFactory(o);
                o.Namespace = objSettings.Namespace.Replace("{schema}", o.DbObject.Schema).Replace("{object}", o.ClassName);

                if (!string.IsNullOrWhiteSpace(objSettings.ImplementInterface))
                {
                    o.ImplementInterfaces.Add(objSettings.ImplementInterface);
                }

                if (objSettings.NamingStrategy == NamingStrategy.Pluralize)
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakePlural(o.DbObject.Name));
                }
                else if (objSettings.NamingStrategy == NamingStrategy.Singularize)
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakeSingular(o.DbObject.Name));
                }
                else
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(o.DbObject.Name);
                }

                // Function columns.
                foreach (var col in t.Columns)
                {
                    var c = new SqlServerColumnConfiguration(col);

                    var propertyName = c.DbObject.Name;
                    if (string.Equals(propertyName, o.ClassName, StringComparison.OrdinalIgnoreCase))
                    {
                        propertyName += "Column";
                    }
                    c.PropertyName = DotNetGenerator.GetAsValidDotNetName(propertyName);
                    c.PropertyType = TypeMapper.ConvertDataTypeToDotNetType(c.DbObject.DataType, c.DbObject.IsNullable);
                    c.DotNetModifier = Settings.TableFunction.DefaultColumnDotNetModifier;
                    o.Columns.Add(c);
                }

                // Function parameters.
                foreach (var param in t.Parameters)
                {
                    var p = new SqlServerParameterConfiguration(param);
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

        protected virtual List<SqlServerStoredProcedureConfiguration> BuildStoredProcedures(IEnumerable<Schema.StoredProcedure> dbTypes)
        {
            var configs = new List<SqlServerStoredProcedureConfiguration>();

            if (Settings.IncludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => Settings.IncludeObjects.Any(i => StringLike(x.FullName, i)));
            }

            if (Settings.ExcludeObjects?.Any() == true)
            {
                dbTypes = dbTypes.Where(x => !Settings.ExcludeObjects.Any(i => StringLike(x.FullName, i)));
            }

            var objSettings = Settings.StoredProcedure;
            foreach (var t in dbTypes)
            {
                if (!objSettings.ShouldInclude(t)) { continue; }

                var o = new SqlServerStoredProcedureConfiguration(t, DotNetGenerator, TypeMapper)
                {
                    DotNetModifier = objSettings.DefaultClassDotNetModifier,
                    AddDataAnnotationAttributes = objSettings.AddDataAnnotationAttributes,
                    AddConstructor = objSettings.AddConstructor,
                    InheritClassName = objSettings.InheritClass,
                    GenerateOutputParameters = objSettings.GenerateOutputParameters,
                    GenerateResultSet = objSettings.GenerateResultSet,
                };

                o.Generate = () => objSettings.GenerateFactory(o);
                o.Namespace = objSettings.Namespace.Replace("{schema}", o.DbObject.Schema).Replace("{object}", o.ClassName);

                if (!string.IsNullOrWhiteSpace(objSettings.ImplementInterface))
                {
                    o.ImplementInterfaces.Add(objSettings.ImplementInterface);
                }

                if (objSettings.NamingStrategy == NamingStrategy.Pluralize)
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakePlural(o.DbObject.Name));
                }
                else if (objSettings.NamingStrategy == NamingStrategy.Singularize)
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakeSingular(o.DbObject.Name));
                }
                else
                {
                    o.ClassName = DotNetGenerator.GetAsValidDotNetName(o.DbObject.Name);
                }

                // StoredProcedure ResultColumns.
                if (objSettings.GenerateResultSet)
                {
                    foreach (var colr in t.ResultColumns)
                    {
                        var c = new SqlServerStoredProcedureResultColumnConfiguration(colr);
                        c.PropertyName = DotNetGenerator.GetAsValidDotNetName(c.DbObject.Name);
                        c.PropertyType = TypeMapper.ConvertDataTypeToDotNetType(c.DbObject.DataType, c.DbObject.IsNullable);
                        c.DotNetModifier = objSettings.DefaultColumnDotNetModifier;
                        o.ResultColumns.Add(c);
                    }
                }

                // StoredProcedure parameters
                foreach (var param in t.Parameters)
                {
                    var p = new SqlServerParameterConfiguration(param);
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

        protected virtual void AddTableDataAnnotationAttributes(SqlServerTableConfiguration config)
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

        protected virtual void AddViewDataAnnotationAttributes(SqlServerViewConfiguration config)
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

        protected virtual void AddTableFunctionDataAnnotationAttributes(SqlServerTableFunctionConfiguration config)
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

        protected virtual void AddTableTypeDataAnnotationAttributes(SqlServerTableTypeConfiguration config)
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

        protected virtual void AddStoredProcedureDataAnnotationAttributes(SqlServerStoredProcedureConfiguration config)
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

        protected virtual void AddColumnDataAnnotationAttributes(SqlServerColumnConfiguration config)
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

        protected virtual void AddStoredProcedureParameterDataAnnotationAttributes(SqlServerParameterConfiguration col)
        {
        }

        protected virtual void AddStoredProcedureColumnDataAnnotationAttributes(SqlServerStoredProcedureResultColumnConfiguration col)
        {
            var hasNameDiff = !string.Equals(col.DbObject.Name, col.PropertyName, StringComparison.OrdinalIgnoreCase);
            if (hasNameDiff)
            {
                var attr = DotNetGenerator.AttributeFactory.CreateColumnAttribute(col.DbObject.Name);
                col.Attributes.Add(attr);
            }
        }

        /// <summary>
        /// Compares the string against a given pattern.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="pattern">The pattern to match, where "*" means any sequence of characters, and "?" means any single character.</param>
        /// <returns><c>true</c> if the string matches the given pattern; otherwise <c>false</c>.</returns>
        protected static bool StringLike(string str, string pattern)
        {
            return new Regex(
                "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            ).IsMatch(str);
        }

        private void SetTableCollectionForeignKeyProperties(IEnumerable<SqlServerTableConfiguration> tables)
        {
            var foreignKeyCollectionType = Settings.Table.ForeignKeyCollectionType;
            var foreignKeyNamingStrategy = Settings.Table.ForeignKeyNamingStrategy;

            foreach (var tbl in tables)
            {
                // Remove all foreign keys from the collection if the table they reference to/from does not exist in the provided table collection.
                var nonExistingFkRelations = new List<SqlServerForeignKeyConfiguration>();
                var nonExistingReferencingFkRelations = new List<SqlServerForeignKeyConfiguration>();
                foreach (var fk in tbl.ForeignKeys)
                {
                    if (!tables.Where(x => x.DbObject.ObjectID == fk.DbObject.ToObjectID).Any())
                    {
                        nonExistingFkRelations.Add(fk);
                    }
                }
                foreach (var fk in tbl.ReferencingForeignKeys)
                {
                    if (!tables.Where(x => x.DbObject.ObjectID == fk.DbObject.FromObjectID).Any())
                    {
                        nonExistingReferencingFkRelations.Add(fk);
                    }
                }
                tbl.ForeignKeys.RemoveAll(q => nonExistingFkRelations.Contains(q));
                tbl.ReferencingForeignKeys.RemoveAll(q => nonExistingReferencingFkRelations.Contains(q));

                if (foreignKeyNamingStrategy == ForeignKeyNamingStrategy.ForeignKeyName)
                {
                    foreach (var fk in tbl.ForeignKeys)
                    {
                        fk.PropertyName = fk.DbObject.ForeignKeyName;
                        fk.PropertyType = tables.Where(x => x.DbObject.ObjectID == fk.DbObject.ToObjectID).Single().ClassName;
                    }
                    foreach (var fk in tbl.ReferencingForeignKeys)
                    {
                        fk.PropertyName = fk.DbObject.ForeignKeyName;
                        fk.PropertyType = tables.Where(x => x.DbObject.ObjectID == fk.DbObject.FromObjectID).Single().ClassName;
                    }
                }
                else if (foreignKeyNamingStrategy == ForeignKeyNamingStrategy.ReferencingTableName)
                {
                    foreach (var fk in tbl.ForeignKeys)
                    {
                        fk.PropertyName = DotNetGenerator.GetAsValidDotNetName(fk.DbObject.ToName);
                        if (fk.DbObject.IsSelfReferencing)
                        {
                            fk.PropertyName = DotNetGenerator.GetAsValidDotNetName(ParseSelfReferencingForeignKeyName(fk.DbObject));
                        }
                        fk.PropertyType = tables.Where(x => x.DbObject.ObjectID == fk.DbObject.ToObjectID).Single().ClassName;
                    }

                    foreach (var fk in tbl.ReferencingForeignKeys)
                    {
                        if (fk.DbObject.RelationshipType == Schema.ForeignKeyRelationshipType.OneToOne)
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
                    var multiples = tbl.ForeignKeys.GroupBy(x => x.PropertyName).Where(x => x.Count() > 1).SelectMany(x => x).ToList();
                    multiples.AddRange(tbl.ReferencingForeignKeys.GroupBy(x => x.PropertyName).Where(x => x.Count() > 1).SelectMany(x => x).ToList());
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
                    foreach (var tblName in tbl.ForeignKeys.Select(x => x.DbObject.FromName).Distinct().ToList())
                    {
                        var fks = tbl.ForeignKeys.Where(x => x.DbObject.FromName == tblName).ToList();

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
                    foreach (var tblName in tbl.ReferencingForeignKeys.Select(x => x.DbObject.ToName).Distinct().ToList())
                    {
                        var fks = tbl.ReferencingForeignKeys.Where(x => x.DbObject.ToName == tblName).ToList();

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
                            if (fk.DbObject.RelationshipType == Schema.ForeignKeyRelationshipType.OneToOne)
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

        private string ParseSelfReferencingForeignKeyName(Schema.ForeignKey fk)
        {
            var parsed = string.Concat(fk.FromColumns.Select(x => RemoveRelationalColumnSuffix(x.ColumnName)));
            parsed += fk.ToName;
            return parsed;
        }

        protected virtual string RemoveRelationalColumnSuffix(string columnName)
        {
            return RemoveRelationalColumnSuffixRegex.Replace(columnName, string.Empty);
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

        protected string ConvertDataTypeToDotNetType(string sqlDataType, bool nullable, bool isTableType = false)
            => Settings.TypeMapper.ConvertDataTypeToDotNetType(sqlDataType, nullable, isTableType);

        protected string ConvertDbParameterToDotNetType(Schema.Parameter p)
            => Settings.TypeMapper.ConvertDbParameterToDotNetType(p);

        protected string ConvertDataTypeToDbType(string sqlDataType, bool isTableType = false)
            => Settings.TypeMapper.ConvertDataTypeToDbType(sqlDataType, isTableType);

        protected string ConvertDataTypeToSqlDbType(string sqlDataType, bool isTableType = false)
            => Settings.TypeMapper.ConvertDataTypeToSqlDbType(sqlDataType, isTableType);

        protected void ParseSqlServerDataType(string sqlDataType, bool nullable, out string dotNetType, out string dbType, out string sqlDbType)
            => Settings.TypeMapper.ParseSqlServerDataType(sqlDataType, nullable, out dotNetType, out dbType, out sqlDbType);
    }
}
