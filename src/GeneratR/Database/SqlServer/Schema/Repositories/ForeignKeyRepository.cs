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
            return GetWhere(string.Empty, null);
        }

        private IEnumerable<ForeignKey> GetWhere(string whereSql, object whereParams)
        {
            var sqlBuilder = new SqlBuilder();

            if (!string.IsNullOrWhiteSpace(whereSql))
            {
                sqlBuilder.Where(whereSql, whereParams);
            }

            if (_schemaContext.IncludeSchemas != null && _schemaContext.IncludeSchemas.Any())
            {
                sqlBuilder.Where("[t].[FromSchema] IN @IncludeSchemas", new { IncludeSchemas = _schemaContext.IncludeSchemas, });
            }

            if (_schemaContext.ExcludeSchemas != null && _schemaContext.ExcludeSchemas.Any())
            {
                sqlBuilder.Where("[t].[FromSchema] NOT IN @ExcludeSchemas", new { ExcludeSchemas = _schemaContext.ExcludeSchemas, });
            }

            var query = sqlBuilder.AddTemplate($@"
SELECT * INTO #ForeignKeys FROM ({SqlQueries.SelectForeignKeys}) AS [t] /**where**/;

SELECT * FROM (
    SELECT * FROM #ForeignKeys
    UNION
    SELECT * FROM ({SqlQueries.SelectForeignKeys}) AS [t] WHERE [t].ToObjectID IN(SELECT ToObjectID FROM #ForeignKeys) 
    ) AS [t]
ORDER BY 
    [t].[FromSchema], [t].[FromName];
                    
SELECT * FROM ({SqlQueries.SelectForeignKeyColumns}) AS [c] WHERE c.ForeignKeyID IN(SELECT ForeignKeyID FROM #ForeignKeys);
SELECT * FROM ({SqlQueries.SelectIndexes}) AS [i] WHERE i.ParentObjectID IN(SELECT FromObjectID FROM #ForeignKeys) OR i.ParentObjectID IN(SELECT ToObjectID FROM #ForeignKeys);
");

            IList<ForeignKey> data;
            IEnumerable<dynamic> allFkColumns;
            IEnumerable<Index> allFkColumnIndexes;
            using (var conn = _schemaContext.GetConnection())
            using (var multi = conn.QueryMultiple(query.RawSql, query.Parameters))
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
