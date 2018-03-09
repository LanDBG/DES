using DSE.DataAccess.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
namespace DSE.DataAccess.Data
{
    public class FunctionFactory
    {

        public string _connectionString { get; set; }

        public FunctionFactory(string connectionString)
        {
            this._connectionString = connectionString;

        }

        public List<Function> GetListFunction()
        {
            var functions = new List<Function>();
            List<Function> selectFunctions = new List<Function>();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM [tb_role_function]", con))
                    {
                        con.Open();
                        DataHelper dh = new DataHelper();

                        functions = dh.GetData(cmd.ExecuteReader(), Function.Builder).ToList();
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Need a logger
                return functions;
            }

            return functions;
        }
    }

    public class FunctionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
