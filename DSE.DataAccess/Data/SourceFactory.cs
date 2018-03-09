using DSE.Common;
using DSE.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DSE.DataAccess.Data
{
    public class SourceFactory
    {

        public string _connectionString { get; set; }

        public SourceFactory(string connectionString)
        {
            this._connectionString = connectionString;

        }

        public List<Source> GetSourceCollection()
        {
            var sources = new List<Source>();

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tb_datasource_configure", con))
                    {
                        con.Open();
                        DataHelper dh = new DataHelper();

                        sources = dh.GetData(cmd.ExecuteReader(), Source.Builder).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return sources;
        }

        public bool CheckConnection(string connection)
        {
            throw new NotImplementedException();
        }

        public Source GetSource(Guid id)
        {
            var source = new Source();
            source.SourceId = id;
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("select * FROM [tb_datasource_configure]  WHERE [o_datasource_id] = @id", con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        DataHelper dh = new DataHelper();
                        source = dh.GetData(cmd.ExecuteReader(), Source.Builder).FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return source;
        }

        public Source Update(Source source)
        {
            source.Updated = false;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                bool updated = false;
                using (SqlCommand cmd = new SqlCommand(@"  
                    UPDATE [tb_datasource_configure] 
                    SET[o_client_id] = @o_client_id                     
                      ,[o_user_id_modification_by] = @o_user_id_modification_by
                      ,[source_name] = @source_name
                      ,[source_type] = @source_type
                      ,[source_server_name] = @source_server_name
                      ,[source_server_username] =@source_server_username
                      ,[source_server_password] = @source_server_password
                      ,[source_server_salt] = @source_server_salt
                      ,[source_server_port] =@source_server_port
                      ,[source_server_database_name] = @source_server_database_name
                      ,[source_server_file_path] = @source_server_file_path
                      ,[is_scheduled] =@is_scheduled
                      ,[schedule_start_time] = @schedule_start_time
                      ,[schedule_interval] =@schedule_interval
                      ,[is_active] = @is_active
                      ,[modification_date] = @modification_date
                      ,[source_file_name] = @source_file_name
                      ,[source_table_name] = @source_table_name
                      ,[source_number_of_record] = @source_number_of_record
                        ,[lastrun] = @lastrun
                        ,[status] = @status
                    WHERE o_datasource_id = @o_datasource_id", con))
                {
                    cmd.Parameters.AddWithValue("@o_client_id", source.ClientId);
                    cmd.Parameters.AddWithValue("@o_user_id_modification_by", source.UpdateBy);
                    cmd.Parameters.AddWithValue("@source_name", source.SourceName);
                    cmd.Parameters.AddWithValue("@source_type", source.SourceType);
                    cmd.Parameters.AddWithValue("@source_server_name", source.Source_ServerName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@source_server_username", source.Source_Server_Username ?? string.Empty);
                    cmd.Parameters.AddWithValue("@source_server_salt", source.Source_Server_Salt ?? string.Empty);
                    cmd.Parameters.AddWithValue("@source_server_port", source.Source_Server_Port ?? string.Empty);
                    cmd.Parameters.AddWithValue("@source_server_password", source.Source_Server_Password ?? string.Empty);
                    cmd.Parameters.AddWithValue("@source_server_database_name", source.Source_Server_DatabaseName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@source_server_file_path", source.Source_Server_FilePath ?? string.Empty);
                    cmd.Parameters.AddWithValue("@is_scheduled", (source.Is_Scheduled as object) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@schedule_start_time", (source.Schedule_Start_Time as object) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@schedule_interval", (source.Schedule_Interval as object) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@is_active", (source.Is_Active as object) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@modification_date", (source.UpdateDate as object) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@source_file_name", (source.UploadFileName as object) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@source_table_name", (source.CurrentTableName as object) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@source_number_of_record", (source.NumberOfRecord as object) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@lastrun", (source.LastRun as object) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@status", (source.Status as object) ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@o_datasource_id", source.SourceId);

                    try
                    {
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            updated = true;
                        }
                    }
                    catch (Exception e)
                    {
                        source.ErrorMessage = e.Message;
                        updated = false;
                    }

                }

                if (updated)
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM [tb_datasource_configure] WHERE o_datasource_id = @id", con))
                    {
                        cmd.Parameters.AddWithValue("@id", source.SourceId);

                        DataHelper dh = new DataHelper();

                        source = dh.GetData(cmd.ExecuteReader(), Source.Builder).FirstOrDefault();
                    }
                    source.Updated = true;

                }

            }
            return source;
        }

        public DataTable GetSourceImportedData(string currentTableName)
        {
            DataTable res = null;
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("select * FROM [" + currentTableName + "] ", con);
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    if (ds.Tables.Count > 0)
                    {
                        ds.Tables[0].TableName = currentTableName;
                        res = ds.Tables[0];
                    }
                    else
                    {
                        throw new Exception("No data found");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;

        }
        /// Validation zone
        public List<ValidateItem> GetValidationResult(Guid sourceId, Guid userId)
        {
            List<ValidateItem> sourceMappings = new List<ValidateItem>();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("pr_get_process_valid_data_table_by_datasource", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@datasource_id", SqlDbType.UniqueIdentifier).Value = sourceId;
                        cmd.Parameters.Add("@user_id", SqlDbType.UniqueIdentifier).Value = userId;
                        DataHelper dh = new DataHelper();
                        sourceMappings = dh.GetData(cmd.ExecuteReader(), ValidateItem.Builder).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return sourceMappings;
        }

        public int RunData(Guid id, Guid userId)
        {
            int is_valid = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("pr_run_data_by_datasource", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@datasource_id", SqlDbType.UniqueIdentifier).Value = id;
                        cmd.Parameters.Add("@user_id", SqlDbType.UniqueIdentifier).Value = userId;
                        cmd.Parameters.Add("@is_valid", SqlDbType.Int).Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        is_valid = Convert.ToInt32(cmd.Parameters["@is_valid"].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return is_valid;
        }

        public int RunDataTarget(Guid id, Guid userId)
        {
            int is_valid = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("pr_run_data_target_by_datasource", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@datasource_id", SqlDbType.UniqueIdentifier).Value = id;
                        cmd.Parameters.Add("@user_id", SqlDbType.UniqueIdentifier).Value = userId;
                        cmd.Parameters.Add("@is_valid", SqlDbType.Int).Direction = ParameterDirection.Output;
                        cmd.ExecuteNonQuery();
                        is_valid = Convert.ToInt32(cmd.Parameters["@is_valid"].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return is_valid;
        }

        public DataTable GetValidationTable(Guid id, Guid userId)
        {
            DataTable res = null;
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("pr_get_process_valid_data_table_by_datasource", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@datasource_id", SqlDbType.UniqueIdentifier).Value = id;
                    cmd.Parameters.Add("@user_id", SqlDbType.UniqueIdentifier).Value = userId;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    if (ds.Tables.Count > 0)
                    {
                        ds.Tables[0].TableName = "Mapping";
                        res = ds.Tables[0];
                    }
                    else
                    {
                        throw new Exception("No data found");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }
    }
}
