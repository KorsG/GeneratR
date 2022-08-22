using System.Collections.Generic;
using System.Linq;

namespace GeneratR.DotNet
{
    public class DotNetTemplate : Templating.StringTemplateBase
    {
        public DotNetTemplate(DotNetLanguageType dotNetLanguage)
        {
            Generator = DotNetGenerator.Create(dotNetLanguage);
        }

        public DotNetTemplate(DotNetGenerator dotNetGenerator)
        {
            Generator = dotNetGenerator;
        }

        public DotNetGenerator Generator { get; }

        public DotNetTemplate WriteNamespaceStart(string name)
        {
            WriteLine(Generator.CreateNamespaceStart(name));
            return this;
        }

        public DotNetTemplate WriteNamespaceEnd()
        {
            WriteLine(Generator.CreateNamespaceEnd());
            return this;
        }

        public DotNetTemplate WriteClassStart(DotNetModifierKeyword modifiers, string name, string inheritClass, params string[] implementInterfaces)
            => WriteClassStart(modifiers, name, inheritClass, implementInterfaces.AsEnumerable());

        public DotNetTemplate WriteClassStart(DotNetModifierKeyword modifiers, string name, string inheritClass, IEnumerable<string> implementInterfaces)
        {
            var value = Generator.CreateClassStart(name, modifiers.HasFlag(DotNetModifierKeyword.Partial), modifiers.HasFlag(DotNetModifierKeyword.Abstract), inheritClass, implementInterfaces);
            WriteLine(value);
            return this;
        }

        public DotNetTemplate WriteClassEnd()
        {
            WriteLine(Generator.CreateClassEnd());
            return this;
        }

        public DotNetTemplate WriteConstructor(DotNetModifierKeyword modifiers, string name)
        {
            WriteLine(Generator.CreateConstructor(modifiers, name));
            return this;
        }

        public DotNetTemplate WriteProperty(DotNetModifierKeyword modifiers, string name, string type, bool readOnly = false)
        {
            var value = Generator.CreateProperty(modifiers, name, type, readOnly);
            WriteLine(value);
            return this;
        }
    }
}
