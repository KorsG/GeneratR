namespace GeneratR.Database.SqlServer
{
    public class LinqToDbSqlServerGeneratorSettings : SqlServerSchemaGenerationSettings
    {
        public LinqToDbSqlServerGeneratorSettings()
        {
        }

        public bool AddLinqToDbMappingAttributes { get; set; }

        public virtual DataConnectionSettings DataConnection { get; set; } = new DataConnectionSettings();

        public class DataConnectionSettings : CodeModelSettingsBase
        {
            public string ClassName { get; set; }
        }
    }
}
