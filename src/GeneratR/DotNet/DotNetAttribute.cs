using System;
using System.Collections.Generic;

namespace GeneratR.DotNet
{
    public class DotNetAttribute
    {
        private readonly DotNetLanguageType _DotNetLanguage;
        private readonly List<string> args = new List<string>();

        public string Name { get; private set; }

        public string BeginTag { get; }

        public string EndTag { get; }

        public string OptionalAssignmentTag { get; }

        /// <summary>
        /// Create a DotNetAttritbute.
        /// </summary>
        /// <param name="dotNetLanguage"></param>
        /// <param name="name">Attribute name.</param>
        /// <exception cref="NotSupportedException">If <paramref name="dotNetLanguage"/> is not supported.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is null/blank/whitespace</exception>
        public DotNetAttribute(DotNetLanguageType dotNetLanguage, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentNullException(nameof(name)); }
            _DotNetLanguage = dotNetLanguage;
            if (_DotNetLanguage == DotNetLanguageType.CS)
            {
                BeginTag = "[";
                EndTag = "]";
                OptionalAssignmentTag = " = ";
            }
            else if (_DotNetLanguage == DotNetLanguageType.VB)
            {
                BeginTag = "<";
                EndTag = ">";
                OptionalAssignmentTag = "=:";
            }
            else
            {
                throw new NotSupportedException("Lang not supported: " + dotNetLanguage.ToString());
            }
            SetName(name);
        }

        /// <summary>
        /// Set the name of the attribute.
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DotNetAttribute SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentNullException(nameof(name)); }
            Name = name;
            return this;
        }

        /// <summary>
        /// Must be provided in correct order.
        /// </summary>
        public DotNetAttribute SetArg(object value)
        {
            args.Add($"{value.ToString()}");
            return this;
        }

        public DotNetAttribute SetOptionalArg(string name, object value)
        {
            if (value is string)
            {
                args.Add($"{name}{OptionalAssignmentTag}\"{value}\"");
            }
            else
            {
                args.Add($"{name}{OptionalAssignmentTag}{value}");
            }
            return this;
        }

        public override string ToString()
        {
            return ToString(true);
        }

        public string ToString(bool includeBeginAndEndTags = true)
        {
            if (includeBeginAndEndTags)
            {
                return string.Format(@"{0}{1}({2}){3}", BeginTag, Name, string.Join(", ", args), EndTag);
            }
            else
            {
                return string.Format(@"{0}({1})", Name, string.Join(", ", args));
            }
        }

    }
}
