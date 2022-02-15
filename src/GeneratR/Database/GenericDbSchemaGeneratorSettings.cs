using GeneratR.DotNet;
using System;
using System.Collections.Generic;

namespace GeneratR.Database
{
    public class GenericDbSchemaGeneratorSettings
    {
        private GenericTableSettings _table;
        private GenericViewSettings _view;

        public GenericDbSchemaGeneratorSettings(DotNetGenerator dotNetGenerator)
        {
            DotNetGenerator = dotNetGenerator;
            Table = new GenericTableSettings();
            View = new GenericViewSettings();
        }

        public DotNetGenerator DotNetGenerator { get; }

        /// <summary>
        /// Inherited from ConnectionString if not set.
        /// </summary>
        public string DatabaseName { get; set; }

        public string ConnectionString { get; set; }
        
        [Obsolete("Not supported")]
        public string ConnectionStringName { get; set; }

        public ICollection<string> IncludeSchemas { get; set; } = new List<string>();

        public ICollection<string> ExcludeSchemas { get; set; } = new List<string>();

        public ICollection<string> IncludeObjects { get; set; } = new List<string>();

        public ICollection<string> ExcludeObjects { get; set; } = new List<string>();

        public GenericTableSettings Table { get => _table; set { _table = value ?? new GenericTableSettings(); } }

        public GenericViewSettings View { get => _view; set { _view = value ?? new GenericViewSettings(); } }
    }
}
