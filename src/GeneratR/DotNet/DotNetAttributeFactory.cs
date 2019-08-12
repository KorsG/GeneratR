using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneratR.DotNet
{
    /// <summary>
    /// Factory class to help build common attributes.
    /// </summary>
    public class DotNetAttributeFactory
    {
        public DotNetAttributeFactory(DotNetLanguageType languageType)
        {
            LanguageType = languageType;
        }

        public DotNetLanguageType LanguageType { get; }

        public DotNetAttribute Create(string attributeName)
        {
            return new DotNetAttribute(LanguageType, attributeName);
        }

        public DotNetAttribute CreateKeyAttribute()
        {
            return new DotNetAttribute(LanguageType, "Key");
        }

        public DotNetAttribute CreateRequiredAttribute(bool allowEmptyStrings = false, string errorMessage = "", string errorMessageResourceName = "", Type errorMessageResourceType = null)
        {
            var b = new DotNetAttribute(LanguageType, "Required");
            if (allowEmptyStrings)
            {
                b = b.SetOptionalArg("AllowEmptyStrings", true);
            }
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                b = b.SetOptionalArg("ErrorMessage", errorMessage, wrapValueInQuotes: true);
            }
            if (!string.IsNullOrWhiteSpace(errorMessageResourceName))
            {
                b = b.SetOptionalArg("ErrorMessageResourceName", errorMessageResourceName, wrapValueInQuotes: true);
            }
            if (errorMessageResourceType != null)
            {
                b = b.SetOptionalArg("ErrorMessageResourceType", errorMessageResourceType);
            }
            return b;
        }

        public DotNetAttribute CreateStringLengthAttribute(int maximumLength, int? minimumLength = null, string errorMessage = "", string errorMessageResourceName = "", Type errorMessageResourceType = null)
        {
            var b = new DotNetAttribute(LanguageType, "StringLength").SetArg(maximumLength);
            if (minimumLength != null)
            {
                b = b.SetOptionalArg("MinimumLength", minimumLength);
            }
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                b = b.SetOptionalArg("ErrorMessage", errorMessage, wrapValueInQuotes: true);
            }
            if (!string.IsNullOrWhiteSpace(errorMessageResourceName))
            {
                b = b.SetOptionalArg("ErrorMessageResourceName", errorMessageResourceName, wrapValueInQuotes: true);
            }
            if (errorMessageResourceType != null)
            {
                b = b.SetOptionalArg("ErrorMessageResourceType", errorMessageResourceType);
            }
            return b;
        }

        public DotNetAttribute CreateMaxLengthAttribute(int? length = null, string errorMessage = "", string errorMessageResourceName = "", Type errorMessageResourceType = null)
        {
            var b = new DotNetAttribute(LanguageType, "MaxLength");
            if (length != null)
            {
                b = b.SetOptionalArg("Length", length);
            }
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                b = b.SetOptionalArg("ErrorMessage", errorMessage, wrapValueInQuotes: true);
            }
            if (!string.IsNullOrWhiteSpace(errorMessageResourceName))
            {
                b = b.SetOptionalArg("ErrorMessageResourceName", errorMessageResourceName, wrapValueInQuotes: true);
            }
            if (errorMessageResourceType != null)
            {
                b = b.SetOptionalArg("ErrorMessageResourceType", errorMessageResourceType);
            }
            return b;
        }

        public DotNetAttribute CreateTableAttribute(string name, string schema = null)
        {
            var b = new DotNetAttribute(LanguageType, "Table").SetArg(name, wrapValueInQuotes: true);
            if (!string.IsNullOrWhiteSpace(schema))
            {
                b.SetOptionalArg("Schema", schema, wrapValueInQuotes: true);
            }
            return b;
        }

        public DotNetAttribute CreateDatabaseGeneratedAttribute(DatabaseGeneratedOption option)
        {
            return new DotNetAttribute(LanguageType, "DatabaseGenerated")
                .SetArg(nameof(DatabaseGeneratedOption) + "." + option.ToString());
        }

        public DotNetAttribute CreateColumnAttribute(int? order = null, string typeName = null)
        {
            var b = new DotNetAttribute(LanguageType, "Column");
            if (order != null)
            {
                b = b.SetOptionalArg("Order", order);
            }
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                b = b.SetOptionalArg("TypeName", typeName, wrapValueInQuotes: true);
            }
            return b;
        }

        public DotNetAttribute CreateColumnAttribute(string name, int? order = null, string typeName = null)
        {
            var b = new DotNetAttribute(LanguageType, "Column").SetArg(name, wrapValueInQuotes: true);
            if (order != null)
            {
                b = b.SetOptionalArg("Order", order);
            }
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                b = b.SetOptionalArg("TypeName", typeName, wrapValueInQuotes: true);
            }
            return b;
        }
    }
}
