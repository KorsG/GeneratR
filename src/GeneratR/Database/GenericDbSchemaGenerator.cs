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
