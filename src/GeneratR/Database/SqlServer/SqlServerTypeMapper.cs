﻿using GeneratR.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTypeMapper
    {
        protected static readonly HashSet<string> _integerTypes = new(StringComparer.OrdinalIgnoreCase) { "byte", "tinyint", "smallint", "int", "bigint", };
        protected static readonly HashSet<string> _variableStringTypes = new(StringComparer.OrdinalIgnoreCase) { "varchar", "nvarchar", };
        protected static readonly HashSet<string> _fixedStringTypes = new(StringComparer.OrdinalIgnoreCase) { "char", "nchar", };
        protected static readonly HashSet<string> _ansiStringTypes = new(StringComparer.OrdinalIgnoreCase) { "char", "varchar", };
        protected static readonly HashSet<string> _unicodeStringTypes = new(StringComparer.OrdinalIgnoreCase) { "nchar", "nvarchar", };
        protected static readonly HashSet<string> _allStringTypes = new(_ansiStringTypes.Union(_unicodeStringTypes), StringComparer.OrdinalIgnoreCase);
        protected static readonly HashSet<string> _decimalTypes = new(StringComparer.OrdinalIgnoreCase) { "decimal", "numeric", };
        protected static readonly HashSet<string> _dateTimeTypes = new(StringComparer.OrdinalIgnoreCase) { "date", "time", "datetime", "datetime2", "datetimeoffset", };
        protected static readonly HashSet<string> _dateTimeTypesWithoutScale = new(StringComparer.OrdinalIgnoreCase) { "date", "time", "datetime", };
        protected static readonly HashSet<string> _dateTimeTypesWithScale = new(StringComparer.OrdinalIgnoreCase) { "datetime2", "datetimeoffset", };
        protected static readonly HashSet<string> _rowVersionTypes = new(StringComparer.OrdinalIgnoreCase) { "timestamp", "rowversion", };

        public SqlServerTypeMapper(DotNetGenerator dotNetGenerator)
        {
            DotNetGenerator = dotNetGenerator;
        }

        internal DotNetGenerator DotNetGenerator { get; }

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
            switch (sqlDataType.ToLowerInvariant())
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

        public virtual string GetFullColumnDataType(Schema.Column col)
        {
            if (DataTypeIsString(col))
            {
                var colLength = col.Length == -1 ? "max" : col.Length.ToString();
                return $"{col.DataType.ToLowerInvariant()}({colLength})";
            }
            else if (DataTypeIsDecimal(col))
            {
                return $"{col.DataType.ToLowerInvariant()}({col.Precision}, {col.Scale})";
            }
            else if (DataTypeIsDateTimeWithScale(col))
            {
                return $"{col.DataType.ToLowerInvariant()}({col.Scale})";
            }
            else if (DataTypeIsRowVersion(col.DataType))
            {
                return "rowversion";
            }

            return col.DataType.ToLowerInvariant();
        }

        public virtual bool DataTypeIsInteger(Schema.Column column) => DataTypeIsInteger(column.DataType);
        public virtual bool DataTypeIsInteger(string dataType) => _integerTypes.Contains(dataType);

        public virtual bool DataTypeIsVariableStringLength(Schema.Column column) => DataTypeIsVariableStringLength(column.DataType);
        public virtual bool DataTypeIsVariableStringLength(string dataType) => _variableStringTypes.Contains(dataType);

        public virtual bool DataTypeIsFixedStringLength(Schema.Column column) => DataTypeIsFixedStringLength(column.DataType);
        public virtual bool DataTypeIsFixedStringLength(string dataType) => _fixedStringTypes.Contains(dataType);

        public virtual bool DataTypeIsAnsiString(Schema.Column column) => DataTypeIsAnsiString(column.DataType);
        public virtual bool DataTypeIsAnsiString(string dataType) => _ansiStringTypes.Contains(dataType);

        public virtual bool DataTypeIsUnicodeString(Schema.Column column) => DataTypeIsUnicodeString(column.DataType);
        public virtual bool DataTypeIsUnicodeString(string dataType) => _unicodeStringTypes.Contains(dataType);

        public virtual bool DataTypeIsString(Schema.Column column) => DataTypeIsString(column.DataType);
        public virtual bool DataTypeIsString(string dataType) => _allStringTypes.Contains(dataType);

        public virtual bool DataTypeIsDecimal(Schema.Column column) => DataTypeIsDecimal(column.DataType);
        public virtual bool DataTypeIsDecimal(string dataType) => _decimalTypes.Contains(dataType);

        public virtual bool DataTypeIsDateTime(Schema.Column column) => DataTypeIsDateTime(column.DataType);
        public virtual bool DataTypeIsDateTime(string dataType) => _dateTimeTypes.Contains(dataType);

        public virtual bool DataTypeIsDateTimeWithoutScale(Schema.Column column) => DataTypeIsDateTimeWithoutScale(column.DataType);
        public virtual bool DataTypeIsDateTimeWithoutScale(string dataType) => _dateTimeTypesWithoutScale.Contains(dataType);

        public virtual bool DataTypeIsDateTimeWithScale(Schema.Column column) => DataTypeIsDateTimeWithScale(column.DataType);
        public virtual bool DataTypeIsDateTimeWithScale(string dataType) => _dateTimeTypesWithScale.Contains(dataType);

        public virtual bool DataTypeIsRowVersion(Schema.Column column) => DataTypeIsRowVersion(column.DataType);
        public virtual bool DataTypeIsRowVersion(string dataType) => _rowVersionTypes.Contains(dataType);
    }
}
