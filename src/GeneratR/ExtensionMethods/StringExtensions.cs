using System;
using System.Text.RegularExpressions;

namespace GeneratR.ExtensionMethods
{
    public static class StringExtensions
    {
        /// <summary>
        /// Compares the string against a given pattern.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <param name="pattern">The pattern to match, where "*" means any sequence of characters, and "?" means any single character.</param>
        /// <returns><c>true</c> if the string matches the given pattern; otherwise <c>false</c>.</returns>
        public static bool Like(this string value, string pattern)
        {
            return new Regex(
                "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            ).IsMatch(value);
        }

        /// <summary>
        /// Ensures that a string ends with a given suffix.
        /// </summary>
        public static string EnsureEndsWith(this string value, string suffix, StringComparison comparisonType = StringComparison.Ordinal)
        {
            return value.EndsWith(suffix, comparisonType) ? value : value += suffix;
        }
    }
}
