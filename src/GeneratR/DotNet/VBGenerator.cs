using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.DotNet
{
    public class VBGenerator : DotNetGenerator
    {
        public VBGenerator() : base(DotNetLanguageType.VB)
        {
        }

        public override string FileExtension => ".vb";

        public override string NullableOperator => "?";

        public override string CommentOperator => "'";

        public override string DocumentationOperator => "'''";

        public override string TrueValue => "True";

        public override string FalseValue => "False";

        public override string CreateClassStart(string name, bool partialClass, bool abstractClass, string inheritClass, params string[] implementInterfaces)
            => CreateClassStart(name, partialClass, abstractClass, inheritClass, implementInterfaces.AsEnumerable());

        public override string CreateClassStart(string name, bool partialClass, bool abstractClass, string inheritClass, IEnumerable<string> implementInterfaces)
        {
            throw new NotImplementedException();
        }

        public override string CreateClassEnd() => "End Class";

        public override string CreateNamespaceStart(string name) => string.Format("Namespace {0} ", name);

        public override string CreateNamespaceEnd() => "End Namespace";

        public override string CreateConstructor(DotNetModifierKeyword modifiers, string name)
        {
            throw new NotImplementedException();
        }

        public override string CreateConstructorStart(DotNetModifierKeyword modifiers, string name)
        {
            throw new NotImplementedException();
        }

        public override string CreateConstructorEnd()
        {
            throw new NotImplementedException();
        }

        public override string CreateProperty(DotNetModifierKeyword modifiers, string propertyName, string propertyTypeName, bool readOnly)
        {
            throw new NotImplementedException();
        }

        public override string CreateProperty(PropertyCodeModel codeModel)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<Type, string> TypeStringMap { get; } =
            new Dictionary<Type, string>() {
                {typeof(object), "Object"},
                {typeof(char), "Char"},
                {typeof(string), "String"},
                {typeof(byte), "Byte" },
                {typeof(byte?), "Byte?" },
                {typeof(short), "Short"},
                {typeof(short?), "Short?" },
                {typeof(int), "Integer" },
                {typeof(int?), "Integer?"},
                {typeof(long), "Long" },
                {typeof(long?), "Long?" },
                {typeof(DateTime), "DateTime"},
                {typeof(DateTime?), "DateTime?" },
                {typeof(DateTimeOffset), "DateTimeOffset" },
                {typeof(DateTimeOffset?), "DateTimeOffset?" },
                {typeof(TimeSpan), "TimeSpan" },
                {typeof(TimeSpan?), "TimeSpan?"},
                {typeof(double), "Double" },
                {typeof(double?), "Double?" },
                {typeof(float), "Float" },
                {typeof(float?), "Float?" },
                {typeof(decimal), "Decimal" },
                {typeof(decimal?), "Decimal?" },
                {typeof(bool), "Boolean" },
                {typeof(bool?), "Boolean?" },
                {typeof(byte[]), "Byte()" },
                {typeof(Guid), "Guid" },
                {typeof(Guid?), "Guid?" },
                {typeof(System.Data.DataTable), "System.Data.DataTable" },
            };

        public override Dictionary<DotNetModifierKeyword, string> DotNetModifierKeywordMap { get; } =
            new Dictionary<DotNetModifierKeyword, string>() {
                {DotNetModifierKeyword.Public, "Public" },
                {DotNetModifierKeyword.Private, "Private" },
                {DotNetModifierKeyword.Protected, "Protected" },
                {DotNetModifierKeyword.Partial, "Partial" },
                {DotNetModifierKeyword.Virtual, "Overrideable" },
                {DotNetModifierKeyword.Static, "Shared" },
                {DotNetModifierKeyword.Override, "Override" },
                {DotNetModifierKeyword.Abstract, "MustInherit" },
                {DotNetModifierKeyword.Sealed, "NotInheritable" },
                {DotNetModifierKeyword.ReadOnly, "ReadOnly"},
                {DotNetModifierKeyword.Const, "Const"},
            };

        public override string GetAsValidDotNetName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) { return string.Empty; }
            value = _regexCleanUp.Replace(value, "_");
            if (char.IsDigit(value[0]))
            {
                value = "_" + value;
            }
            if (ReservedDotNetNames.Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                value = string.Format("[{0}]", value);
            }
            return value;
        }

        public override string CreateNamespaceImports(IEnumerable<string> names)
        {
            throw new NotImplementedException();
        }

        public override string CreateNamespaceImport(string name, string alias = null)
        {
            throw new NotImplementedException();
        }

        public override string CreateNamespaceImports(IEnumerable<NamespaceImportCodeModel> models)
        {
            throw new NotImplementedException();
        }

        public override string CreateNamespaceImport(NamespaceImportCodeModel model)
        {
            throw new NotImplementedException();
        }

        public override HashSet<string> ReservedDotNetNames { get; } =
           new HashSet<string>()
           {
                "addhandler","addressof","alias","and","andalso","ansi","append","as","assembly",
                "auto","binary","boolean","byref","byte","byval","call","case","catch","cbool",
                "cbyte","cchar","cdate","cdec","cdbl","char","cint","class","clng","cobj","compare",
                "cshort","csng","cstr","ctype","date","decimal","declare","default","delegate",
                "dim","do","double","each","else","elseif","end","endif","enum","erase","error",
                "event","explicit","false","finally","for","friend","function","get","gettype",
                "goto","handles","if","implements","imports","in","inherits","input","integer",
                "interface","is","let","lib","like","lock","long","loop","me","mid","mod","module",
                "mustinherit","mustoverride","mybase","myclass","namespace","new","next","not","nothing",
                "notinheritable","notoverridable","object","off","on","option","optional","or","orelse",
                "output","overloads","overridable","overrides","paramarray","preserve","private","property",
                "protected","public","raiseevent","random","read","readonly","redim","rem","removehandler",
                "resume","return","seek","select","set","shadows","shared","short","single","static",
                "step","stop","string","structure","sub","synclock","text","then","throw","to","true",
                "try","typeof","unicode","until","variant","when","while","with","with","write","xor"
            };
    }
}
