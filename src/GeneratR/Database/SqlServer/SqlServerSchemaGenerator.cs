using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerSchemaGenerator : GenericDbSchemaGenerator
    {
        public SqlServerSchemaGeneratorSettings Settings { get; }

        public SqlServerSchemaGenerator(DotNetGenerator DotNetGenerator)
            : this(DotNetGenerator, new SqlServerSchemaGeneratorSettings())
        {
        }

        public SqlServerSchemaGenerator(DotNetGenerator DotNetGenerator, SqlServerSchemaGeneratorSettings settings)
            : base(DotNetGenerator, settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public virtual SqlServerDbSchema LoadSqlServerDbSchema()
        {
            var schema = new SqlServerDbSchema();

            var schemaContext = new Schema.SqlServerSchemaContext(Settings.ConnectionString);

            // Tables
            if (Settings.Table.Generate)
            {
                var dbTables = schemaContext.Tables.GetAll();
                foreach (var tbl in dbTables)
                {
                    if (!base.ShouldGenerateDbObject(tbl.Name, tbl.Schema)) { continue; }

                    var o = new SqlServerTableConfiguration(tbl);
                    o.Namespace = Settings.Table.Namespace.Replace("{schema}", o.DbObject.Schema).Replace("{object}", o.ClassName);

                    if (Settings.Table.NamingStrategy == NamingStrategy.Pluralize)
                    {
                        o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakePlural(o.DbObject.Name));
                    }
                    else if (Settings.Table.NamingStrategy == NamingStrategy.Singularize)
                    {
                        o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakeSingular(o.DbObject.Name));
                    }
                    else
                    {
                        o.ClassName = DotNetGenerator.GetAsValidDotNetName(o.DbObject.Name);
                    }

                    // Table columns.
                    foreach (var col in tbl.Columns)
                    {
                        var c = new SqlServerColumnConfiguration(col);
                        c.PropertyName = DotNetGenerator.GetAsValidDotNetName(c.DbObject.Name);
                        c.PropertyType = ConvertDataTypeToDotNetType(c.DbObject.DataType, c.DbObject.IsNullable);
                        c.DotNetModifier = Settings.Table.DefaultColumnDotNetModifier;
                        o.Columns.Add(c);
                    }

                    // Table foreign keys.
                    foreach (var fk in tbl.ForeignKeys)
                    {
                        var f = new SqlServerForeignKeyConfiguration(fk)
                        {
                            DotNetModifier = Settings.Table.DefaultForeignKeyDotNetModifier
                        };
                        o.ForeignKeys.Add(f);
                    }
                    foreach (var fk in tbl.ReferencingForeignKeys)
                    {
                        var f = new SqlServerForeignKeyConfiguration(fk)
                        {
                            DotNetModifier = Settings.Table.DefaultForeignKeyDotNetModifier
                        };
                        o.ReferencingForeignKeys.Add(f);
                    }

                    schema.Tables.Add(o);
                }

                // Foreign key properties and relations must be handled after tables have been parsed, because it needs info from all tables in the collection to parse correctly.
                SetTableCollectionForeignKeyProperties(schema.Tables);
            }

            // Views.
            if (Settings.View.Generate)
            {
                var dbViews = schemaContext.Views.GetAll();
                foreach (var vw in dbViews)
                {
                    if (!base.ShouldGenerateDbObject(vw.Name, vw.Schema)) { continue; }

                    var o = new SqlServerViewConfiguration(vw);

                    o.Namespace = Settings.View.Namespace.Replace("{schema}", o.DbObject.Schema).Replace("{object}", o.ClassName);

                    if (Settings.View.NamingStrategy == NamingStrategy.Pluralize)
                    {
                        o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakePlural(o.DbObject.Name));
                    }
                    else if (Settings.View.NamingStrategy == NamingStrategy.Singularize)
                    {
                        o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakeSingular(o.DbObject.Name));
                    }
                    else
                    {
                        o.ClassName = DotNetGenerator.GetAsValidDotNetName(o.DbObject.Name);
                    }

                    // View columns.
                    foreach (var col in vw.Columns)
                    {
                        var c = new SqlServerColumnConfiguration(col);
                        c.PropertyName = DotNetGenerator.GetAsValidDotNetName(c.DbObject.Name);
                        c.PropertyType = ConvertDataTypeToDotNetType(c.DbObject.DataType, c.DbObject.IsNullable);
                        c.DotNetModifier = Settings.View.DefaultColumnDotNetModifier;
                        o.Columns.Add(c);
                    }

                    schema.Views.Add(o);
                }
            }

            // TableFunctions.
            if (Settings.TableFunction.Generate)
            {
                var dbFuncs = schemaContext.TableFunctions.GetAll();
                foreach (var f in dbFuncs)
                {
                    if (!base.ShouldGenerateDbObject(f.Name, f.Schema)) { continue; }

                    var o = new SqlServerTableFunctionConfiguration(f);

                    o.Namespace = Settings.TableFunction.Namespace.Replace("{schema}", o.DbObject.Schema).Replace("{object}", o.ClassName);

                    if (Settings.TableFunction.NamingStrategy == NamingStrategy.Pluralize)
                    {
                        o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakePlural(o.DbObject.Name));
                    }
                    else if (Settings.TableFunction.NamingStrategy == NamingStrategy.Singularize)
                    {
                        o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakeSingular(o.DbObject.Name));
                    }
                    else
                    {
                        o.ClassName = DotNetGenerator.GetAsValidDotNetName(o.DbObject.Name);
                    }

                    // Function columns.
                    foreach (var col in f.Columns)
                    {
                        var c = new SqlServerColumnConfiguration(col);
                        c.PropertyName = DotNetGenerator.GetAsValidDotNetName(c.DbObject.Name);
                        c.PropertyType = ConvertDataTypeToDotNetType(c.DbObject.DataType, c.DbObject.IsNullable);
                        c.DotNetModifier = Settings.TableFunction.DefaultColumnDotNetModifier;
                        o.Columns.Add(c);
                    }

                    // Function parameters.
                    foreach (var param in f.Parameters)
                    {
                        var p = new SqlServerParameterConfiguration(param);
                        p.PropertyName = DotNetGenerator.GetAsValidDotNetName(p.DbObject.Name);
                        p.PropertyType = ConvertDbParameterToDotNetType(p.DbObject);
                        o.Parameters.Add(p);
                    }

                    schema.TableFunctions.Add(o);
                }
            }

            // StoredProcedures.
            if (Settings.StoredProcedure.Generate)
            {
                var dbProcs = schemaContext.StoredProcedures.GetAll(Settings.StoredProcedure.GenerateResultSet);
                foreach (var proc in dbProcs)
                {
                    if (!base.ShouldGenerateDbObject(proc.Name, proc.Schema)) { continue; }

                    var o = new SqlServerStoredProcedureConfiguration(proc);

                    o.Namespace = Settings.StoredProcedure.Namespace.Replace("{schema}", o.DbObject.Schema).Replace("{object}", o.ClassName);

                    if (Settings.StoredProcedure.NamingStrategy == NamingStrategy.Pluralize)
                    {
                        o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakePlural(o.DbObject.Name));
                    }
                    else if (Settings.StoredProcedure.NamingStrategy == NamingStrategy.Singularize)
                    {
                        o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakeSingular(o.DbObject.Name));
                    }
                    else
                    {
                        o.ClassName = DotNetGenerator.GetAsValidDotNetName(o.DbObject.Name);
                    }

                    // StoredProcedure ResultColumns.
                    if (Settings.StoredProcedure.GenerateResultSet)
                    {
                        foreach (var colr in proc.ResultColumns)
                        {
                            var c = new SqlServerStoredProcedureResultColumnConfiguration(colr);
                            c.PropertyName = DotNetGenerator.GetAsValidDotNetName(c.DbObject.Name);
                            c.PropertyType = ConvertDataTypeToDotNetType(c.DbObject.DataType, c.DbObject.IsNullable);
                            c.DotNetModifier = Settings.StoredProcedure.DefaultColumnDotNetModifier;
                            o.ResultColumns.Add(c);
                        }
                    }

                    // StoredProcedure parameters
                    foreach (var param in proc.Parameters)
                    {
                        var p = new SqlServerParameterConfiguration(param);
                        p.PropertyName = DotNetGenerator.GetAsValidDotNetName(p.DbObject.Name);
                        p.PropertyType = ConvertDbParameterToDotNetType(p.DbObject);
                        o.Parameters.Add(p);
                    }

                    schema.StoredProcedures.Add(o);
                }
            }


            // TableTypes.
            if (Settings.TableType.Generate)
            {
                var dbTypes = schemaContext.TableTypes.GetAll();
                foreach (var t in dbTypes)
                {
                    if (!base.ShouldGenerateDbObject(t.Name, t.Schema)) { continue; }

                    var o = new SqlServerTableTypeConfiguration(t);

                    o.Namespace = Settings.TableType.Namespace.Replace("{schema}", o.DbObject.Schema).Replace("{object}", o.ClassName);

                    if (Settings.TableType.NamingStrategy == NamingStrategy.Pluralize)
                    {
                        o.ClassName = DotNetGenerator.GetAsValidDotNetName(Inflector.MakePlural(o.DbObject.Name));
                    }
                    else if (Settings.TableType.NamingStrategy == NamingStrategy.Singularize)
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
                        c.PropertyName = DotNetGenerator.GetAsValidDotNetName(c.DbObject.Name);
                        c.PropertyType = ConvertDataTypeToDotNetType(c.DbObject.DataType, c.DbObject.IsNullable);
                        c.DotNetModifier = Settings.TableType.DefaultColumnDotNetModifier;
                        o.Columns.Add(c);
                    }

                    schema.TableTypes.Add(o);
                }
            }

            return schema;
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

        public virtual string ConvertDataTypeToDotNetType(string sqlDataType, bool nullable, bool isTableType = false)
        {
            if (isTableType)
            {
                sqlDataType = "table type";
            }
            ParseSqlServerDataType(sqlDataType, nullable, out string dotNetType, out string dbType, out string sqlDbType);
            return dotNetType;
        }

        public virtual string ConvertDbParameterToDotNetType(Schema.Parameter p)
        {
            if (p.IsTableType)
            {
                return p.DataType;
            }
            ParseSqlServerDataType(p.DataType, p.IsNullable, out string dotNetType, out string dbType, out string sqlDbType);
            return dotNetType;
        }

        public virtual string ConvertDataTypeToDbType(string sqlDataType, bool isTableType = false)
        {
            if (isTableType)
            {
                sqlDataType = "table type";
            }
            ParseSqlServerDataType(sqlDataType, false, out string _, out string dbType, out string sqlDbType);
            return dbType;
        }

        public virtual string ConvertDataTypeToSqlDbType(string sqlDataType, bool isTableType = false)
        {
            if (isTableType)
            {
                sqlDataType = "table type";
            }
            ParseSqlServerDataType(sqlDataType, false, out string dotNetType, out string dbType, out string sqlDbType);
            return sqlDbType;
        }

        public virtual void ParseSqlServerDataType(string sqlDataType, bool nullable, out string dotNetType, out string dbType, out string sqlDbType)
        {
            switch (sqlDataType.ToLower())
            {
                case "char":
                    dotNetType = DotNetGenerator.GetTypeAsString(typeof(string));
                    dbType = "DbType.AnsiStringFixedLength";
                    sqlDbType = "SqlDbType.Char";
                    break;
                case "varchar":
                    dotNetType = DotNetGenerator.GetTypeAsString(typeof(string));
                    dbType = "DbType.AnsiString";
                    sqlDbType = "SqlDbType.VarChar";
                    break;
                case "text":
                    dotNetType = DotNetGenerator.GetTypeAsString(typeof(string));
                    dbType = "DbType.AnsiString";
                    sqlDbType = "SqlDbType.VarChar";
                    break;
                case "nchar":
                    dotNetType = DotNetGenerator.GetTypeAsString(typeof(string));
                    dbType = "DbType.StringFixedLength";
                    sqlDbType = "SqlDbType.NChar";
                    break;
                case "nvarchar":
                    dotNetType = DotNetGenerator.GetTypeAsString(typeof(string));
                    dbType = "DbType.String";
                    sqlDbType = "SqlDbType.NVarChar";
                    break;
                case "ntext":
                    dotNetType = DotNetGenerator.GetTypeAsString(typeof(string));
                    dbType = "DbType.String";
                    sqlDbType = "SqlDbType.NVarChar";
                    break;
                case "bigint":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(long?)) : DotNetGenerator.GetTypeAsString(typeof(long));
                    dbType = "DbType.Int64";
                    sqlDbType = "SqlDbType.BigInt";
                    break;
                case "int":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(int?)) : DotNetGenerator.GetTypeAsString(typeof(int));
                    dbType = "DbType.Int32";
                    sqlDbType = "SqlDbType.Int";
                    break;
                case "smallint":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(short?)) : DotNetGenerator.GetTypeAsString(typeof(short));
                    dbType = "DbType.Int16";
                    sqlDbType = "SqlDbType.SmallInt";
                    break;
                case "tinyint":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(byte?)) : DotNetGenerator.GetTypeAsString(typeof(byte));
                    dbType = "DbType.Byte";
                    sqlDbType = "SqlDbType.TinyInt";
                    break;
                case "uniqueidentifier":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(Guid?)) : DotNetGenerator.GetTypeAsString(typeof(Guid));
                    dbType = "DbType.Guid";
                    sqlDbType = "SqlDbType.UniqueIdentifier";
                    break;
                case "smalldatetime":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(DateTime?)) : DotNetGenerator.GetTypeAsString(typeof(DateTime));
                    dbType = "DbType.DateTime";
                    sqlDbType = "SqlDbType.SmallDateTime";
                    break;
                case "datetime":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(DateTime?)) : DotNetGenerator.GetTypeAsString(typeof(DateTime));
                    dbType = "DbType.DateTime";
                    sqlDbType = "SqlDbType.DateTime";
                    break;
                case "datetime2":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(DateTime?)) : DotNetGenerator.GetTypeAsString(typeof(DateTime));
                    dbType = "DbType.DateTime2";
                    sqlDbType = "SqlDbType.DateTime2";
                    break;
                case "date":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(DateTime?)) : DotNetGenerator.GetTypeAsString(typeof(DateTime));
                    dbType = "DbType.Date";
                    sqlDbType = "SqlDbType.Date";
                    break;
                case "time":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(TimeSpan?)) : DotNetGenerator.GetTypeAsString(typeof(TimeSpan));
                    dbType = "DbType.Time";
                    sqlDbType = "SqlDbType.Time";
                    break;
                case "datetimeoffset":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(DateTimeOffset?)) : DotNetGenerator.GetTypeAsString(typeof(DateTimeOffset));
                    dbType = "DbType.DateTimeOffset";
                    sqlDbType = "SqlDbType.DateTimeOffset";
                    break;
                case "float":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(double?)) : DotNetGenerator.GetTypeAsString(typeof(double));
                    dbType = "DbType.Double";
                    sqlDbType = "SqlDbType.Float";
                    break;
                case "real":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(float?)) : DotNetGenerator.GetTypeAsString(typeof(float));
                    dbType = "DbType.Double";
                    sqlDbType = "SqlDbType.Real";
                    break;
                case "numeric":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(decimal?)) : DotNetGenerator.GetTypeAsString(typeof(decimal));
                    dbType = "DbType.Decimal";
                    sqlDbType = "SqlDbType.Decimal";
                    break;
                case "smallmoney":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(decimal?)) : DotNetGenerator.GetTypeAsString(typeof(decimal));
                    dbType = "DbType.Decimal";
                    sqlDbType = "SqlDbType.SmallMoney";
                    break;
                case "decimal":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(decimal?)) : DotNetGenerator.GetTypeAsString(typeof(decimal));
                    dbType = "DbType.Decimal";
                    sqlDbType = "SqlDbType.Decimal";
                    break;
                case "money":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(decimal?)) : DotNetGenerator.GetTypeAsString(typeof(decimal));
                    dbType = "DbType.Decimal";
                    sqlDbType = "SqlDbType.Money";
                    break;
                case "bit":
                    dotNetType = nullable ? DotNetGenerator.GetTypeAsString(typeof(bool?)) : DotNetGenerator.GetTypeAsString(typeof(bool));
                    dbType = "DbType.Boolean";
                    sqlDbType = "SqlDbType.Bit";
                    break;
                case "image":
                    dotNetType = DotNetGenerator.GetTypeAsString(typeof(byte[]));
                    dbType = "DbType.Binary";
                    sqlDbType = "SqlDbType.Image";
                    break;
                case "binary":
                    dotNetType = DotNetGenerator.GetTypeAsString(typeof(byte[]));
                    dbType = "DbType.Binary";
                    sqlDbType = "SqlDbType.Binary";
                    break;
                case "varbinary":
                    dotNetType = DotNetGenerator.GetTypeAsString(typeof(byte[]));
                    dbType = "DbType.Binary";
                    sqlDbType = "SqlDbType.VarBinary";
                    break;
                case "timestamp":
                    dotNetType = DotNetGenerator.GetTypeAsString(typeof(byte[]));
                    dbType = "DbType.Binary";
                    sqlDbType = "SqlDbType.Timestamp";
                    break;
                case "geography":
                    dotNetType = "Microsoft.SqlServer.Types.SqlGeography";
                    dbType = "DbType.Object";
                    sqlDbType = "SqlDbType.Udt";
                    break;
                case "geometry":
                    dotNetType = "Microsoft.SqlServer.Types.SqlGeometry";
                    dbType = "DbType.Object";
                    sqlDbType = "SqlDbType.Udt";
                    break;
                case "table type":
                    dotNetType = DotNetGenerator.GetTypeAsString(typeof(object));
                    dbType = "DbType.Object";
                    sqlDbType = "SqlDbType.Structured";
                    break;
                case "sysname":
                    dotNetType = DotNetGenerator.GetTypeAsString(typeof(string));
                    dbType = "DbType.String";
                    sqlDbType = "SqlDbType.NVarChar";
                    break;
                case "xml":
                    dotNetType = DotNetGenerator.GetTypeAsString(typeof(string));
                    dbType = "DbType.Xml";
                    sqlDbType = "SqlDbType.Xml";
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Unknown SqlServerDataType: '{0}'", sqlDataType));
            }
        }
    }
}
