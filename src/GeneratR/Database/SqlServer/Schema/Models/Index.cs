using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratR.Database.SqlServer.Schema
{
    public class Index
    {
        public object ParentObject { get; set; }

        public string IndexName { get; set; }
        
        /// <summary> 
        /// ID of the index. Is unique only within the ParentObject.
        /// 0 = Heap
        /// 1 = Clustered index
        /// > 1 = Nonclustered index
        /// </summary>
        public int ParentIndexID { get; set; }
        public int ParentObjectID { get; set; }
        public string ParentSchema { get; set; }
        public string ParentName { get; set; }
        public string ColumnName { get; set; }

        /// <summary> 
        /// ID of the index column. Is unique only within ParentIndexID.
        /// </summary>
        public int ColumnIndexID { get; set; }

        /// <summary> 
        /// Ordinal (1-based) within set of key-columns.
        /// </summary>
        public int ColumnOrdinalPosition { get; set; }
        public bool IsClustered { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsUniqueKey { get; set; }
        public bool IsUniqueConstraint { get; set; }
        public bool IsColumnNullable { get; set; }
        public bool NoRecompute { get; set; }
        public bool IgnoreDuplicateKey { get; set; }
        public bool IsPadIndex { get; set; }
        public bool IsTable { get; set; }
        public bool IsView { get; set; }
        public bool IsFullTextKey { get; set; }
        public bool IsStatistics { get; set; }
        public bool IsHypothetical { get; set; }
        public string FileGroup { get; set; }
        public short FillFactor { get; set; }
        public bool IsDescending { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsIncludedColumn { get; set; }
        public bool IsComputedColumn { get; set; }
        public bool HasFilter { get; set; }
        public string FilterDefinition { get; set; }

        public override string ToString()
        {
            return $"{IndexName}";
        }

        public Index Clone()
        {
            var clone = (Index)MemberwiseClone();
            return clone;
        }
    }
}
