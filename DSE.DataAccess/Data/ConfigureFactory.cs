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
    class ConfigureFactory
    {
        public string _connectionString { get; set; }

        public ConfigureFactory(string connectionString)
        {
            this._connectionString = connectionString;

        }
        public Configure GetConfigByName(string name)
        {
            var configs = new List<Configure>();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM [tb_configure] WHERE [config_name] = @name", con))
                    {
                        cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = name;
                        DataHelper dh = new DataHelper();
                        var result = cmd.ExecuteReader();
                        configs = dh.GetData(result, Configure.Builder).ToList();
                        return configs.FirstOrDefault();
                    }
                }

            }
            catch (System.Exception ex)
            {
                // Need a KPIger
                return configs.FirstOrDefault();
            }
        }

        public Boolean Update(Configure config)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                bool updated = false;
                using (SqlCommand cmd = new SqlCommand(@"  
                    UPDATE [tb_configure] 
                    SET config_name   = @config_name,
                        config_value = @config_value,
                        system_id   = @system_id,
                        status = @status,
                        comment = @comment,
                        updatedate = @updatedate,
                        createdate = @createdate
                    WHERE id = @id", con))
                {
                    cmd.Parameters.AddWithValue("@id", config.id);
                    cmd.Parameters.AddWithValue("@config_name", config.config_name);
                    cmd.Parameters.AddWithValue("@config_value", config.config_value ?? string.Empty);
                    cmd.Parameters.AddWithValue("@system_id", (config.system_id as object) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@status", (config.status as object) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@comment", config.comment ?? string.Empty);
                    cmd.Parameters.AddWithValue("@updatedate", (config.updatedate as object) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@createdate", (config.createdate as object) ?? DBNull.Value);

                    try
                    {
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            updated = true;
                        }
                    }
                    catch (Exception e)
                    {
                        updated = false;
                    }

                    return updated;
                }
            }
        }
    }
}
