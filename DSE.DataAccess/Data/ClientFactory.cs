using DSE.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace DSE.DataAccess.Data
{
    public class ClientFactory
    {
        public string _connectionString { get; set; }

        public ClientFactory(string connectionString)
        {
            this._connectionString = connectionString;

        }

        public List<SelectListItem> GetSelectListClient()
        {
            var roles = new List<Client>();
            List<SelectListItem> selectRoles = new List<SelectListItem>();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tb_client", con))
                    {
                        con.Open();
                        DataHelper dh = new DataHelper();

                        roles = dh.GetData(cmd.ExecuteReader(), Client.Builder).ToList();
                    }
                }
               
                foreach (var item in roles)
                {
                    selectRoles.Add(new SelectListItem { Value = item.ClientId.ToString(), Text = item.ClientName });
                }

            }
            catch (Exception)
            {
                return new List<SelectListItem>();
            }
            return selectRoles;
        }
    }
}
