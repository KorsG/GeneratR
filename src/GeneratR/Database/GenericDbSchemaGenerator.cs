using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GeneratR.DotNet;

namespace GeneratR.Database
{
    public abstract class GenericDbSchemaGenerator
    {
        protected static Regex RemoveRelationalColumnSuffixRegex = new Regex(@"(Id|No|Key|Code)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private GenericDbSchemaGeneratorSettings _settings { get; }

        public GenericDbSchemaGenerator(DotNetGenerator dotNetGenerator, GenericDbSchemaGeneratorSettings settings)
        {
            DotNetGenerator = dotNetGenerator;
            _settings = settings;
        }

        public DotNetGenerator DotNetGenerator { get; }

        protected virtual bool ShouldGenerateDbObject(string objectName, string schemaName = "*")
        {
            var globalExcludeRegex = _settings.SchemaObjectRegexExcludes.Where(q => q.Key == "*" || q.Key == "").Select(q => q.Value).FirstOrDefault();
            var schemaExcludeRegex = _settings.SchemaObjectRegexExcludes.Where(q => q.Key.Equals(schemaName, StringComparison.OrdinalIgnoreCase)).Select(q => q.Value).SingleOrDefault();
            var globalIncludeRegex = _settings.SchemaObjectRegexIncludes.Where(q => q.Key == "*" || q.Key == "").Select(q => q.Value).FirstOrDefault();
            var schemaIncludeRegex = _settings.SchemaObjectRegexIncludes.Where(q => q.Key.Equals(schemaName, StringComparison.OrdinalIgnoreCase)).Select(q => q.Value).SingleOrDefault();

            var includeDefined = !string.IsNullOrWhiteSpace(globalIncludeRegex) || !string.IsNullOrWhiteSpace(schemaIncludeRegex);

            if (!string.IsNullOrWhiteSpace(globalExcludeRegex) && Regex.IsMatch(objectName, globalExcludeRegex))
            {
                return false;
            }
            else if (!string.IsNullOrWhiteSpace(schemaExcludeRegex) && Regex.IsMatch(objectName, schemaExcludeRegex))
            {
                return false;
            }
            else if (!string.IsNullOrWhiteSpace(globalIncludeRegex) && Regex.IsMatch(objectName, globalIncludeRegex))
            {
                return true;
            }
            else if (!string.IsNullOrWhiteSpace(schemaIncludeRegex) && Regex.IsMatch(objectName, schemaIncludeRegex))
            {
                return true;
            }
            else
            {
                if (includeDefined)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        protected virtual string RemoveRelationalColumnSuffix(string columnName)
        {
            return RemoveRelationalColumnSuffixRegex.Replace(columnName, string.Empty);
        }

        protected virtual string CreateForeignKeyCollectionTypeDotNetString(ForeignKeyCollectionType collectionType, string elementType)
        {
            // TODO: Create vb compatible method.
            switch (collectionType)
            {
                case ForeignKeyCollectionType.IEnumerable:
                    return string.Format("IEnumerable<{0}>", elementType);
                case ForeignKeyCollectionType.ICollection:
                    return string.Format("ICollection<{0}>", elementType);
                case ForeignKeyCollectionType.IList:
                    return string.Format("IList<{0}>", elementType);
                case ForeignKeyCollectionType.List:
                    return string.Format("List<{0}>", elementType);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
