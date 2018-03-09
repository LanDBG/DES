using DSE.DataAccess.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
namespace DSE.DataAccess.Data
{
    public class RoleFactory
    {

        public string _connectionString { get; set; }

        public RoleFactory(string connectionString)
        {
            this._connectionString = connectionString;

        }

        public List<SelectListItem> GetSelectListRole()
        {
            var roles = new List<Role>();
            List<SelectListItem> selectRoles = new List<SelectListItem>();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tb_role", con))
                    {
                        con.Open();
                        DataHelper dh = new DataHelper();

                        roles = dh.GetData(cmd.ExecuteReader(), Role.Builder).ToList();
                    }
                }
              
                foreach (var item in roles)
                {
                    selectRoles.Add(new SelectListItem { Value = item.RoleId.ToString(), Text = item.RoleName });
                }
            }
            catch (System.Exception ex)
            {
                // Need a logger
                return new List<SelectListItem>();                
            }

            return selectRoles;
        }

        public List<Role> GetListRole()
        {
            var functions = new List<Role>();
            List<Role> selectFunctions = new List<Role>();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM [tb_role]", con))
                    {
                        con.Open();
                        DataHelper dh = new DataHelper();

                        functions = dh.GetData(cmd.ExecuteReader(), Role.Builder).ToList();
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
}
