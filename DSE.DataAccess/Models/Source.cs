using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace DSE.DataAccess.Models
{
    public class Source
    {
        public Guid SourceId { get; set; }
        public int ClientId { get; set; }
        public Guid CreateBy { get; set; }
        public Guid? UpdateBy { get; set; }
        public string SourceName { get; set; }
        public int SourceType { get; set; } // Enum : 1- SQL, 2 Excel, 3 Oracle...
        public string Source_ServerName { get; set; }
        public string Source_Server_Username { get; set; }
        public string Source_Server_Password { get; set; }
        public string Source_Server_Salt { get; set; }
        public string Source_Server_Port { get; set; }
        public string Source_Server_DatabaseName { get; set; }
        public string Source_Server_FilePath { get; set; }
        public byte? Is_Scheduled { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true)]
        public DateTime? Schedule_Start_Time { get; set; }
        public DateTime Schedule_Start_Time_D { get; set; }
        public DateTime Schedule_Start_Time_T { get; set; }
        public decimal? Schedule_Interval { get; set; }
        public byte? Is_Active { get; set; }
        public int? NumberOfRecord { get; set; }
        public DateTime? CreateDate { get; set; }
        public bool Updated { get; internal set; }
        public string ErrorMessage { get;  set; }
        public string UploadFileName { get;  set; }
        public string CurrentTableName { get;  set; }
        public DateTime? UpdateDate { get;  set; }
        public List<DataTable> CurrentData { get; set; }

        public bool Status { get; set; }
        public DateTime? LastRun { get; set; }

        public static Source Builder(IDataRecord record)
        {
            return new Source
            {
                SourceId = record.GetGuid(record.GetOrdinal("o_datasource_id")),
                ClientId = record.GetInt32(record.GetOrdinal("o_client_id")),
                CreateBy = record.GetGuid(record.GetOrdinal("o_user_id_create_by")),
                UpdateBy = record.IsDBNull(record.GetOrdinal("o_user_id_modification_by")) ? null : (Guid?)record.GetGuid(record.GetOrdinal("o_user_id_modification_by")),
                SourceName = record.GetString(record.GetOrdinal("source_name")),
                SourceType = record.GetInt32(record.GetOrdinal("source_type")),
                Source_ServerName = record.IsDBNull(record.GetOrdinal("source_server_name")) ? null : record.GetString(record.GetOrdinal("source_server_name")),
                Source_Server_Username = record.IsDBNull(record.GetOrdinal("source_server_username")) ? null : record.GetString(record.GetOrdinal("source_server_username")),
                Source_Server_Password = record.IsDBNull(record.GetOrdinal("source_server_password")) ? null : record.GetString(record.GetOrdinal("source_server_password")),
                Source_Server_Salt = record.IsDBNull(record.GetOrdinal("source_server_salt")) ? null : record.GetString(record.GetOrdinal("source_server_salt")),
                Source_Server_Port = record.IsDBNull(record.GetOrdinal("source_server_port")) ? null : record.GetString(record.GetOrdinal("source_server_port")),
                Source_Server_DatabaseName = record.IsDBNull(record.GetOrdinal("source_server_database_name"))?null: record.GetString(record.GetOrdinal("source_server_database_name")),
                Source_Server_FilePath = record.IsDBNull(record.GetOrdinal("source_server_file_path")) ? null : record.GetString(record.GetOrdinal("source_server_file_path")),
                Is_Scheduled = record.IsDBNull(record.GetOrdinal("is_scheduled")) ? null : (byte?)record.GetByte(record.GetOrdinal("is_scheduled")),
                Schedule_Start_Time = record.IsDBNull(record.GetOrdinal("schedule_start_time")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("schedule_start_time")),
                Schedule_Interval = record.IsDBNull(record.GetOrdinal("schedule_interval")) ? null : (decimal?)record.GetDecimal(record.GetOrdinal("schedule_interval")),
                Is_Active = record.IsDBNull(record.GetOrdinal("is_active")) ? null : (byte?)record.GetByte(record.GetOrdinal("is_active")),
                UpdateDate = record.IsDBNull(record.GetOrdinal("modification_date")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("modification_date")),
                CreateDate = record.IsDBNull(record.GetOrdinal("create_date")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("create_date")),
                UploadFileName = record.IsDBNull(record.GetOrdinal("source_file_name")) ? null : record.GetString(record.GetOrdinal("source_file_name")),
                CurrentTableName = record.IsDBNull(record.GetOrdinal("source_table_name")) ? null : record.GetString(record.GetOrdinal("source_table_name")),
                LastRun = record.IsDBNull(record.GetOrdinal("lastrun")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("lastrun")),
                Status = record.IsDBNull(record.GetOrdinal("status")) ? false : record.GetBoolean(record.GetOrdinal("status")),
                NumberOfRecord = record.IsDBNull(record.GetOrdinal("source_number_of_record")) ? 0 : record.GetInt32(record.GetOrdinal("source_number_of_record"))
            };
    }
}
}