using System.Linq;
using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class LinqToDbDataConnectionTemplate : Templating.StringTemplateBase
    {
        private readonly DotNetGenerator _dotNet;
        private readonly SqlServerSchemaCodeModels _schemaModels;
        private readonly LinqToDbDataConnectionCodeModel _model;

        public LinqToDbDataConnectionTemplate(LinqToDbDataConnectionCodeModel model)
        {
            _model = model;
            _dotNet = model.DotNetGenerator;
            _schemaModels = model.SchemaModels;
        }

        public string Generate()
        {
            WriteLine("using System;");
            WriteLine("using System.Collections.Generic;");
            WriteLine("using System.Reflection;");
            WriteLine("using LinqToDB;");
            WriteLine("using LinqToDB.Configuration;");
            WriteLine("using LinqToDB.Data;");

            foreach (var ns in _schemaModels.GetNamespaces())
            {
                WriteLine($"using {ns};");
            }

            WriteLine();
            WriteLine(_dotNet.CreateNamespaceStart(_model.Namespace));
            using (IndentScope())
            {
                var classAsAbstract = _model.DotNetModifier.HasFlag(DotNetModifierKeyword.Abstract);
                var classAsPartial = _model.DotNetModifier.HasFlag(DotNetModifierKeyword.Partial);

                var inheritClassName = string.IsNullOrWhiteSpace(_model.InheritClassName) ? "DataConnection" : _model.InheritClassName;
                WriteLine(_dotNet.CreateClassStart(_model.ClassName, classAsPartial, classAsAbstract, inheritClassName, _model.ImplementInterfaces));
                using (IndentScope())
                {
                    if (_model.AddConstructor)
                    {
                        WriteLine(_dotNet.CreateConstructor(DotNetModifierKeyword.Public, _model.ClassName));
                    }

                    WriteLine("#region Tables");
                    foreach (var item in _schemaModels.Tables)
                    {
                        WriteLine();
                        WriteLine($"public ITable<{item.ClassName}> {item.ClassName} => GetTable<{item.ClassName}>();");
                    }
                    WriteLine();
                    WriteLine("#endregion Tables");

                    WriteLine();

                    WriteLine("#region Views");
                    foreach (var item in _schemaModels.Views)
                    {
                        WriteLine();
                        WriteLine($"public ITable<{item.ClassName}> {item.ClassName} => GetTable<{item.ClassName}>();");
                    }
                    WriteLine();
                    WriteLine("#endregion Views");

                    WriteLine();

                    WriteLine("#region TableFunctions");
                    foreach (var item in _schemaModels.TableFunctions)
                    {
                        WriteLine();

                        var args = item.Parameters.Select(x => $"{x.PropertyType} {Inflector.MakeInitialLowerCase(x.PropertyName)}");
                        WriteLine($@"[Sql.TableFunction(Schema = ""{item.DbObject.Schema}"", Name = ""{item.DbObject.Name}"")]");
                        WriteLine($"public virtual ITable<{item.ClassName}> {item.ClassName} ({string.Join(", ", args)})");
                        WriteLine("{");
                        using (IndentScope())
                        {
                            if (item.Parameters.Any())
                            {
                                var argNames = item.Parameters.Select(x => Inflector.MakeInitialLowerCase(x.PropertyName));
                                WriteLine($"return GetTable<{item.ClassName}>(this, (MethodInfo)MethodBase.GetCurrentMethod(), {string.Join(",", argNames)});");
                            }
                            else
                            {
                                WriteLine($"return GetTable<{item.ClassName}>(this, (MethodInfo)MethodBase.GetCurrentMethod());");
                            }
                        }
                        WriteLine("}");
                    }
                    WriteLine();
                    WriteLine("#endregion TableFunctions");

                }
                WriteLine(_dotNet.CreateClassEnd());
            }
            WriteLine(_dotNet.CreateNamespaceEnd());

            return TemplateBuilder.ToString();
        }
    }
}
