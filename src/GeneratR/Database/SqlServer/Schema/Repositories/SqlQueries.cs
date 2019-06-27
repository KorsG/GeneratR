using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer.Schema
{
    internal static class SqlQueries
    {
        public static string SelectTables => @"
SELECT 
    [ObjectID]=o.object_id,
    [Schema]=s.name,
    [Name]=o.name
FROM 
    sys.objects AS [o] WITH (NOLOCK)
    JOIN sys.schemas AS [s] WITH (NOLOCK) 
        ON o.schema_id = s.schema_id
WHERE 
    o.type IN ('U') 
    AND o.is_ms_shipped=0
    AND o.name NOT IN ('sp_alterdiagram', 'sp_creatediagram', 'sp_dropdiagram', 'sp_helpdiagramdefinition', 'sp_helpdiagrams', 'sp_renamediagram', 'sp_upgraddiagrams', 'sysdiagrams')
";

        public static string SelectViews => @"
SELECT 
	[ObjectID]=o.object_id,
	[Schema]=s.name,
	[Name]=o.name
FROM 
	sys.objects AS [o] WITH (NOLOCK)
	JOIN sys.schemas AS [s] WITH (NOLOCK) 
		ON s.schema_id = o.schema_id
WHERE 
	o.type IN ('V') 
	AND o.is_ms_shipped=0
	AND o.name NOT IN ('sp_alterdiagram', 'sp_creatediagram', 'sp_dropdiagram', 'sp_helpdiagramdefinition', 'sp_helpdiagrams', 'sp_renamediagram', 'sp_upgraddiagrams', 'sysdiagrams')
";

        public static string SelectForeignKeys => @"
SELECT
    [ForeignKeyID]=fk.object_id,
    [ForeignKeyName]=fk.name,
    [FromObjectID]=fk.parent_object_id,
    [FromSchema]=SCHEMA_NAME(CAST(OBJECTPROPERTYEX(fk.parent_object_id,N'SchemaId') AS int)),
    [FromName]=OBJECT_NAME(fk.parent_object_id),
    [ToObjectID]=fk.referenced_object_id,
    [ToSchema]=SCHEMA_NAME(CAST(OBJECTPROPERTYEX(fk.referenced_object_id,N'SchemaId') AS int)),
    [ToName]=OBJECT_NAME(fk.referenced_object_id),
    [IsDisabled]=fk.is_disabled,
    [IsNotForReplication]=fk.is_not_for_replication,
    [IsNotTrusted]=fk.is_not_trusted
FROM 
    sys.foreign_keys AS [fk] WITH (NOLOCK)
WHERE 
    [fk].is_ms_shipped=0
";

        public static string SelectForeignKeyColumns => @"
SELECT
    [ForeignKeyID]=fkc.constraint_object_id,
    [FromColumnID]=pc.column_id,
    [FromColumnName]=pc.name,
    [FromOrdinalPosition]=fkc.constraint_column_id,
    [FromIsNullable]=pc.is_nullable,
    [ToColumnID]=rc.column_id,
    [ToColumnName]=rc.name,
    [ToOrdinalPosition]=fkc.constraint_column_id,
    [ToIsNullable]=rc.is_nullable
FROM 
    sys.foreign_key_columns AS [fkc] WITH (NOLOCK)
    JOIN sys.columns AS [pc] WITH (NOLOCK) 
        ON [fkc].parent_column_id = [pc].column_id 
        AND [fkc].parent_object_id = [pc].object_id
    JOIN sys.columns AS [rc] WITH (NOLOCK)
        ON [fkc].referenced_column_id = [rc].column_id 
        AND [fkc].referenced_object_id= [rc].object_id
";

        public static string SelectIndexes => @"
SELECT	
    [IndexName]=i.name,
    [ParentIndexID]=i.index_id,
    [ParentObjectID]=o.object_id,
    [ParentSchema]=os.name,
    [ParentName]=o.name,
    [ColumnName]=c.name,
    [ColumnIndexID]=ic.index_column_id,
    [ColumnOrdinalPosition]=ic.key_ordinal,
    [IsClustered]=CONVERT(bit, CASE i.type WHEN 1 THEN 1 ELSE 0 END),
    [IsUniqueKey]=i.is_unique,
    [IsUniqueConstraint]=i.is_unique_constraint,
    [IsPrimaryKey]=i.is_primary_key,
    [IsColumnNullable]=c.is_nullable,
    [NoRecompute]=s.no_recompute, 
    [IgnoreDuplicateKey]=i.ignore_dup_key,
    [IsPadIndex]= i.is_padded,
    [IsTable]=CONVERT(bit, CASE WHEN o.type = 'U' THEN 1 ELSE 0 END),
    [IsView]=CONVERT(bit, CASE WHEN o.type = 'V' THEN 1 ELSE 0 END),
    [IsFullTextKey]=CONVERT(bit, INDEXPROPERTY(i.object_id, i.name, N'IsFulltextKey')),
    [IsStatistics]=CONVERT(bit, 0), 
    [IsHypothetical]=i.is_hypothetical, 
    [FileGroup]=fg.name,
    [IndexFillFactor]=i.fill_factor,
    [IsDescending]=ic.is_descending_key,
    [IsDisabled]=i.is_disabled,
    [IsComputed]=CONVERT(bit, c.is_computed),
    [IsIncludedColumn]=ic.is_included_column , 
    [HasFilter]=i.has_filter, 
    [FilterDefinition]=i.filter_definition,
    [ParentType]=o.type,
    i.type,
    i.type_desc
FROM 
    sys.indexes AS [i] WITH (NOLOCK)
	LEFT JOIN sys.data_spaces AS [fg] WITH (NOLOCK) ON fg.data_space_id = i.data_space_id
	LEFT JOIN sys.objects AS [o] WITH (NOLOCK) ON o.object_id = i.object_id
	LEFT JOIN sys.schemas AS [os] WITH (NOLOCK) ON os.schema_id = o.schema_id
	LEFT JOIN sys.index_columns AS [ic] WITH (NOLOCK) ON ic.object_id = i.object_id AND ic.index_id = i.index_id
	LEFT JOIN sys.columns AS [c] WITH (NOLOCK) ON c.object_id = ic.object_id AND c.column_id = ic.column_id
	LEFT JOIN sys.stats AS [s] WITH (NOLOCK) ON s.object_id = i.object_id AND s.name = i.name
WHERE 
    i.type IN (0, 1, 2, 3) 
	AND o.is_ms_shipped=0 
	AND (os.name+'.'+o.name) NOT IN('dbo.sysdiagrams')
";

        public static string SelectColumns => @"
SELECT 
    [t].*,
    [IdentitySeed]=CASE WHEN t.IsIdentity=1 THEN IDENT_SEED(t.ParentSchema+'.'+t.ParentName) ELSE 0 END,
    [IdentityIncrement]=CASE WHEN t.IsIdentity=1 THEN IDENT_INCR(t.ParentSchema+'.'+t.ParentName) ELSE 0 END
FROM (
    SELECT 
	[ParentObjectID]=c.object_id,
    [ParentSchema]=CAST((CASE WHEN o.type='TT' THEN tt.[Schema] ELSE o.[Schema] END) AS nvarchar(255)),
    [ParentName]=CAST((CASE WHEN o.type='TT' THEN tt.[Name] ELSE o.[Name] END) AS nvarchar(255)),
    [ParentType]=o.type,
    [Name]=c.name, 
    [Position]=c.column_id,
    [DataType]=t.name,
    [Length]=
	    CASE 
            WHEN c.max_length >= 0 AND t.name IN (N'nchar', N'nvarchar') THEN c.max_length/2 
		    ELSE c.max_length 
	    END, 
    [Precision]=c.precision,
    [Scale]=c.scale,
    [IsNullable]=c.is_nullable, 
    [IsComputed]=c.is_computed,
    [IsPrimaryKey]=CAST((CASE WHEN kinfo.name IS NULL THEN 'false' ELSE 'true' END) AS bit),
    [PrimaryKeyPosition]=kinfo.key_ordinal,
    [IsRowGuid]=CAST(COLUMNPROPERTY(c.column_id, c.name, N'IsRowGuidCol') AS bit),
    [DefaultValueDefinition]=OBJECT_DEFINITION(c.default_object_id),
    [IsIdentity]=c.is_identity,
	[Description] = description.value
FROM 
    sys.columns AS [c] WITH (NOLOCK)
    JOIN sys.types AS [t] WITH (NOLOCK) 
        ON t.user_type_id = c.user_type_id
	LEFT JOIN sys.extended_properties AS [description] WITH (NOLOCK)
		ON description.major_id = c.object_id
		AND description.minor_id = c.column_id
		AND description.name = 'MS_Description'
    OUTER APPLY
	(
        SELECT 
			[Schema]=inners.name, innero.* 
		FROM 
			sys.objects AS [innero] WITH (NOLOCK)
			JOIN sys.schemas AS [inners] WITH (NOLOCK) 
				ON inners.schema_id = innero.schema_id 
        WHERE 
			innero.object_id = c.object_id
    ) AS [o]
    OUTER APPLY
	(
        SELECT 
			[Schema]=inners.name, innertt.* 
		FROM 
			sys.table_types AS [innertt] WITH (NOLOCK)
			JOIN sys.schemas AS [inners] WITH (NOLOCK) 
				ON inners.schema_id = innertt.schema_id 
        WHERE 
			innertt.type_table_object_id = c.object_id
    ) AS [tt]
    OUTER APPLY 
	(
        SELECT 
			k.name, k.type, k.unique_index_id, ic.index_id, ic.is_included_column, ic.key_ordinal 
		FROM 
			sys.key_constraints AS [k] WITH (NOLOCK) 
			JOIN sys.index_columns AS [ic] WITH (NOLOCK) 
			    ON ic.object_id = c.object_id AND ic.index_id = k.unique_index_id
		WHERE 
			k.parent_object_id = c.object_id 
			AND k.type='PK' 
			AND ic.column_id = c.column_id
    ) AS [kinfo]
WHERE 
    (o.is_ms_shipped=0 OR tt.is_table_type=1)
    AND o.name NOT IN ('sp_alterdiagram', 'sp_creatediagram', 'sp_dropdiagram', 'sp_helpdiagramdefinition', 'sp_helpdiagrams', 'sp_renamediagram', 'sp_upgraddiagrams', 'sysdiagrams')
) AS [t]
";

        public static string SelectParameters => @"
SELECT 
    [ParentObjectID]=p.object_id,
	[ParentSchema]=s.name, 
	[ParentName]=o.name, 
	[ParentType]=o.type, 
	[Name]=p.name, 
	[Position]=p.parameter_id, 
	[DataTypeSchema]=ts.name,
	[DataType]=t.name,
	[Length]=
	    CASE 
            WHEN p.max_length >= 0 AND t.name IN (N'nchar', N'nvarchar') THEN p.max_length/2 
		    ELSE p.max_length 
	    END,
	[Precision]=p.precision,
	[Scale]=p.scale,
	[IsNullable]=t.is_nullable,
	[IsOutput]=p.is_output,
	[IsReadonly]=p.is_readonly,
	[IsTableType]=t.is_table_type
FROM 
    sys.parameters AS [p] WITH (NOLOCK)
	JOIN sys.objects AS [o] WITH (NOLOCK) 
        ON p.object_id = o.object_id
	LEFT JOIN sys.schemas [s] WITH (NOLOCK) 
        ON o.schema_id = s.schema_id
	LEFT JOIN sys.types AS [t] WITH (NOLOCK) 
        ON p.user_type_id = t.user_type_id
	LEFT JOIN sys.schemas AS [ts] WITH (NOLOCK) 
        ON ts.schema_id = t.schema_id
WHERE 
    o.type in ('P', 'RF', 'PC', 'FN', 'FS', 'IF', 'TF') 
    AND o.name NOT IN ('fn_diagramobjects', 'sp_alterdiagram', 'sp_creatediagram', 'sp_dropdiagram', 'sp_helpdiagramdefinition', 'sp_helpdiagrams', 'sp_renamediagram', 'sp_upgraddiagrams', 'sysdiagrams')
    AND o.is_ms_shipped=0
";

        public static string SelectTableFunctions => @"
SELECT
    [ObjectID]=o.object_id,
    [Schema]=s.name,
    [Name]=o.name
FROM 
    sys.objects AS [o] WITH (NOLOCK)
    JOIN sys.schemas AS [s] WITH (NOLOCK)
        ON o.schema_id = s.schema_id
WHERE 
    o.type IN ('IF','TF')
    AND o.is_ms_shipped=0
    AND o.name NOT IN ('sp_alterdiagram', 'sp_creatediagram', 'sp_dropdiagram', 'sp_helpdiagramdefinition', 'sp_helpdiagrams', 'sp_renamediagram', 'sp_upgraddiagrams', 'sysdiagrams')
";

        public static string SelectStoredProcedures => @"
SELECT 
    [ObjectID]=o.object_id,
    [Schema]=s.name,
    [Name]=o.name
FROM 
    sys.objects AS [o] WITH (NOLOCK)
    JOIN sys.schemas AS [s] WITH (NOLOCK)
        ON o.schema_id = s.schema_id
WHERE 
    o.type IN ('P') 
    AND o.is_ms_shipped = 'false'
    AND o.name NOT IN ('sp_alterdiagram', 'sp_creatediagram', 'sp_dropdiagram', 'sp_helpdiagramdefinition', 'sp_helpdiagrams', 'sp_renamediagram', 'sp_upgraddiagrams', 'sysdiagrams')
";

        public static string SelectStoredProcedureResultColumns => @"
SELECT 
    [ParentSchema]=s.name, 
    [ParentName]=p.name, 
    [Name]=r.name,
    [DataType]=t.name, 
    [Length]=
	    CASE 
            WHEN r.max_length >= 0 AND t.name IN (N'nchar', N'nvarchar') THEN r.max_length/2 
		    ELSE r.max_length 
	    END,
    [Precision]=r.precision,
    [Scale]=r.scale,
    [Position]=r.column_ordinal, 
    [IsNullable]=r.is_nullable, 
    [IsComputed]=r.is_computed_column
FROM 
    sys.procedures AS [p]
    CROSS APPLY sys.dm_exec_describe_first_result_set_for_object(p.object_id, 0) AS [r]
    JOIN sys.schemas AS [s] 
        ON s.schema_id = p.schema_id
    JOIN sys.types AS [t] 
        ON t.user_type_id = r.system_type_id
WHERE 
    p.name NOT IN ('sp_alterdiagram', 'sp_creatediagram', 'sp_dropdiagram', 'sp_helpdiagramdefinition', 'sp_helpdiagrams', 'sp_renamediagram', 'sp_upgraddiagrams', 'sysdiagrams')
    AND r.name IS NOT NULL
";

        public static string SelectTableTypes => @"
SELECT 
    [ObjectID]=t.type_table_object_id,
    [Schema]=s.name,
    [Name]=t.name, 
    [IsNullable]=t.is_nullable
FROM 
    sys.table_types AS [t] WITH (NOLOCK) 
    JOIN sys.schemas AS [s] WITH (NOLOCK) 
        ON s.schema_id = t.schema_id
WHERE 
    t.is_table_type=1
";
    }
}
