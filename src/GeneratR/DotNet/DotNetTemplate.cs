using System.Collections.Generic;
using System.Linq;

namespace GeneratR.DotNet
{
    public class DotNetTemplate : Templating.StringTemplateBase
    {
        public DotNetTemplate(DotNetLanguageType dotNetLanguage) : this(DotNetGenerator.Create(dotNetLanguage))
        {
        }

        public DotNetTemplate(DotNetGenerator dotNetGenerator)
        {
            DotNetGenerator = dotNetGenerator;
        }

        public DotNetGenerator DotNetGenerator { get; }

        public DotNetTemplate WriteNamespaceImport(string @namespace, string alias = null)
        {
            WriteLine(DotNetGenerator.CreateNamespaceImport(@namespace, alias));
            return this;
        }

        public DotNetTemplate WriteNamespaceImports(IEnumerable<string> namespaces)
        {
            WriteLine(DotNetGenerator.CreateNamespaceImports(namespaces));
            return this;
        }

        public DotNetTemplate WriteNamespaceImport(NamespaceImportCodeModel model)
        {
            WriteLine(DotNetGenerator.CreateNamespaceImport(model));
            return this;
        }

        public DotNetTemplate WriteNamespaceImports(IEnumerable<NamespaceImportCodeModel> models)
        {
            WriteLine(DotNetGenerator.CreateNamespaceImports(models));
            return this;
        }

        public DotNetTemplate WriteNamespaceStart(string name)
        {
            WriteLine(DotNetGenerator.CreateNamespaceStart(name));
            return this;
        }

        public DotNetTemplate WriteNamespaceEnd()
        {
            WriteLine(DotNetGenerator.CreateNamespaceEnd());
            return this;
        }

        public DotNetTemplate WriteClassStart(DotNetModifierKeyword modifiers, string name, string inheritClass, params string[] implementInterfaces)
            => WriteClassStart(modifiers, name, inheritClass, implementInterfaces.AsEnumerable());

        public DotNetTemplate WriteClassStart(DotNetModifierKeyword modifiers, string name, string inheritClass, IEnumerable<string> implementInterfaces)
        {
            var value = DotNetGenerator.CreateClassStart(name, modifiers.HasFlag(DotNetModifierKeyword.Partial), modifiers.HasFlag(DotNetModifierKeyword.Abstract), inheritClass, implementInterfaces);
            WriteLine(value);
            return this;
        }

        public DotNetTemplate WriteClassEnd()
        {
            WriteLine(DotNetGenerator.CreateClassEnd());
            return this;
        }

        public DotNetTemplate WriteConstructor(DotNetModifierKeyword modifiers, string name)
        {
            WriteLine(DotNetGenerator.CreateConstructor(modifiers, name));
            return this;
        }

        public DotNetTemplate WriteProperty(DotNetModifierKeyword modifiers, string name, string type, bool readOnly = false, DotNetAttributeCollection attributes = null)
        {
            if (attributes?.Any() == true)
            {
                Write(attributes.Build());
            }
            var value = DotNetGenerator.CreateProperty(modifiers, name, type, readOnly);
            WriteLine(value);
            return this;
        }

        public DotNetTemplate WriteProperty(PropertyCodeModel model)
        {
            var value = DotNetGenerator.CreateProperty(model);
            WriteLine(value);
            return this;
        }
    }
}
