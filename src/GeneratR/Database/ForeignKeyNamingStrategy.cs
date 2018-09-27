namespace GeneratR.Database
{
    public enum ForeignKeyNamingStrategy
    {
        ///<summary>
        /// Uses the name of the referencing table. They will be numbered if multiple keys exists between the same tables.
        ///</summary>
        ReferencingTableName,

        ///<summary>
        /// Uses the name of the foreign key. 
        ///</summary>
        ForeignKeyName,

        ///<summary>
        ///	If Fk "From" column is named [Parent], and "To" table is named [EntityText], the best name would be [ParentEntityText]. 
        ///	If Fk "From" column is named [BossEmployeeID], and "To" table is named [Employee], the best name would be [BossEmployee].
        ///	If Fk "From" column is named [Country], and "To" table is named [Country], the best name would be [Country].
        ///</summary>
        Intelligent,
    }
}
