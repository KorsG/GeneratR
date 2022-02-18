using GeneratR.DotNet;
using GeneratR.Templating;
using System.Linq;

namespace GeneratR.Database.SqlServer.Templates
{
    public class TableTypeTemplate : StringTemplateBase
    {
        private readonly SqlServerTableTypeConfiguration _config;
        private readonly DotNetGenerator _dotNet;
        private readonly SqlServerTypeMapper _typeMapper;

        public TableTypeTemplate(SqlServerTableTypeConfiguration config)
        {
            _config = config;
            _dotNet = _config.DotNetGenerator;
            _typeMapper = _config.TypeMapper;
        }

        public virtual string Generate()
        {
            WriteLine("using System;");
            WriteLine("using System.Collections.Generic;");
            if (_config.AddDataAnnotationAttributes)
            {
                WriteLine("using System.ComponentModel.DataAnnotations;");
                WriteLine("using System.ComponentModel.DataAnnotations.Schema;");
            }

            if (_config.AddSqlDataRecordMappings)
            {
                WriteLine("using System.Data;");
                WriteLine("using Microsoft.SqlServer.Server;");
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
                    WriteLine($@"public static string SqlName => ""{_config.DbObject.FullName}"";");
                    WriteLine();

                    if (_config.AddConstructor)
                    {
                        WriteLine(_dotNet.CreateConstructor(DotNetModifierKeyword.Public, _config.ClassName));
                    }

                    foreach (var col in _config.Columns.OrderBy(x => x.DbObject.Position))
                    {
                        WriteLine();
                        if (!string.IsNullOrWhiteSpace(col.DbObject.Description))
                        {
                            WriteLine($@"/// <summary>{col.DbObject.Description}</summary>");
                        }

                        if (col.Attributes.Any())
                        {
                            Write(col.Attributes.ToMultilineString());
                        }
                        WriteLine(_dotNet.CreateProperty(col.DotNetModifier, col.PropertyName, col.PropertyType, false));
                    }
                    WriteLine();

                    if (_config.AddSqlDataRecordMappings)
                    {
                        WriteLine("public SqlDataRecord ToSqlDataRecord()");
                        WriteLine("{");
                        using (IndentScope())
                        {
                            WriteLine("var record = new SqlDataRecord(_sqlColumnMetaData);");

                            foreach (var col in _config.Columns.OrderBy(x => x.DbObject.Position))
                            {
                                WriteLine($@"record.SetValue({col.DbObject.Position - 1}, {col.PropertyName});");
                            }

                            WriteLine("return record;");
                        }
                        WriteLine("}");
                        WriteLine();

                        WriteLine("private static readonly SqlMetaData[] _sqlColumnMetaData = new[]");
                        WriteLine("{");
                        using (IndentScope())
                        {
                            foreach (var col in _config.Columns.OrderBy(x => x.DbObject.Position))
                            {
                                var sqlDataType = _typeMapper.ConvertDataTypeToSqlDbType(col.DbObject.DataType);
                                if (_config.TypeMapper.DataTypeIsString(col.DbObject))
                                {
                                    WriteLine($@"new SqlMetaData(""{col.PropertyName}"", {sqlDataType}, {col.DbObject.Length}),");
                                }
                                else if (_config.TypeMapper.DataTypeIsDecimal(col.DbObject))
                                {
                                    WriteLine($@"new SqlMetaData(""{col.PropertyName}"", {sqlDataType}, {col.DbObject.Precision}, {col.DbObject.Scale}),");
                                }
                                else
                                {
                                    WriteLine($@"new SqlMetaData(""{col.PropertyName}"", {sqlDataType}),");
                                }
                            }
                        }
                        WriteLine("};");
                    }
                }
                WriteLine(_dotNet.CreateClassEnd());
            }
            WriteLine(_dotNet.CreateNamespaceEnd());

            return TemplateBuilder.ToString();
        }
    }
}
