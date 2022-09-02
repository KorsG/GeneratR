using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerSchemaCodeModels
    {
        public SqlServerSchemaCodeModels()
        {
            Tables = new List<TableCodeModel>();
            Views = new List<ViewCodeModel>();
            TableFunctions = new List<TableFunctionCodeModel>();
            StoredProcedures = new List<StoredProcedureCodeModel>();
            TableTypes = new List<TableTypeCodeModel>();
        }

        public List<TableCodeModel> Tables { get; }

        public List<ViewCodeModel> Views { get; }

        public List<TableFunctionCodeModel> TableFunctions { get; }

        public List<StoredProcedureCodeModel> StoredProcedures { get; }

        public List<TableTypeCodeModel> TableTypes { get; }

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

        public TableCodeModel GetTable(string schema, string name)
        {
            return Tables.FirstOrDefault(x => x.DbObject.Schema == schema && x.DbObject.Name == name);
        }

        public TableCodeModel GetTable(string fullName)
        {
            return Tables.FirstOrDefault(x => x.DbObject.FullName == fullName);
        }

        public SqlServerSchemaCodeModels WithTable(string fullName, Action<TableCodeModel> action)
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

        public SqlServerSchemaCodeModels WithTable(string schema, string name, Action<TableCodeModel> action)
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

        public ViewCodeModel GetView(string fullName)
        {
            return Views.FirstOrDefault(x => x.DbObject.FullName == fullName);
        }

        public SqlServerSchemaCodeModels WithView(string fullName, Action<ViewCodeModel> action)
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

        public TableFunctionCodeModel GetTableFunction(string fullName)
        {
            return TableFunctions.FirstOrDefault(x => x.DbObject.FullName == fullName);
        }

        public SqlServerSchemaCodeModels WithTableFunction(string fullName, Action<TableFunctionCodeModel> action)
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

        public StoredProcedureCodeModel GetStoredProcedure(string fullName)
        {
            return StoredProcedures.FirstOrDefault(x => x.DbObject.FullName == fullName);
        }

        public SqlServerSchemaCodeModels WithStoredProcedure(string fullName, Action<StoredProcedureCodeModel> action)
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

        public TableTypeCodeModel GetTableType(string fullName)
        {
            return TableTypes.FirstOrDefault(x => x.DbObject.FullName == fullName);
        }

        public SqlServerSchemaCodeModels WithTableType(string fullName, Action<TableTypeCodeModel> action)
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
