using System.Collections.Generic;
using System.Linq;
using GeneratR.DotNet;

namespace GeneratR.Database.SqlServer
{
    public class LinqToDbDataConnectionTemplate
    {
        private readonly SqlServerSchemaCodeModels _schemaModels;
        private readonly LinqToDbTypeMapper _typeMapper;
        private readonly DotNetGenerator _dotNetGenerator;

        public LinqToDbDataConnectionTemplate(LinqToDbDataConnectionCodeModel model, LinqToDbTypeMapper typeMapper, DotNetGenerator dotNetGenerator)
        {
            Model = model;
            _typeMapper = typeMapper;
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
                        t.WriteLine($"public ITable<{item.ClassName}> {item.ClassName} => this.GetTable<{item.ClassName}>();");
                    }
                    t.WriteLine();
                    t.WriteLine("#endregion Tables");

                    t.WriteLine();

                    t.WriteLine("#region Views");
                    foreach (var item in _schemaModels.Views)
                    {
                        t.WriteLine();
                        t.WriteLine($"public ITable<{item.ClassName}> {item.ClassName} => this.GetTable<{item.ClassName}>();");
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
                                t.WriteLine($"return this.GetTable<{item.ClassName}>(this, (MethodInfo)MethodBase.GetCurrentMethod(), {string.Join(",", argNames)});");
                            }
                            else
                            {
                                t.WriteLine($"return this.GetTable<{item.ClassName}>(this, (MethodInfo)MethodBase.GetCurrentMethod());");
                            }
                        }
                        t.WriteLine("}");
                    }
                    t.WriteLine();
                    t.WriteLine("#endregion TableFunctions");

                    t.WriteLine();

                    t.WriteLine("#region StoredProcedures");
                    foreach (var item in _schemaModels.StoredProcedures)
                    {
                        t.WriteLine();

                        var methodName = item.ClassName + "Async";
                        var methodArgs = item.Parameters.Select(x => $"{x.PropertyType} {Inflector.MakeInitialLowerCase(x.PropertyName)}").ToList();
                        methodArgs.Add("CancellationToken cancellationToken = default");

                        t.WriteLine($"public async Task<{item.ClassName}> {methodName} ({string.Join(", ", methodArgs)})");
                        t.WriteLine("{");
                        using (t.IndentScope())
                        {
                            // TODO: Should map potential returned table and/or output parameters.
                            // this.QueryProcAsync/this.QueryProcMultipleAsync
                            // if (item.ResultColumns.Any())) { } etc...

                            t.WriteLine(@"var returnParam = new DataParameter() { Name = ""ReturnValue"", Direction = System.Data.ParameterDirection.ReturnValue, };");

                            var procArgs = new List<string>() {
                                "cancellationToken",
                                "returnParam",
                            };

                            if (item.Parameters.Any())
                            {
                                foreach (var param in item.Parameters)
                                {
                                    var valueArg = Inflector.MakeInitialLowerCase(param.PropertyName);
                                    procArgs.Add(_typeMapper.GetLinqToDbDataParameter(param, valueArg));
                                }
                            }

                            t.WriteLine(@$"await this.ExecuteProcAsync(""{item.DbObject.FullName}"", {string.Join(", ", procArgs)}).ConfigureAwait(false);");

                            t.WriteLine($"var result = new {item.ClassName}()");
                            t.WriteLine("{");
                            using (t.IndentScope())
                            {
                                t.WriteLine(@$"ReturnValue = ((int?)returnParam.Output?.Value) ?? 0,");
                            }
                            t.WriteLine("};");
                            t.WriteLine("return result;");
                        }
                        t.WriteLine("}");
                    }
                    t.WriteLine();
                    t.WriteLine("#endregion StoredProcedures");

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
