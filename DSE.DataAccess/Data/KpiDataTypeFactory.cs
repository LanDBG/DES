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
    public class KpiDataTypeFactory
    {
        public string _connectionString { get; set; }

        public KpiDataTypeFactory(string connectionString)
        {
            this._connectionString = connectionString;

        }

        public List<KpiDataType> GetKpiDataTypes()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("[pr_get_target_kpi_datatype]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        DataHelper dh = new DataHelper();
                        var result = cmd.ExecuteReader();
                        var dataTypes = dh.GetData(result, KpiDataType.Builder).ToList();
                        return dataTypes;
                    }
                }

            }
            catch (System.Exception ex)
            {
                // Need a KPIger
                return new List<KpiDataType>();
            }
        }
    }
}
