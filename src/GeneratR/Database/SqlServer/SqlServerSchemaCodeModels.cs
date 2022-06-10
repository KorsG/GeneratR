using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerSchemaCodeModels
    {
        public SqlServerSchemaCodeModels()
        {
            Tables = new List<SqlServerTableCodeModel>();
            Views = new List<SqlServerViewCodeModel>();
            TableFunctions = new List<SqlServerTableFunctionCodeModel>();
            StoredProcedures = new List<SqlServerStoredProcedureCodeModel>();
            TableTypes = new List<SqlServerTableTypeCodeModel>();
        }

        public List<SqlServerTableCodeModel> Tables { get; }

        public List<SqlServerViewCodeModel> Views { get; }

        public List<SqlServerTableFunctionCodeModel> TableFunctions { get; }

        public List<SqlServerStoredProcedureCodeModel> StoredProcedures { get; }

        public List<SqlServerTableTypeCodeModel> TableTypes { get; }

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

        public SqlServerTableCodeModel GetTable(string schema, string name)
        {
            return Tables?.FirstOrDefault(x => x.DbObject.Schema == schema && x.DbObject.Name == name);
        }

        public SqlServerTableCodeModel GetTable(string fullName)
        {
            return Tables?.FirstOrDefault(x => x.DbObject.FullName == fullName);
        }

        public SqlServerSchemaCodeModels WithTable(string fullName, Action<SqlServerTableCodeModel> action)
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

        public SqlServerSchemaCodeModels WithTable(string schema, string name, Action<SqlServerTableCodeModel> action)
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

        public SqlServerViewCodeModel GetView(string fullName)
        {
            return Views?.FirstOrDefault(x => x.DbObject.FullName == fullName);
        }

        public SqlServerSchemaCodeModels WithView(string fullName, Action<SqlServerViewCodeModel> action)
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

        public SqlServerTableFunctionCodeModel GetTableFunction(string fullName)
        {
            return TableFunctions?.FirstOrDefault(x => x.DbObject.FullName == fullName);
        }

        public SqlServerSchemaCodeModels WithTableFunction(string fullName, Action<SqlServerTableFunctionCodeModel> action)
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

        public SqlServerStoredProcedureCodeModel GetStoredProcedure(string fullName)
        {
            return StoredProcedures?.FirstOrDefault(x => x.DbObject.FullName == fullName);
        }

        public SqlServerSchemaCodeModels WithStoredProcedure(string fullName, Action<SqlServerStoredProcedureCodeModel> action)
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

        public SqlServerTableTypeCodeModel GetTableType(string fullName)
        {
            return TableTypes?.FirstOrDefault(x => x.DbObject.FullName == fullName);
        }

        public SqlServerSchemaCodeModels WithTableType(string fullName, Action<SqlServerTableTypeCodeModel> action)
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
