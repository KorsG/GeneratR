using GeneratR.DotNet;
using System.Linq;

namespace GeneratR.Database.SqlServer.Templates
{
    public class TableTypeTemplate : DotNetTemplate
    {
        private readonly TableTypeCodeModel _model;
        private readonly DotNetGenerator _dotNet;
        private readonly SqlServerTypeMapper _typeMapper;

        public TableTypeTemplate(TableTypeCodeModel model)
            : base(model.DotNetGenerator)
        {
            _model = model;
            _dotNet = _model.DotNetGenerator;
            _typeMapper = _model.TypeMapper;
        }

        public virtual string Generate()
        {
            WriteNamespaceImports(_model.NamespaceImports);
            WriteLine();
            WriteLine(_dotNet.CreateNamespaceStart(_model.Namespace));
            using (IndentScope())
            {
                var classAsAbstract = _model.DotNetModifier.HasFlag(DotNetModifierKeyword.Abstract);
                var classAsPartial = _model.DotNetModifier.HasFlag(DotNetModifierKeyword.Partial);
                if (_model.Attributes.Any())
                {
                    Write(_model.Attributes.Build());
                }
                WriteLine(_dotNet.CreateClassStart(_model.ClassName, classAsPartial, classAsAbstract, _model.InheritClassName, _model.ImplementInterfaces));
                using (IndentScope())
                {
                    WriteLine($@"public static string SqlName => ""{_model.DbObject.FullName}"";");
                    WriteLine();

                    if (_model.AddConstructor)
                    {
                        WriteLine(_dotNet.CreateConstructor(DotNetModifierKeyword.Public, _model.ClassName));
                    }

                    foreach (var col in _model.Columns.OrderBy(x => x.DbObject.Position))
                    {
                        WriteLine();
                        WriteProperty(col);
                    }

                    foreach (var p in _model.Properties)
                    {
                        WriteLine();
                        WriteProperty(p);
                    }

                    if (_model.AddSqlDataRecordMappings)
                    {
                        WriteLine();
                        WriteLine("public SqlDataRecord ToSqlDataRecord()");
                        WriteLine("{");
                        using (IndentScope())
                        {
                            WriteLine("var record = new SqlDataRecord(_sqlColumnMetaData);");

                            foreach (var col in _model.Columns.OrderBy(x => x.DbObject.Position))
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
                            foreach (var col in _model.Columns.OrderBy(x => x.DbObject.Position))
                            {
                                var sqlDataType = _typeMapper.ConvertDataTypeToSqlDbType(col.DbObject.DataType);
                                if (_model.TypeMapper.DataTypeIsString(col.DbObject))
                                {
                                    WriteLine($@"new SqlMetaData(""{col.PropertyName}"", {sqlDataType}, {col.DbObject.Length}),");
                                }
                                else if (_model.TypeMapper.DataTypeIsDecimal(col.DbObject))
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

            return base.ToString();
        }
    }
}
