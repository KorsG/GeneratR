using System;
using System.Text.RegularExpressions;
using GeneratR.DotNet;

namespace GeneratR.Database
{
    public abstract class GenericDbSchemaGenerator
    {
        protected static Regex RemoveRelationalColumnSuffixRegex = new Regex(@"(Id|No|Key|Code)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly GenericDbSchemaGeneratorSettings _settings;

        public GenericDbSchemaGenerator(GenericDbSchemaGeneratorSettings settings)
        {
            _settings = settings;
        }

        public DotNetGenerator DotNetGenerator => _settings.DotNetGenerator;

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
