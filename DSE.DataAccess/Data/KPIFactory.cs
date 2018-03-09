using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Web.Script.Serialization;
using DSE.DataAccess.Models;

namespace DSE.DataAccess.Data
{
    public class KPIFactory
    {
        public string _connectionString { get; set; }

        public KPIFactory(string connectionString)
        {
            this._connectionString = connectionString;

        }

        public List<object> GetKPIs(string archivedName, int start, int size, ref int count)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("[pr_get_target_kpi]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        //if (!string.IsNullOrEmpty(archivedName))
                        //{
                        cmd.Parameters.Add("@archived_name", SqlDbType.NChar).Value = archivedName;
                        //}

                        cmd.Parameters.Add("@start", SqlDbType.Int).Value = start;
                        cmd.Parameters.Add("@size", SqlDbType.Int).Value = size;
                        cmd.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;

                        DataHelper dh = new DataHelper();
                        var result = cmd.ExecuteReader();
                        var returnResult = ToJson(result);
                        count = Convert.ToInt32(cmd.Parameters["@count"]?.Value ?? 0);
                        return returnResult;
                    }
                }

            }
            catch (System.Exception ex)
            {
                // Need a KPIger
                return new List<object>();
            }

            return new List<object>();
        }

        public List<string> GetFieldsName()
        {
            return GetFieldsName(null);
        }
        public List<string> GetFieldsName(Dictionary<string, Type> types)
        {
            var fieldsName = new List<string>();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("[pr_get_target_kpi]", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@start", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@size", SqlDbType.Int).Value = 1;
                        cmd.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;

                        DataHelper dh = new DataHelper();
                        var result = cmd.ExecuteReader();

                        int fields = result.FieldCount;

                        for (int i = 0; i < fields; i++)
                        {
                            var name = result.GetName(i);
                            fieldsName.Add(name);
                            if (types != null)
                            {
                                types.Add(name, result.GetFieldType(i));
                            }
                        }

                        return fieldsName;
                    }
                }

            }
            catch (System.Exception ex)
            {
                // Need a KPIger
                return fieldsName;
            }
        }

        public List<object> ToJson(SqlDataReader rdr)
        {
            return ToJson(rdr, new List<Type>());
        }

        public List<object> ToJson(SqlDataReader rdr, List<Type> types)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.WriteStartArray();

                while (rdr.Read())
                {
                    jsonWriter.WriteStartObject();

                    int fields = rdr.FieldCount;

                    for (int i = 0; i < fields; i++)
                    {
                        jsonWriter.WritePropertyName(rdr.GetName(i));
                        jsonWriter.WriteValue(rdr[i]);
                        types.Add(rdr.GetFieldType(i));
                    }

                    jsonWriter.WriteEndObject();
                }

                jsonWriter.WriteEndArray();
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                rdr.Close();
                return json_serializer.Deserialize<List<object>>(sw.ToString());
            }
        }

        public object GetKPIById(string id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM [vw_target_kpi] WHERE [o_target_kpi_id] = @id", con))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.NChar).Value = id;

                        DataHelper dh = new DataHelper();
                        var result = cmd.ExecuteReader();
                        var returnResult = ToJson(result);
                        if (returnResult.Count > 0)
                        {
                            return returnResult[0];
                        }
                        return new object();
                    }
                }

            }
            catch (System.Exception ex)
            {
                // Need a KPIger
                return new object();
            }

        }

        public Configure GetKPIShowFields()
        {
            try
            {
                ConfigureFactory configFac = new ConfigureFactory(_connectionString);
                return configFac.GetConfigByName("KPI_SHOW_FIELDS");
            }
            catch (System.Exception ex)
            {
                // Need a KPIger
                throw ex;
            }
        }

        public bool UpdateKPIShowFields(string fields)
        {
            try
            {
                ConfigureFactory configFac = new ConfigureFactory(_connectionString);
                var config = configFac.GetConfigByName("KPI_SHOW_FIELDS");
                config.config_value = fields;
                return configFac.Update(config);
            }
            catch (System.Exception ex)
            {
                // Need a KPIger
                throw ex;
            }
        }
    }
}
