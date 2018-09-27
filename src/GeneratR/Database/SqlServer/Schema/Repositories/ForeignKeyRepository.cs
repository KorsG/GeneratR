using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace GeneratR.Database.SqlServer.Schema
{
    public class ForeignKeyRepository
    {
        private readonly SqlServerSchemaContext _schemaContext;

        public ForeignKeyRepository(SqlServerSchemaContext schemaContext)
        {
            _schemaContext = schemaContext;
        }

        public IEnumerable<ForeignKey> GetAll()
        {
            return (new SelectQueryBuilder(_schemaContext).Execute());
        }

        private IEnumerable<ForeignKey> GetWhere(string whereSql, object whereParams)
        {
            var b = new SelectQueryBuilder(_schemaContext);
            b.AddWhere(whereSql, whereParams);
            return b.Execute();
        }

        private class SelectQueryBuilder
        {
            private readonly SqlServerSchemaContext _schemaContext;
            private readonly IDictionary<string, object> _whereClauses = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            private readonly IDictionary<string, object> _orderByClauses = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            public SelectQueryBuilder(SqlServerSchemaContext schemaContext)
            {
                _schemaContext = schemaContext;
            }

            public SelectQueryBuilder AddWhere(string sql, object parameters = null)
            {
                _whereClauses.Add(sql, parameters);
                return this;
            }

            public SelectQueryBuilder AddOrderBy(string sql, object parameters = null)
            {
                _orderByClauses.Add(sql, parameters);
                return this;
            }

            public IEnumerable<ForeignKey> Execute()
            {
                var b = new SqlBuilder();
                IList<ForeignKey> data;

                foreach (var q in _whereClauses)
                {
                    b.Where(q.Key, q.Value);
                }
                if (_orderByClauses.Any())
                {
                    foreach (var q in _orderByClauses)
                    {
                        b.OrderBy(q.Key, q.Value);
                    }
                }
                else
                {
                    b.OrderBy("FromSchema, FromName"); // Default sort.
                }

                var t = b.AddTemplate($@"
SELECT * INTO #ForeignKeys FROM ({SqlQueries.SelectForeignKeys}) [t0] /**where**/;
SELECT * FROM #ForeignKeys
UNION
SELECT * FROM ({SqlQueries.SelectForeignKeys}) [t1] WHERE [t1].ToObjectID IN(SELECT ToObjectID FROM #ForeignKeys) /**orderby**/;
                    
SELECT * FROM ({SqlQueries.SelectForeignKeyColumns}) [c] WHERE c.ForeignKeyID IN(SELECT ForeignKeyID FROM #ForeignKeys);
SELECT * FROM ({SqlQueries.SelectIndexes}) [i] WHERE i.ParentObjectID IN(SELECT FromObjectID FROM #ForeignKeys) OR i.ParentObjectID IN(SELECT ToObjectID FROM #ForeignKeys);
");

                IEnumerable<dynamic> allFkColumns;
                IEnumerable<Index> allFkColumnIndexes;
                using (var conn = _schemaContext.GetConnection())
                using (var multi = conn.QueryMultiple(t.RawSql, t.Parameters))
                {
                    data = multi.Read<ForeignKey>().ToList();
                    allFkColumns = multi.Read<dynamic>();
                    allFkColumnIndexes = multi.Read<Index>();
                }

                foreach (var q in data)
                {
                    var fkc = allFkColumns.Where(x => x.ForeignKeyID == q.ForeignKeyID).ToList();
                    fkc.ForEach(f =>
                    {
                        q.FromColumns.Add(new ForeignKeyColumn()
                        {
                            ForeignKeyID = f.ForeignKeyID,
                            ColumnID = f.FromColumnID,
                            ColumnName = f.FromColumnName,
                            OrdinalPosition = f.FromOrdinalPosition,
                            IsNullable = f.FromIsNullable
                        });
                        q.ToColumns.Add(new ForeignKeyColumn()
                        {
                            ForeignKeyID = f.ForeignKeyID,
                            ColumnID = f.ToColumnID,
                            ColumnName = f.ToColumnName,
                            OrdinalPosition = f.ToOrdinalPosition,
                            IsNullable = f.ToIsNullable
                        });
                    });

                    // Get constraints/indexes that belongs to FK "From" object.
                    // http://social.msdn.microsoft.com/Forums/sqlserver/en-US/c0ee3665-fe41-4b85-a10f-41af4cfe257c/sysindexesisunique-vs-sysindexesisuniqueconstraint
                    var fromObjectIndexes = allFkColumnIndexes.Where(x => x.ParentObjectID == q.FromObjectID).ToList();

                    // Get distinct list of primary/unique keys/constraints/indexes names, that contains any of the FK "From" columns.
                    var distinctFromObjectIndexNames = (
                        from i in fromObjectIndexes
                        where (i.IsUniqueKey || i.IsUniqueConstraint || i.IsPrimaryKey)
                        && q.FromColumns.Select(x => x.ColumnName.ToLower()).Contains(i.ColumnName.ToLower())
                        select i.IndexName).Distinct().ToList();

                    // Check if all of the FK "From" columns is a full unique key.
                    var fromColumnsIsUnique = false;
                    distinctFromObjectIndexNames.ForEach(ixName =>
                    {
                        var keyColumns = fromObjectIndexes.Where(i => i.IndexName == ixName).ToList();
                        if (keyColumns.Count() == q.FromColumns.Count())
                        {
                            fromColumnsIsUnique = true;
                        }
                    });

                    if (fromColumnsIsUnique)
                    {
                        q.RelationshipType = ForeignKeyRelationshipType.OneToOne;
                    }
                    else
                    {
                        q.RelationshipType = ForeignKeyRelationshipType.OneToMany;
                    }

                    // A foreignkey's primarykey cannot be a unique filtered index - so a fk's primarykey cannot contain nullable columns.              
                    if (q.IsDisabled || q.FromColumns.Any(x => x.IsNullable))
                    {
                        q.IsOptional = true;
                    }
                }

                return data;
            }
        }
    }
}
