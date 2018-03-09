using DSE.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSE.DataAccess.Data
{
    public class SourceMappingFactory
    {
        public string _connectionString { get; set; }

        public SourceMappingFactory(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public List<SourceMapping> GetSourceMappingCollection(Guid sourceId)
        {
            List<SourceMapping> sourceMappings = new List<SourceMapping>();

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("pr_get_mapping_table_by_datasource", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@datasource_id", SqlDbType.UniqueIdentifier).Value = sourceId;
                        DataHelper dh = new DataHelper();
                        sourceMappings = dh.GetData(cmd.ExecuteReader(), SourceMapping.Builder).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return sourceMappings;
        }

        public DataTable GetSourceMappingTable(Guid id)
        {
            DataTable res = null;
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("pr_get_mapping_table_by_datasource", con);                   
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@datasource_id", SqlDbType.UniqueIdentifier).Value = id;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    if (ds.Tables.Count > 0)
                    {
                        ds.Tables[0].TableName = "Mapping";
                        res = ds.Tables[0];
                    }
                    else
                    {
                        throw new Exception("No data found");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }
    }
}
