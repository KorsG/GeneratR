using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.DotNet
{
    public class CSGenerator : DotNetGenerator
    {
        public CSGenerator() : base(DotNetLanguageType.CS)
        {
        }

        public override string FileExtension => ".cs";

        public override string NullableOperator => "?";

        public override string CommentOperator => "//";

        public override string DocumentationOperator => "///";

        public override string TrueValue => "true";

        public override string FalseValue => "false";

        public override string CreateNamespaceImports(IEnumerable<string> names) => CreateNamespaceImports(names.Select(x => new NamespaceImportCodeModel(x)));

        public override string CreateNamespaceImport(string name, string alias = null) => CreateNamespaceImport(new NamespaceImportCodeModel(name));

        public override string CreateNamespaceImports(IEnumerable<NamespaceImportCodeModel> models)
        {
            var imports = models.GroupBy(x => x.Namespace).Select(x => CreateNamespaceImport(x.First()));
            return string.Join(Environment.NewLine, imports);
        }

        public override string CreateNamespaceImport(NamespaceImportCodeModel model)
        {
            if (model.HasAlias)
            {
                return $"using {model.Alias} = {model.Namespace};";
            }
            else
            {
                return $"using {model.Namespace};";
            }
        }

        public override string CreateClassStart(string name, bool partialClass, bool abstractClass, string inheritClass, params string[] implementInterfaces)
            => CreateClassStart(name, partialClass, abstractClass, inheritClass, implementInterfaces.AsEnumerable());

        public override string CreateClassStart(string name, bool partialClass, bool abstractClass, string inheritClass, IEnumerable<string> implementInterfaces)
        {
            var value = "public ";
            if (abstractClass) { value += "abstract "; }
            if (partialClass) { value += "partial "; }

            value += "class " + name;

            var includeItems = new List<string>();
            if (!string.IsNullOrWhiteSpace(inheritClass))
            {
                includeItems.Add(inheritClass);
            }
            if (implementInterfaces != null)
            {
                includeItems.AddRange(implementInterfaces.Where(x => !string.IsNullOrWhiteSpace(x)));
            }
            if (includeItems.Any())
            {
                value += " : " + string.Join(", ", includeItems);
            }
            value += Environment.NewLine + "{";

            return value;
        }

        public override string CreateClassEnd() => "}";

        public override string CreateNamespaceStart(string name)
        {
            return string.Format("namespace {0}{1}{2}", name, Environment.NewLine, "{");
        }

        public override string CreateNamespaceEnd() => "}";

        public override string CreateConstructor(DotNetModifierKeyword modifiers, string name)
        {
            return CreateConstructorStart(modifiers, name) + CreateConstructorEnd();
        }

        public override string CreateConstructorStart(DotNetModifierKeyword modifiers, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentException(nameof(name)); }

            var sb = new StringBuilder();

            var modifierCollection = Enum.GetValues(modifiers.GetType()).Cast<DotNetModifierKeyword>().Where(x => modifiers.HasFlag(x) && Convert.ToInt64(x) != 0);
            foreach (var modifier in modifierCollection)
            {
                sb.Append(GetModifierKeywordAsString(modifier) + " ");
            }

            sb.AppendLine(name + "()");
            sb.AppendLine("{");

            return sb.ToString();
        }

        public override string CreateConstructorEnd() => "}";

        public override string CreateProperty(DotNetModifierKeyword modifiers, string propertyName, string propertyTypeName, bool readOnly)
        {
            return CreateProperty(new PropertyCodeModel()
            {
                PropertyName = propertyName,
                PropertyType = propertyTypeName,
                IsReadOnly = readOnly,
                Modifier = modifiers,
            });
        }

        public override string CreateProperty(PropertyCodeModel model)
        {
            var propertyName = model.PropertyName;
            var propertyTypeName = model.PropertyType;
            var modifiers = model.Modifier;

            if (string.IsNullOrWhiteSpace(propertyName)) { throw new ArgumentException(nameof(propertyName)); }
            if (string.IsNullOrWhiteSpace(propertyTypeName)) { throw new ArgumentException(nameof(propertyTypeName)); }

            var sb = new StringBuilder();

            if (model.XmlDocumentation.HasContents)
            {
                var xmlDoc = model.XmlDocumentation.Build(this);
                sb.AppendLine(xmlDoc);
            }

            if (model.Attributes?.Any() == true)
            {
                sb.Append(model.Attributes.Build());
            }

            var modifierCollection = Enum.GetValues(modifiers.GetType()).Cast<DotNetModifierKeyword>().Where(x => modifiers.HasFlag(x) && Convert.ToInt64(x) != 0);
            foreach (var modifier in modifierCollection)
            {
                sb.Append(GetModifierKeywordAsString(modifier) + " ");
            }

            sb.Append(propertyTypeName + " ");
            sb.Append(propertyName);

            var getterHasBody = !string.IsNullOrWhiteSpace(model.GetterBody);
            var getterAccess = GetAccessModifierKeywordAsString(model.GetterModifier);

            var setterHasBody = !string.IsNullOrWhiteSpace(model.SetterBody);
            var setterAccess = GetAccessModifierKeywordAsString(model.SetterModifier);

            var hasBody = getterHasBody || setterHasBody;

            sb.Append(" {");
            if (!string.IsNullOrWhiteSpace(getterAccess))
            {
                sb.Append($" {getterAccess}");
            }
            sb.Append(" get");
            if (hasBody)
            {
                sb.Append($" {{ {model.GetterBody} }}");
            }
            else
            {
                sb.Append(";");
            }

            if (!model.IsReadOnly)
            {
                if (!string.IsNullOrWhiteSpace(setterAccess))
                {
                    sb.Append($" {setterAccess}");
                }
                sb.Append(" set");
                if (hasBody)
                {
                    sb.Append($" {{ {model.SetterBody} }}");
                }
                else
                {
                    sb.Append(";");
                }
            }

            sb.Append(" }");

            if (!string.IsNullOrWhiteSpace(model.DefaultValue))
            {
                sb.Append($" = {model.DefaultValue};");
            }

            return sb.ToString();
        }

        public override Dictionary<Type, string> TypeStringMap { get; } =
            new Dictionary<Type, string>() {
                {typeof(object), "object"},
                {typeof(char), "char"},
                {typeof(string), "string"},
                {typeof(byte), "byte" },
                {typeof(byte?), "byte?" },
                {typeof(short), "short"},
                {typeof(short?), "short?" },
                {typeof(int), "int" },
                {typeof(int?), "int?"},
                {typeof(long), "long" },
                {typeof(long?), "long?" },
                {typeof(DateTime), "DateTime"},
                {typeof(DateTime?), "DateTime?" },
                {typeof(DateTimeOffset), "DateTimeOffset" },
                {typeof(DateTimeOffset?), "DateTimeOffset?" },
                {typeof(TimeSpan), "TimeSpan" },
                {typeof(TimeSpan?), "TimeSpan?"},
                {typeof(double), "double" },
                {typeof(double?), "double?" },
                {typeof(float), "float" },
                {typeof(float?), "float?" },
                {typeof(decimal), "decimal" },
                {typeof(decimal?), "decimal?" },
                {typeof(bool), "bool" },
                {typeof(bool?), "bool?" },
                {typeof(byte[]), "byte[]" },
                {typeof(Guid), "Guid" },
                {typeof(Guid?), "Guid?" },
                {typeof(System.Data.DataTable), "System.Data.DataTable" },
            };

        public override Dictionary<DotNetModifierKeyword, string> DotNetModifierKeywordMap { get; } =
            new Dictionary<DotNetModifierKeyword, string>() {
                {DotNetModifierKeyword.Public, "public" },
                {DotNetModifierKeyword.Private, "private" },
                {DotNetModifierKeyword.Protected, "protected" },
                {DotNetModifierKeyword.Partial, "partial" },
                {DotNetModifierKeyword.Virtual, "virtual" },
                {DotNetModifierKeyword.Static, "static" },
                {DotNetModifierKeyword.Override, "override" },
                {DotNetModifierKeyword.Abstract, "abstract" },
                {DotNetModifierKeyword.Sealed, "sealed" },
                {DotNetModifierKeyword.ReadOnly, "readonly"},
                {DotNetModifierKeyword.Const, "const"},
                {DotNetModifierKeyword.Internal, "internal" },
            };

        public override string GetAsValidDotNetName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) { return string.Empty; }
            value = _regexCleanUp.Replace(value, "_");
            if (char.IsDigit(value[0]))
            {
                value = "_" + value;
            }
            if (ReservedDotNetNames.Contains(value, StringComparer.Ordinal))
            {
                value = string.Format("@{0}", value);
            }
            return value;
        }

        public override HashSet<string> ReservedDotNetNames { get; } =
            new HashSet<string>() {
                "abstract", "event", "new", "struct", "as", "explicit", "null",
                "switch", "base", "extern", "object", "this", "bool", "false", "operator", "throw",
                "break", "finally", "out", "true", "byte", "fixed", "override", "try", "case", "float",
                "params", "typeof", "catch", "for", "private", "uint", "char", "foreach", "protected",
                "ulong", "checked", "goto", "public", "unchecked", "class", "if", "readonly", "unsafe",
                "const", "implicit", "ref", "ushort", "continue", "in", "return", "using", "decimal",
                "int", "sbyte", "virtual", "default", "interface", "sealed", "volatile", "delegate",
                "internal", "short", "void", "do", "is", "sizeof", "while", "double", "lock",
                "stackalloc", "else", "long", "static", "enum", "namespace", "string"
            };

    }
}
