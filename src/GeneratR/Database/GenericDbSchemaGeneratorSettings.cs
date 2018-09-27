using System;
using System.Collections.Generic;

namespace GeneratR.Database
{
    public class GenericDbSchemaGeneratorSettings
    {
        public GenericDbSchemaGeneratorSettings()
        {
            SchemaObjectRegexExcludes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            SchemaObjectRegexIncludes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Table = new GenericTableSettings();
            View = new GenericViewSettings();
        }

        public string ConnectionString { get; set; }

        public string ConnectionStringName { get; set; }

        public Dictionary<string, string> SchemaObjectRegexExcludes { get; }

        public Dictionary<string, string> SchemaObjectRegexIncludes { get; }

        public GenericTableSettings Table { get { return _Table; } set { _Table = value ?? new GenericTableSettings(); } }
        private GenericTableSettings _Table;

        public GenericViewSettings View { get { return _View; } set { _View = value ?? new GenericViewSettings(); } }
        private GenericViewSettings _View;
    }
}
