using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratR.Database.SqlServer
{
    public class SqlServerTableCodeModel : DbObjectClassCodeModel<Schema.Table>
    {
        public SqlServerTableCodeModel(Schema.Table dbObject, DotNet.DotNetGenerator dotNetGenerator, SqlServerTypeMapper typeMapper)
            : base(dbObject, dotNetGenerator)
        {
            Columns = new List<SqlServerColumnCodeModel>();
            ForeignKeys = new List<SqlServerForeignKeyCodeModel>();
            ReferencingForeignKeys = new List<SqlServerForeignKeyCodeModel>();
            TypeMapper = typeMapper;
        }

        public SqlServerTypeMapper TypeMapper { get; }

        public List<SqlServerColumnCodeModel> Columns { get; set; }

        public bool GenerateForeignKeys { get; set; }

        public List<SqlServerForeignKeyCodeModel> ForeignKeys { get; set; }

        public List<SqlServerForeignKeyCodeModel> ReferencingForeignKeys { get; set; }

        public bool GenerateReferencingForeignKeys { get; set; }

        public SqlServerColumnCodeModel GetColumn(string name)
        {
            return Columns?.FirstOrDefault(x => x.DbObject.Name == name);
        }

        public SqlServerTableCodeModel WithColumn(string name, Action<SqlServerColumnCodeModel> action)
        {
            if (action != null)
            {
                var column = GetColumn(name);
                if (column != null)
                {
                    action(column);
                }
            }
            return this;
        }
    }
}
