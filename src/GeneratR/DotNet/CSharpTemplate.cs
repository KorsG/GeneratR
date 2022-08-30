using System.Collections.Generic;
using System.Linq;

namespace GeneratR.DotNet
{
    public class CSharpTemplate : Templating.StringTemplateBase
    {
        public CSharpTemplate(DotNetLanguageType dotNetLanguage) : this(DotNetGenerator.Create(dotNetLanguage))
        {
        }

        public CSharpTemplate(DotNetGenerator dotNetGenerator)
        {
            Generator = dotNetGenerator;
        }

        public DotNetGenerator Generator { get; }

        public CSharpTemplate WriteUsing(string @namespace)
        {
            WriteLine(Generator.CreateImportNamespace(@namespace));
            return this;
        }

        public CSharpTemplate WriteUsings(IEnumerable<string> namespaces)
        {
            WriteLine(Generator.CreateImportNamespaces(namespaces));
            return this;
        }

        public CSharpTemplate WriteNamespaceStart(string name)
        {
            WriteLine(Generator.CreateNamespaceStart(name));
            return this;
        }

        public CSharpTemplate WriteNamespaceEnd()
        {
            WriteLine(Generator.CreateNamespaceEnd());
            return this;
        }

        public CSharpTemplate WriteClassStart(DotNetModifierKeyword modifiers, string name, string inheritClass, params string[] implementInterfaces)
            => WriteClassStart(modifiers, name, inheritClass, implementInterfaces.AsEnumerable());

        public CSharpTemplate WriteClassStart(DotNetModifierKeyword modifiers, string name, string inheritClass, IEnumerable<string> implementInterfaces)
        {
            var value = Generator.CreateClassStart(name, modifiers.HasFlag(DotNetModifierKeyword.Partial), modifiers.HasFlag(DotNetModifierKeyword.Abstract), inheritClass, implementInterfaces);
            WriteLine(value);
            return this;
        }

        public CSharpTemplate WriteClassEnd()
        {
            WriteLine(Generator.CreateClassEnd());
            return this;
        }

        public CSharpTemplate WriteConstructor(DotNetModifierKeyword modifiers, string name)
        {
            WriteLine(Generator.CreateConstructor(modifiers, name));
            return this;
        }

        public CSharpTemplate WriteProperty(DotNetModifierKeyword modifiers, string name, string type, bool readOnly = false, DotNetAttributeCollection attributes = null)
        {
            if (attributes?.Any() == true)
            {
                Write(attributes.ToMultilineString());
            }
            var value = Generator.CreateProperty(modifiers, name, type, readOnly);
            WriteLine(value);
            return this;
        }
    }
}
