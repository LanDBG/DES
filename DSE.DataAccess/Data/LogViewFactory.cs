using DSE.DataAccess.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace DSE.DataAccess.Data
{
    public class LogViewFactory
    {
        public string _connectionString { get; set; }

        public LogViewFactory(string connectionString)
        {
            this._connectionString = connectionString;

        }

        public List<LogViewModel> GetLogs(Guid? sourceId, int start, int size, ref int count)
        {
            var logs = new List<LogViewModel>();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("[pr_get_job_history_by_datasource]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        if (sourceId != null)
                        {
                            cmd.Parameters.Add("@datasource_id", SqlDbType.UniqueIdentifier).Value = sourceId;
                        }

                        cmd.Parameters.Add("@start", SqlDbType.Int).Value = start;
                        cmd.Parameters.Add("@size", SqlDbType.Int).Value = size;
                        cmd.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;

                        DataHelper dh = new DataHelper();
                        var result = cmd.ExecuteReader();
                        logs = dh.GetData(result, LogViewModel.Builder).ToList();

                        count = Convert.ToInt32(cmd.Parameters["@count"]?.Value ?? logs.Count);
                    }
                }

            }
            catch (System.Exception ex)
            {
                // Need a logger
                return logs;
            }

            return logs;
        }

        public List<LogDetailViewModel> LogDetailByJobId(Guid jobId)
        {
            var log = new List<LogDetailViewModel>();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("[pr_get_job_history_detail_by_job_id]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@job_id", SqlDbType.NChar).Value = jobId.ToString();

                        DataHelper dh = new DataHelper();
                        var result = cmd.ExecuteReader();
                        log = dh.GetData(result, LogDetailViewModel.Builder).ToList();
                    }
                }

            }
            catch (System.Exception ex)
            {
                // Need a logger
                return log;
            }
            return log;
        }
    }
}
