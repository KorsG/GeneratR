using GeneratR.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratR.Database.SqlServer
{
    public class TableTypeCodeModel : DbObjectClassCodeModel<Schema.TableType>
    {
        public TableTypeCodeModel(Schema.TableType dbObject, SqlServerTypeMapper typeMapper)
            : base(dbObject)
        {
            Columns = new List<ColumnCodeModel>();
            TypeMapper = typeMapper;
        }

        public SqlServerTypeMapper TypeMapper { get; }

        public List<ColumnCodeModel> Columns { get; set; }

        public bool AddSqlDataRecordMappings { get; set; }

        public ColumnCodeModel GetColumn(string name)
        {
            return Columns?.FirstOrDefault(x => x.DbObject.Name == name);
        }

        public TableTypeCodeModel WithColumn(string name, Action<ColumnCodeModel> action)
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

        public new TableTypeCodeModel Clone()
        {
            var baseClone = base.Clone();
            var clone = (TableTypeCodeModel)MemberwiseClone();
            clone.Attributes = baseClone.Attributes;
            clone.Properties = baseClone.Properties;
            clone.ImplementInterfaces = baseClone.ImplementInterfaces;
            clone.DbObject = DbObject.Clone();
            clone.Columns = new List<ColumnCodeModel>(Columns.Select(x => x.Clone()));

            return clone;
        }
    }
}
