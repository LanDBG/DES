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
    public class KPIDateFactory
    {
        public string _connectionString { get; set; }

        public KPIDateFactory(string connectionString)
        {
            this._connectionString = connectionString;

        }

        public Tuple<DateTime, DateTime> GetKPIDate(string archivedName)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("[pr_get_target_kpi_startdate_enddate]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@archived_name", SqlDbType.NChar).Value = archivedName;
                        var result = cmd.ExecuteReader();
                        result.Read();
                        var startDate = result.GetDateTime(result.GetOrdinal("start_date"));
                        var endtDate = result.GetDateTime(result.GetOrdinal("end_date"));
                        var r = new {startDate, endtDate };
                        return Tuple.Create<DateTime, DateTime>(startDate, endtDate);
                    }
                }

            }
            catch (System.Exception ex)
            {
                return null;
            }
        }
    }
}