using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace GeneratR.Database.SqlServer.Schema
{
    public class ParameterRepository
    {
        private readonly SqlServerSchemaContext _schemaContext;

        public ParameterRepository(SqlServerSchemaContext schemaContext)
        {
            _schemaContext = schemaContext;
        }

        public IEnumerable<Parameter> GetAll()
        {
            return GetWhere("", null);
        }

        public IEnumerable<Parameter> GetAllForTableFunctions()
        {
            return GetWhere("ParentType IN @ParentTypes", new { ParentTypes = new List<string>() { "IF", "TF" } });
        }

        public IEnumerable<Parameter> GetAllForScalarFunctions()
        {
            return GetWhere("ParentType IN @ParentTypes", new { ParentTypes = new List<string>() { "FN" } });
        }

        public IEnumerable<Parameter> GetAllForStoredProcedures()
        {
            return GetWhere("ParentType=@ParentType", new { ParentType = "P" });
        }

        private IEnumerable<Parameter> GetWhere(string sqlWhere, object sqlParams)
        {
            if (!string.IsNullOrWhiteSpace(sqlWhere) && !sqlWhere.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
            {
                sqlWhere = "WHERE " + sqlWhere;
            }

            var parameters = new List<Parameter>();
            var sqlText = $"SELECT * FROM ({SqlQueries.SelectParameters}) AS [t] {sqlWhere} ORDER BY [t].[ParentSchema], [t].[ParentName], [t].[Position]";

            using (var conn = _schemaContext.GetConnection())
            {
                var dbParameters = conn.Query(sqlText, sqlParams).ToList();
                foreach (var q in dbParameters)
                {
                    var param = new Parameter()
                    {
                        ParentObjectID = q.ParentObjectID,
                        ParentSchema = q.ParentSchema,
                        ParentName = q.ParentName,
                        ParentType = q.ParentType,
                        Position = (int)q.Position,
                        Name = (q.Name as string).Replace("@", string.Empty),
                        DataTypeSchema = q.DataTypeSchema,
                        DataType = q.DataType,
                        Length = (short)q.Length,
                        Precision = (byte)q.Precision,
                        Scale = (byte)q.Scale,
                        IsNullable = q.IsNullable ?? false,
                        IsReadonly = q.IsReadonly,
                        IsTableType = q.IsTableType,
                        Direction = ((bool)q.IsOutput) ? ParameterDirection.InAndOutDirection : ParameterDirection.InDirection, // SqlServer does not have OutputOnly parameters as of 2012.
                    };

                    parameters.Add(param);
                }
            }

            return parameters;
        }
    }
}
