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
    public class LogFactory
    {
        public string _connectionString { get; set; }

        public LogFactory(string connectionString)
        {
            this._connectionString = connectionString;

        }

        public List<Log> GetLogs(int start, int size, ref int count)
        {
            var Logs = new List<Log>();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT * FROM [tb_log] 
                          ORDER BY [ActionTime]
                          OFFSET @START ROWS
                          FETCH NEXT @SIZE ROWS ONLY", con))
                    {
                        cmd.Parameters.Add("@START", SqlDbType.Int).Value = start;
                        cmd.Parameters.Add("@SIZE", SqlDbType.Int).Value = size;
                        DataHelper dh = new DataHelper();
                        Logs = dh.GetData(cmd.ExecuteReader(), Log.Builder).ToList();
                    }

                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [tb_log]", con))
                    {
                        DataHelper dh = new DataHelper();
                        count = (int)cmd.ExecuteScalar();
                    }
                }

            }
            catch (System.Exception ex)
            {
                // Need a logger
                return Logs;
            }

            return Logs;
        }

        public bool AddLog(Log log)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        @"INSERT INTO [tb_log] 
                          (TaskName, Status, ActionTime, Description, ServiceID)
                          VALUES(@TaskName, @Status, @ActionTime, @Description, @ServiceID)
                          ", con))
                    {
                        cmd.Parameters.Add("@TaskName", SqlDbType.NChar).Value = log.TaskName;
                        cmd.Parameters.Add("@Status", SqlDbType.NChar).Value = log.Status;
                        cmd.Parameters.Add("@ActionTime", SqlDbType.DateTime).Value = log.ActionTime;
                        cmd.Parameters.Add("@Description", SqlDbType.NChar).Value = log.Description;
                        cmd.Parameters.Add("@ServiceID", SqlDbType.UniqueIdentifier).Value = log.ServiceID;
                        DataHelper dh = new DataHelper();
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Need a logger
                return false;
            }
        }
    }
}
