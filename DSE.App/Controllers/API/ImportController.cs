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
                //WriteLog(insertstatement);
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
