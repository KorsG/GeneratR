namespace GeneratR.Database.SqlServer
{
    public class LinqToDbSqlServerGeneratorSettings : SqlServerSchemaGenerationSettings
    {
        public LinqToDbSqlServerGeneratorSettings()
        {
        }

        public bool AddLinqToDbMappingAttributes { get; set; }

        public DataConnectionSettings DataConnection { get; set; } = new DataConnectionSettings();

        public class DataConnectionSettings : CodeModelSettingsBase 
        {
            public string ClassName { get; set; }

            public string InheritClassName { get; set; }
        }
    }
}
