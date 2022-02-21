using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerDbSchema
    {
        public SqlServerDbSchema()
        {
            Tables = new List<SqlServerTableConfiguration>();
            Views = new List<SqlServerViewConfiguration>();
            TableFunctions = new List<SqlServerTableFunctionConfiguration>();
            StoredProcedures = new List<SqlServerStoredProcedureConfiguration>();
            TableTypes = new List<SqlServerTableTypeConfiguration>();
        }

        public List<SqlServerTableConfiguration> Tables { get; }

        public List<SqlServerViewConfiguration> Views { get; }

        public List<SqlServerTableFunctionConfiguration> TableFunctions { get; }

        public List<SqlServerStoredProcedureConfiguration> StoredProcedures { get; }

        public List<SqlServerTableTypeConfiguration> TableTypes { get; }

        /// <summary>
        /// Get distinct collection of Schema names from currently loaded collections.
        /// </summary>
        public IEnumerable<string> GetSchemaNames()
        {
            return Tables.Select(q => q.DbObject.Schema)
                .Union(Views.Select(q => q.DbObject.Schema))
                .Union(TableFunctions.Select(q => q.DbObject.Schema))
                .Union(StoredProcedures.Select(q => q.DbObject.Schema))
                .Union(TableTypes.Select(q => q.DbObject.Schema))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Get distinct collection of namespaces from currently loaded collections.
        /// </summary>
        public IEnumerable<string> GetNamespaces()
        {
            return Tables.Select(q => q.Namespace)
                .Union(Views.Select(q => q.Namespace))
                .Union(TableFunctions.Select(q => q.Namespace))
                .Union(StoredProcedures.Select(q => q.Namespace))
                .Union(TableTypes.Select(q => q.Namespace))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public SqlServerTableConfiguration GetTable(string schema, string name)
        {
            return Tables?.FirstOrDefault(x => x.DbObject.Schema == schema && x.DbObject.Name == name);
        }

        public SqlServerTableConfiguration GetTable(string fullName)
        {
            return Tables?.FirstOrDefault(x => x.DbObject.FullName == fullName);
        }

        public SqlServerDbSchema WithTable(string fullName, Action<SqlServerTableConfiguration> action)
        {
            if (action != null)
            {
                var obj = GetTable(fullName);
                if (obj != null)
                {
                    action(obj);
                }
            }
            return this;
        }

        public SqlServerDbSchema WithTable(string schema, string name, Action<SqlServerTableConfiguration> action)
        {
            if (action != null)
            {
                var obj = GetTable(schema, name);
                if (obj != null)
                {
                    action(obj);
                }
            }
            return this;
        }

        public SqlServerViewConfiguration GetView(string fullName)
        {
            return Views?.FirstOrDefault(x => x.DbObject.FullName == fullName);
        }

        public SqlServerDbSchema WithView(string fullName, Action<SqlServerViewConfiguration> action)
        {
            if (action != null)
            {
                var obj = GetView(fullName);
                if (obj != null)
                {
                    action(obj);
                }
            }
            return this;
        }

        public SqlServerTableFunctionConfiguration GetTableFunction(string fullName)
        {
            return TableFunctions?.FirstOrDefault(x => x.DbObject.FullName == fullName);
        }

        public SqlServerDbSchema WithTableFunction(string fullName, Action<SqlServerTableFunctionConfiguration> action)
        {
            if (action != null)
            {
                var obj = GetTableFunction(fullName);
                if (obj != null)
                {
                    action(obj);
                }
            }
            return this;
        }

        public SqlServerStoredProcedureConfiguration GetStoredProcedure(string fullName)
        {
            return StoredProcedures?.FirstOrDefault(x => x.DbObject.FullName == fullName);
        }

        public SqlServerDbSchema WithStoredProcedure(string fullName, Action<SqlServerStoredProcedureConfiguration> action)
        {
            if (action != null)
            {
                var obj = GetStoredProcedure(fullName);
                if (obj != null)
                {
                    action(obj);
                }
            }
            return this;
        }

        public SqlServerTableTypeConfiguration GetTableType(string fullName)
        {
            return TableTypes?.FirstOrDefault(x => x.DbObject.FullName == fullName);
        }

        public SqlServerDbSchema WithTableType(string fullName, Action<SqlServerTableTypeConfiguration> action)
        {
            if (action != null)
            {
                var obj = GetTableType(fullName);
                if (obj != null)
                {
                    action(obj);
                }
            }
            return this;
        }
    }
}
