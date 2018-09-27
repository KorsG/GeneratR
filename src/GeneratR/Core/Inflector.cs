using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GeneratR
{
    public static class Inflector
    {
        private static readonly List<Rule> _plurals = new List<Rule>();
        private static readonly List<Rule> _singulars = new List<Rule>();
        private static readonly List<string> _uncountables = new List<string>();

        /// <summary>
        /// Initializes the <see cref="Inflector"/> class.
        /// </summary>
        static Inflector()
        {
            AddPlural("$", "s");
            AddPlural("s$", "s");
            AddPlural("(ax|test)is$", "$1es");
            AddPlural("(octop|vir)us$", "$1i");
            AddPlural("(alias|status)$", "$1es");
            AddPlural("(bu)s$", "$1ses");
            AddPlural("(buffal|tomat)o$", "$1oes");
            AddPlural("([ti])um$", "$1a");
            AddPlural("sis$", "ses");
            AddPlural("(?:([^f])fe|([lr])f)$", "$1$2ves");
            AddPlural("(hive)$", "$1s");
            AddPlural("([^aeiouy]|qu)y$", "$1ies");
            AddPlural("(x|ch|ss|sh)$", "$1es");
            AddPlural("(matr|vert|ind)ix|ex$", "$1ices");
            AddPlural("([m|l])ouse$", "$1ice");
            AddPlural("^(ox)$", "$1en");
            AddPlural("(quiz)$", "$1zes");

            AddSingular("s$", string.Empty);
            AddSingular("ss$", "ss");
            AddSingular("(n)ews$", "$1ews");
            AddSingular("([ti])a$", "$1um");
            AddSingular("((a)naly|(b)a|(d)iagno|(p)arenthe|(p)rogno|(s)ynop|(t)he)ses$", "$1$2sis");
            AddSingular("(^analy)ses$", "$1sis");
            AddSingular("([^f])ves$", "$1fe");
            AddSingular("(hive)s$", "$1");
            AddSingular("(tive)s$", "$1");
            AddSingular("([lr])ves$", "$1f");
            AddSingular("([^aeiouy]|qu)ies$", "$1y");
            AddSingular("(s)eries$", "$1eries");
            AddSingular("(m)ovies$", "$1ovie");
            AddSingular("(x|ch|ss|sh)es$", "$1");
            AddSingular("([m|l])ice$", "$1ouse");
            AddSingular("(bus)es$", "$1");
            AddSingular("(o)es$", "$1");
            AddSingular("(shoe)s$", "$1");
            AddSingular("(cris|ax|test)es$", "$1is");
            AddSingular("(octop|vir)i$", "$1us");
            AddSingular("(alias|status)$", "$1");
            AddSingular("(alias|status)es$", "$1");
            AddSingular("^(ox)en", "$1");
            AddSingular("(vert|ind)ices$", "$1ex");
            AddSingular("(matr)ices$", "$1ix");
            AddSingular("(quiz)zes$", "$1");
            AddSingular("x_$", "$1");
            AddSingular("v_$", "$1");

            AddIrregular("person", "people");
            AddIrregular("man", "men");
            AddIrregular("child", "children");
            AddIrregular("sex", "sexes");
            AddIrregular("tax", "taxes");
            AddIrregular("move", "moves");
            AddIrregular("goose", "geese");
            AddIrregular("alumna", "alumnae");

            AddUncountabe("equipment");
            AddUncountabe("information");
            AddUncountabe("rice");
            AddUncountabe("money");
            AddUncountabe("species");
            AddUncountabe("series");
            AddUncountabe("fish");
            AddUncountabe("sheep");
            AddUncountabe("deer");
            AddUncountabe("aircraft");
        }

        /// <summary>
        /// Adds the irregular rule.
        /// </summary>
        /// <param name="singular">The singular.</param>
        /// <param name="plural">The plural.</param>
        private static void AddIrregular(string singular, string plural)
        {
            AddPlural(string.Concat("(", singular[0], ")", singular.Substring(1), "$"), string.Concat("$1", plural.Substring(1)));
            AddSingular(string.Concat("(", plural[0], ")", plural.Substring(1), "$"), string.Concat("$1", singular.Substring(1)));
        }

        /// <summary>
        /// Adds the uncountable rule.
        /// </summary>
        /// <param name="word">The word.</param>
        private static void AddUncountabe(string word)
        {
            _uncountables.Add(word.ToLower());
        }

        /// <summary>
        /// Adds the plural rule.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="replacement">The replacement.</param>
        private static void AddPlural(string rule, string replacement)
        {
            _plurals.Add(new Rule(rule, replacement));
        }

        /// <summary>
        /// Adds the singular rule.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="replacement">The replacement.</param>
        private static void AddSingular(string rule, string replacement)
        {
            _singulars.Add(new Rule(rule, replacement));
        }

        /// <summary>
        /// Makes the word plural.
        /// </summary>
        /// <param name="word">The word.</param>
        public static string MakePlural(string word)
        {
            return ApplyRules(_plurals, word);
        }

        /// <summary>
        /// Makes the word singular.
        /// </summary>
        /// <param name="word">The word.</param>
        public static string MakeSingular(string word)
        {
            return ApplyRules(_singulars, word);
        }

        /// <summary>
        /// Applies the rules.
        /// </summary>
        /// <param name="rules">The rules.</param>
        /// <param name="word">The word.</param>
        private static string ApplyRules(IList<Rule> rules, string word)
        {
            var result = word;
            if (!_uncountables.Contains(word.ToLower()))
            {
                for (int i = rules.Count - 1; i >= 0; i--)
                {
                    var currentPass = rules[i].Apply(word);
                    if (currentPass != null)
                    {
                        result = currentPass;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Converts the string to title case.
        /// </summary>
        /// <param name="word">The word.</param>
        public static string ToTitleCase(string word)
        {
            return Regex.Replace(ToHumanCase(AddUnderscores(word)), @"\b([a-z])",
                delegate (Match match) { return match.Captures[0].Value.ToUpper(); });
        }

        /// <summary>
        /// Converts the string to human case.
        /// </summary>
        /// <param name="lowercaseAndUnderscoredWord">The lowercase and underscored word.</param>
        public static string ToHumanCase(string lowercaseAndUnderscoredWord)
        {
            return MakeInitialCaps(Regex.Replace(lowercaseAndUnderscoredWord, @"_", " "));
        }

        /// <summary>
        /// Adds the underscores.
        /// </summary>
        /// <param name="pascalCasedWord">The pascal cased word.</param>
        public static string AddUnderscores(string pascalCasedWord)
        {
            return Regex.Replace(Regex.Replace(Regex.Replace(pascalCasedWord, @"([A-Z]+)([A-Z][a-z])", "$1_$2"), @"([a-z\d])([A-Z])", "$1_$2"), @"[-\s]", "_").ToLower();
        }

        /// <summary>
        /// Makes the initial caps.
        /// </summary>
        /// <param name="word">The word.</param>
        public static string MakeInitialCaps(string word)
        {
            return string.Concat(word.Substring(0, 1).ToUpper(), word.Substring(1).ToLower());
        }

        /// <summary>
        /// Makes the initial lower case.
        /// </summary>
        /// <param name="word">The word.</param>
        public static string MakeInitialLowerCase(string word)
        {
            return string.Concat(word.Substring(0, 1).ToLower(), word.Substring(1));
        }

        /// <summary>
        /// Determine whether the passed string is numeric, by attempting to parse it to a double
        /// </summary>
        /// <param name="str">The string to evaluated for numeric conversion</param>
        /// <returns>
        /// 	<c>true</c> if the string can be converted to a number; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsStringNumeric(string str)
        {
            return double.TryParse(str, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out double result);
        }

        /// <summary>
        /// Adds the ordinal suffix.
        /// </summary>
        /// <param name="number">The number.</param>
        public static string AddOrdinalSuffix(string number)
        {
            if (IsStringNumeric(number))
            {
                var n = int.Parse(number);
                var nMod100 = n % 100;

                if (nMod100 >= 11 && nMod100 <= 13)
                {
                    return string.Concat(number, "th");
                };

                switch (n % 10)
                {
                    case 1:
                        return string.Concat(number, "st");
                    case 2:
                        return string.Concat(number, "nd");
                    case 3:
                        return string.Concat(number, "rd");
                    default:
                        return string.Concat(number, "th");
                }
            }
            return number;
        }

        /// <summary>
        /// Converts the underscores to dashes.
        /// </summary>
        /// <param name="underscoredWord">The underscored word.</param>
        public static string ConvertUnderscoresToDashes(string underscoredWord)
        {
            return underscoredWord.Replace('_', '-');
        }

        private class Rule
        {
            public readonly Regex _regex;
            public readonly string _replacement;

            /// <summary>
            /// Initializes a new instance of the <see cref="Rule"/> class.
            /// </summary>
            /// <param name="regexPattern">The regex pattern.</param>
            /// <param name="replacementText">The replacement text.</param>
            public Rule(string regexPattern, string replacementText)
            {
                _regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
                _replacement = replacementText;
            }

            /// <summary>
            /// Applies the specified word.
            /// </summary>
            /// <param name="word">The word.</param>
            public string Apply(string word)
            {
                if (!_regex.IsMatch(word)) { return null; }

                var replaced = _regex.Replace(word, _replacement);
                if (word == word.ToUpper())
                {
                    replaced = replaced.ToUpper();
                };

                return replaced;
            }
        }
    }
}
