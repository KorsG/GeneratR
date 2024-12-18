﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratR.Database.SqlServer
{
    public class TableCodeModel : DbObjectClassCodeModel<Schema.Table>
    {
        public TableCodeModel(Schema.Table dbObject, SqlServerTypeMapper typeMapper)
            : base(dbObject)
        {
            Columns = new List<ColumnCodeModel>();
            ForeignKeys = new List<ForeignKeyCodeModel>();
            TypeMapper = typeMapper;
        }

        public SqlServerTypeMapper TypeMapper { get; }

        public List<ColumnCodeModel> Columns { get; set; }

        public List<ForeignKeyCodeModel> ForeignKeys { get; set; }

        public TableCodeModel AddColumn(ColumnCodeModel model)
        {
            if (Columns == null) { Columns = new List<ColumnCodeModel>(); }
            Columns.Add(model);
            return this;
        }

        public ColumnCodeModel GetColumn(string name)
        {
            return Columns.FirstOrDefault(x => x.DbObject.Name == name);
        }

        public TableCodeModel WithColumn(string name, Action<ColumnCodeModel> action)
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

        public new TableCodeModel Clone()
        {
            var baseClone = base.Clone();
            var clone = (TableCodeModel)MemberwiseClone();
            clone.Attributes = baseClone.Attributes;
            clone.Properties = baseClone.Properties;
            clone.ImplementInterfaces = baseClone.ImplementInterfaces;
            clone.DbObject = DbObject.Clone();
            clone.Columns = new List<ColumnCodeModel>(Columns.Select(x=> x.Clone()));
            clone.ForeignKeys = new List<ForeignKeyCodeModel>(ForeignKeys.Select(x => x.Clone()));

            return clone;
        }
    }
}
