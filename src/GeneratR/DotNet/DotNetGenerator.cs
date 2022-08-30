using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace GeneratR.DotNet
{
    public abstract class DotNetGenerator
    {
        protected const string NotImplementedForDotNetLanguageMessage = @"Method not implemented for current .NET Language";
        protected readonly Regex _regexCleanUp = new Regex(@"[^\w\d_]", RegexOptions.Compiled);

        private DotNetGenerator()
        {
        }

        protected DotNetGenerator(DotNetLanguageType DotNetLanguageType)
        {
            _DotNetLanguageType = DotNetLanguageType;
            _attributeFactory = new DotNetAttributeFactory(DotNetLanguageType);
        }

        /// <summary>
        /// Create DotNet generator.
        /// </summary>
        /// <param name="DotNetLanguageType"></param>
        public static DotNetGenerator Create(DotNetLanguageType DotNetLanguageType)
        {
            switch (DotNetLanguageType)
            {
                case DotNetLanguageType.CS:
                    return new CSGenerator();
                case DotNetLanguageType.VB:
                    return new VBGenerator();
                default:
                    throw new NotSupportedException(NotImplementedForDotNetLanguageMessage);
            }
        }

        public DotNetLanguageType DotNetLanguage { get { return _DotNetLanguageType; } }

        public DotNetAttributeFactory AttributeFactory { get { return _attributeFactory; } }

        /// <summary>
        /// Get the DotNetLanguage file extension. 
        /// </summary>
        public abstract string FileExtension { get; }

        public abstract string NullableOperator { get; }

        public abstract string CommentOperator { get; }

        /// <summary>
        /// Get boolean "true" as string.
        /// </summary>
        public abstract string TrueValue { get; }

        /// <summary>
        /// Get boolean "false" as string.
        /// </summary>
        public abstract string FalseValue { get; }

        public abstract string CreateImportNamespace(string name);
        public abstract string CreateImportNamespaces(IEnumerable<string> names);

        /// <summary> 
        /// This method creates the signature for the class in provided DotNet syntax.
        /// </summary>
        public abstract string CreateClassStart(string name, bool partialClass, bool abstractClass, string inheritClass, params string[] implementInterfaces);
        public abstract string CreateClassStart(string name, bool partialClass, bool abstractClass, string inheritClass, IEnumerable<string> implementInterfaces);
        public abstract string CreateClassEnd();

        public abstract string CreateNamespaceStart(string name);
        public abstract string CreateNamespaceEnd();

        public abstract string CreateConstructor(DotNetModifierKeyword modifiers, string name);
        public abstract string CreateConstructorStart(DotNetModifierKeyword modifiers, string name);
        public abstract string CreateConstructorEnd();

        public abstract string CreateProperty(DotNetModifierKeyword modifiers, string propertyName, string propertyTypeName, bool readOnly);
        public abstract string CreateProperty(PropertyCodeModel codeModel);

        #region Type names ToString.

        /// <summary>
        /// Get Type's string representation.
        /// </summary>
        /// <param name="t"></param>
        public string GetTypeAsString(Type t)
        {
            if (TypeStringMap.TryGetValue(t, out string typeString))
            {
                return typeString;
            }
            else
            {
                Debug.WriteLine(string.Format("No type mapping exists for Type: '{0}' - returning best guess.", t.Name));
                // Return best guess
                var underlyingType = Nullable.GetUnderlyingType(t.UnderlyingSystemType);
                if (underlyingType == null)
                {
                    return t.Name;
                }
                else
                {
                    return underlyingType.Name + NullableOperator;
                }
            }
        }

        public abstract Dictionary<Type, string> TypeStringMap { get; }

        #endregion

        #region DotNet Keywords ToString

        /// <summary>
        /// Get as string
        /// </summary>
        /// <param name="kw"></param>
        /// <exception cref="System.NotSupportedException"></exception>
        public string GetModifierKeywordAsString(DotNetModifierKeyword kw)
        {
            if (kw == DotNetModifierKeyword.None) { return string.Empty; }

            if (DotNetModifierKeywordMap.TryGetValue(kw, out var keywordString))
            {
                return keywordString;
            }
            else
            {
                throw new NotSupportedException(string.Format("No mapping exists for Keyword: '{0}'", kw.ToString()));
            }
        }

        /// <summary>
        /// Get as string
        /// </summary>
        /// <param name="kw"></param>
        /// <exception cref="NotSupportedException"></exception>
        public string GetAccessModifierKeywordAsString(DotNetAccessModifierKeyword kw)
        {
            if (kw == DotNetAccessModifierKeyword.None) { return string.Empty; }

            var accessModifiers = Enum.GetValues(kw.GetType()).Cast<DotNetAccessModifierKeyword>().Where(x => x != 0 && kw.HasFlag(x));

            var accessModifierStrings = new List<string>();

            foreach (var am in accessModifiers)
            {
                var modifier = am switch
                {
                    DotNetAccessModifierKeyword.Public => DotNetModifierKeyword.Public,
                    DotNetAccessModifierKeyword.Private => DotNetModifierKeyword.Private,
                    DotNetAccessModifierKeyword.Internal => DotNetModifierKeyword.Internal,
                    DotNetAccessModifierKeyword.Protected => DotNetModifierKeyword.Protected,
                    DotNetAccessModifierKeyword.None => DotNetModifierKeyword.None,
                    _ => throw new NotSupportedException(string.Format("No mapping exists for Keyword: '{0}'", kw.ToString())),
                };

                if (DotNetModifierKeywordMap.TryGetValue(modifier, out var keywordString))
                {
                    accessModifierStrings.Add(keywordString);
                }
                else
                {
                    throw new NotSupportedException(string.Format("No mapping exists for Keyword: '{0}'", kw.ToString()));
                }
            }

            return string.Join(" ", accessModifierStrings);
        }

        public abstract Dictionary<DotNetModifierKeyword, string> DotNetModifierKeywordMap { get; }

        #endregion

        #region Reserved DotNet names

        /// <summary>
        /// Parse string to valid DotNetName. 
        /// </summary>
        /// <param name="value"></param>
        public abstract string GetAsValidDotNetName(string value);

        public abstract HashSet<string> ReservedDotNetNames { get; }

        #endregion

        #region Private members

        private readonly DotNetLanguageType _DotNetLanguageType;
        private readonly DotNetAttributeFactory _attributeFactory;

        #endregion

    }
}
