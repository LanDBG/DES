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
    public class ArchiveFactory
    {
        public string _connectionString { get; set; }

        public ArchiveFactory(string connectionString)
        {
            this._connectionString = connectionString;

        }

        public List<Archive> GetArchives()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("[pr_get_target_archive_kpi]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        DataHelper dh = new DataHelper();
                        var result = cmd.ExecuteReader();
                        return dh.GetData(result, Archive.Builder).ToList();
                    }
                }

            }
            catch (System.Exception ex)
            {
                // Need a KPIger
                return new List<Archive>();
            }

            
        }

    }
}
