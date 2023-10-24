using GeneratR.DotNet;
using GeneratR.ExtensionMethods;
using System;
using System.Collections.Generic;

namespace GeneratR.Database.SqlServer;

public class LinqToDbTypeMapper : SqlServerTypeMapper
{
    public LinqToDbTypeMapper(DotNetGenerator dotNetGenerator) : base(dotNetGenerator)
    {
    }

    internal string GetLinqToDbDataParameter(ParameterCodeModel param, string valueArg = null)
    {
        var direction = param.DbObject.Direction switch
        {
            Schema.ParameterDirection.OutDirection => "System.Data.ParameterDirection.Output",
            Schema.ParameterDirection.InDirection => "System.Data.ParameterDirection.Input",
            Schema.ParameterDirection.InAndOutDirection => "System.Data.ParameterDirection.InputOutput",
            _ => throw new NotImplementedException($"Direction: {param.DbObject.Direction} not mapped"),
        };

        var initArgs = new List<string>()
        {
            @$"Name = ""@{param.DbObject.Name}""",
            // TODO: Consider LinqToDbType also/instead (remember table-valued types when designing)
            @$"DbType = ""{param.DbObject.DataType}""",
            @$"Direction = {direction}",
        };

        if (param.DbObject.Direction.In(Schema.ParameterDirection.InDirection, Schema.ParameterDirection.InAndOutDirection) && !string.IsNullOrWhiteSpace(valueArg))
        {
            initArgs.Add(@$"Value = {valueArg}");
        }

        if (DataTypeIsString(param.DbObject.DataType))
        {
            initArgs.Add($"Size = {param.DbObject.Length}");
        }
        else if (DataTypeIsDecimal(param.DbObject.DataType))
        {
            initArgs.Add($"Precision = {param.DbObject.Precision}");
            initArgs.Add($"Scale = {param.DbObject.Scale}");
        }
        else if (DataTypeIsDateTimeWithScale(param.DbObject.DataType))
        {
            initArgs.Add($"Scale = {param.DbObject.Scale}");
        }

        return $"new DataParameter() {{ {string.Join(", ", initArgs)} }}";
    }

    public virtual string GetLinqToDbColumnDataType(string sqlServerDataType)
    {
        const string prefix = "LinqToDB.DataType.";

        return sqlServerDataType.ToLowerInvariant() switch
        {
            "image" => prefix + "Image",
            "text" => prefix + "Text",
            "binary" => prefix + "Binary",
            "tinyint" => prefix + "Byte",
            "date" => prefix + "Date",
            "time" => prefix + "Time",
            "bit" => prefix + "Boolean",
            "smallint" => prefix + "Int16",
            "decimal" => prefix + "Decimal",
            "int" => prefix + "Int32",
            "smalldatetime" => prefix + "SmallDateTime",
            "real" => prefix + "Single",
            "money" => prefix + "Money",
            "datetime" => prefix + "DateTime",
            "float" => prefix + "Double",
            "numeric" => prefix + "Decimal",
            "smallmoney" => prefix + "SmallMoney",
            "datetime2" => prefix + "DateTime2",
            "bigint" => prefix + "Int64",
            "varbinary" => prefix + "VarBinary",
            "timestamp" => prefix + "Timestamp",
            "sysname" => prefix + "NVarChar",
            "nvarchar" => prefix + "NVarChar",
            "varchar" => prefix + "VarChar",
            "ntext" => prefix + "NText",
            "uniqueidentifier" => prefix + "Guid",
            "datetimeoffset" => prefix + "DateTimeOffset",
            "sql_variant" => prefix + "Variant",
            "xml" => prefix + "Xml",
            "char" => prefix + "Char",
            "nchar" => prefix + "NChar",
            "hierarchyid" or "geography" or "geometry" => prefix + "Udt",
            "table type" => prefix + "Structured",
            _ => prefix + "Undefined",
        };
    }
}
