using DSE.App.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace DSE.App.Controllers.API
{
    public class ImportController : ApiController
    {
        [Route("api/ImportData/{tablename}")]
        [HttpPost]
        [ResponseType(typeof(string))]
        public HttpResponseMessage ImportData(string tablename, [FromBody]RawData body)
        {
            if (body == null || body.Data == null)
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Invalid data");
            try
            {
                string insertstatement = StringToDatatable(body.Data, tablename);
                WriteLog(insertstatement);
                using (SqlConnection con = new SqlConnection(dseCnnString))
                {
                    con.Open();
                    //Truncate table
                    if (body.Truncate)
                    {
                        string sqlTruncate = "TRUNCATE TABLE " + tablename;
                        SqlCommand cmd = new SqlCommand(sqlTruncate, con);
                        cmd.ExecuteNonQuery();
                    }
                    //Cannot use SQL Bulk Copy here cause of dynamic column required and datatype issue
                    SqlCommand inserCmd = new SqlCommand(insertstatement, con);
                    inserCmd.ExecuteNonQuery();

                    con.Close();
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Success");
            }
            catch (Exception ex)
            {
                //WriteLog(ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [Route("api/GetSetting/{configname}")]
        [HttpPost]
        [ResponseType(typeof(TableSetting))]
        public HttpResponseMessage GetSetting(string configname, [FromBody]TableSetting body)
        {
            if (body == null || body.ConfigName == null)
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Invalid data");
            
            try
            {
                TableSetting obj = new TableSetting();
                obj.ConfigName = body.ConfigName;
                using (SqlConnection con = new SqlConnection(dseCnnString))
                {
                    SqlCommand cmd = new SqlCommand("pr_table_configure_get", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@config_name", body.ConfigName);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        obj.WhereColumn = reader["where_column"] == DBNull.Value ? "" : (string)reader["where_column"];
                        obj.StartValue = reader["start_value"] == DBNull.Value ? "" : (string)reader["start_value"];
                        obj.EndValue = reader["end_value"] == DBNull.Value ? "" : (string)reader["end_value"];
                        obj.Sequence = reader["sequence"] == DBNull.Value ? 0 : (int)reader["sequence"];
                        obj.LastValue = reader["last_value"] == DBNull.Value ? "" : (string)reader["last_value"];

                        obj.SourceTable = reader["source_table"] == DBNull.Value ? "" : (string)reader["source_table"];
                        obj.Columns = reader["columns"] == DBNull.Value ? "" : (string)reader["columns"];
                        obj.PrimaryKey = reader["primary_key"] == DBNull.Value ? "" : (string)reader["primary_key"];
                        obj.RowPerRound = reader["row_per_round"] == DBNull.Value ? 0 : (int)reader["row_per_round"];
                        obj.Truncate = reader["is_truncate"] == DBNull.Value ? false : (bool)reader["is_truncate"];
                        obj.Active = reader["is_active"] == DBNull.Value ? false : (bool)reader["is_active"];
                        reader.Close();
                    }
                    con.Close();
                }
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
            catch (Exception ex)
            {
                //WriteLog(ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [Route("api/UpdateSetting/{configname}")]
        [HttpPost]
        [ResponseType(typeof(string))]
        public HttpResponseMessage UpdateSetting(string configname, [FromBody]TableSetting body)
        {
            if (body == null || body.ConfigName == null)
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Invalid data");
            if (body.StartValue == null || body.EndValue == null)
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Invalid start & end value");
            try
            {
                //WriteLog(insertstatement);
                int rowEffected = 0;
                using (SqlConnection con = new SqlConnection(dseCnnString))
                {
                    SqlCommand cmd = new SqlCommand("pr_table_configure_update", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@config_name", body.ConfigName);
                    cmd.Parameters.AddWithValue("@start_value", body.StartValue);
                    cmd.Parameters.AddWithValue("@end_value", body.EndValue);
                    cmd.Parameters.AddWithValue("@last_value", body.LastValue);
                    con.Open();
                    rowEffected = cmd.ExecuteNonQuery();
                    con.Close();
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Success");
            }
            catch (Exception ex)
            {
                //WriteLog(ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        #region Helper
        static string dseCnnString = ConfigurationManager.ConnectionStrings["dseCnnString"].ConnectionString;
        public string StringToDatatable(string data, string table)
        {
            string[] lines = data.Split('\n');
            string insert = string.Format("INSERT INTO {0} ({1}) VALUES ", table, lines[0]);
            string result = string.Empty;
            for (int i = 1; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i].Trim()))
                {
                    if (i == 1) //last row
                        result += "(" + lines[i].Replace('"', '\'') + ") \n";
                    else
                        result += ", (" + lines[i].Replace('"', '\'') + ") \n";
                }
            }
            
            return insert + result;
        }
        public void WriteLog(string s)
        {

            StreamWriter log;
            string fileName = "E:\\Duke\\DES\\trunk\\DSE.App\\logs\\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            if (!File.Exists(fileName))
            {
                log = new System.IO.StreamWriter(fileName);
            }
            else
            {
                log = File.AppendText(fileName);
            }

            // Write to the file:
            log.WriteLine(DateTime.Now);
            log.WriteLine(s);
            log.WriteLine("---------------------------");

            // Close the stream:
            log.Close();
        }

       
        #endregion
    }
}
