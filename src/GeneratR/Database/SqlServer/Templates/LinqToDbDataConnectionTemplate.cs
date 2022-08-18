using System.Linq;
using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class LinqToDbDataConnectionTemplate : Templating.StringTemplateBase
    {
        private readonly DotNetGenerator _dotNet;
        private readonly SqlServerSchemaCodeModels _schemaModels;

        public LinqToDbDataConnectionTemplate(LinqToDbDataConnectionCodeModel model)
        {
            Model = model;
            _dotNet = model.DotNetGenerator;
            _schemaModels = model.SchemaModels;
        }

        protected LinqToDbDataConnectionCodeModel Model { get; }

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
            WriteLine(_dotNet.CreateNamespaceStart(Model.Namespace));
            using (IndentScope())
            {
                var classAsAbstract = Model.DotNetModifier.HasFlag(DotNetModifierKeyword.Abstract);
                var classAsPartial = Model.DotNetModifier.HasFlag(DotNetModifierKeyword.Partial);
                var inheritClassName = string.IsNullOrWhiteSpace(Model.InheritClassName) ? "DataConnection" : Model.InheritClassName;
                WriteLine(_dotNet.CreateClassStart(Model.ClassName, classAsPartial, classAsAbstract, inheritClassName, Model.ImplementInterfaces));
                using (IndentScope())
                {
                    WriteConstructors();

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

        public virtual void WriteConstructors()
        {
            if (Model.AddConstructor)
            {
                WriteLine(_dotNet.CreateConstructor(DotNetModifierKeyword.Public, Model.ClassName));
            }
        }
    }
}
