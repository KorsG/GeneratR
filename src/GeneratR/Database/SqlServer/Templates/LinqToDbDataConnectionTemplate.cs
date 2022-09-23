using System.Linq;
using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class LinqToDbDataConnectionTemplate
    {
        private readonly SqlServerSchemaCodeModels _schemaModels;
        private readonly DotNetGenerator _dotNetGenerator;

        public LinqToDbDataConnectionTemplate(LinqToDbDataConnectionCodeModel model, DotNetGenerator dotNetGenerator)
        {
            Model = model;
            _dotNetGenerator = dotNetGenerator;
            _schemaModels = model.SchemaModels;
        }

        protected LinqToDbDataConnectionCodeModel Model { get; }

        public string Generate()
        {
            var t = new DotNetTemplate(_dotNetGenerator);

            t.WriteNamespaceImports(Model.NamespaceImports);
            t.WriteLine();

            t.WriteNamespaceStart(Model.Namespace);
            using (t.IndentScope())
            {
                var inheritClassName = string.IsNullOrWhiteSpace(Model.InheritClassName) ? "DataConnection" : Model.InheritClassName;
                t.WriteClassStart(Model.DotNetModifier, Model.ClassName, inheritClassName, Model.ImplementInterfaces);
                using (t.IndentScope())
                {
                    WriteConstructors(t);

                    t.WriteLine("#region Tables");
                    foreach (var item in _schemaModels.Tables)
                    {
                        t.WriteLine();
                        t.WriteLine($"public ITable<{item.ClassName}> {item.ClassName} => GetTable<{item.ClassName}>();");
                    }
                    t.WriteLine();
                    t.WriteLine("#endregion Tables");

                    t.WriteLine();

                    t.WriteLine("#region Views");
                    foreach (var item in _schemaModels.Views)
                    {
                        t.WriteLine();
                        t.WriteLine($"public ITable<{item.ClassName}> {item.ClassName} => GetTable<{item.ClassName}>();");
                    }
                    t.WriteLine();
                    t.WriteLine("#endregion Views");

                    t.WriteLine();

                    t.WriteLine("#region TableFunctions");
                    foreach (var item in _schemaModels.TableFunctions)
                    {
                        t.WriteLine();

                        var args = item.Parameters.Select(x => $"{x.PropertyType} {Inflector.MakeInitialLowerCase(x.PropertyName)}");
                        t.WriteLine($@"[Sql.TableFunction(Schema = ""{item.DbObject.Schema}"", Name = ""{item.DbObject.Name}"")]");
                        t.WriteLine($"public virtual ITable<{item.ClassName}> {item.ClassName} ({string.Join(", ", args)})");
                        t.WriteLine("{");
                        using (t.IndentScope())
                        {
                            if (item.Parameters.Any())
                            {
                                var argNames = item.Parameters.Select(x => Inflector.MakeInitialLowerCase(x.PropertyName));
                                t.WriteLine($"return GetTable<{item.ClassName}>(this, (MethodInfo)MethodBase.GetCurrentMethod(), {string.Join(",", argNames)});");
                            }
                            else
                            {
                                t.WriteLine($"return GetTable<{item.ClassName}>(this, (MethodInfo)MethodBase.GetCurrentMethod());");
                            }
                        }
                        t.WriteLine("}");
                    }
                    t.WriteLine();
                    t.WriteLine("#endregion TableFunctions");

                    foreach (var p in Model.Properties)
                    {
                        t.WriteLine();
                        t.WriteProperty(p);
                    }
                }
                t.WriteClassEnd();
            }
            t.WriteNamespaceEnd();

            return t.ToString();
        }

        public virtual void WriteConstructors(DotNetTemplate writer)
        {
            if (Model.AddConstructor)
            {
                writer.WriteConstructor(DotNetModifierKeyword.Public, Model.ClassName);
            }
        }
    }
}
