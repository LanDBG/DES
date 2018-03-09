using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSE.DataAccess.Models
{
    public class Configure
    {
        public int id { get; set; }

        public string config_name { get; set; }

        public string config_value { get; set; }

        public Byte? system_id { get; set; }

        public Byte? status { get; set; }

        public string comment { get; set; }

        public DateTime? updatedate { get; set; }

        public DateTime? createdate { get; set; }

        public static Configure Builder(IDataRecord record)
        {
            var id = record.GetInt32(record.GetOrdinal("id"));
            var config_name = record.IsDBNull(record.GetOrdinal("config_name")) ? string.Empty : record.GetString(record.GetOrdinal("config_name"));
            var config_value = record.IsDBNull(record.GetOrdinal("config_value")) ? string.Empty : record.GetString(record.GetOrdinal("config_value"));
            var system_id = record.IsDBNull(record.GetOrdinal("system_id")) ? null : (Byte?)record.GetByte(record.GetOrdinal("system_id"));
            var status = record.IsDBNull(record.GetOrdinal("status")) ? null : (Byte?)record.GetByte(record.GetOrdinal("status"));
            var comment = record.IsDBNull(record.GetOrdinal("comment")) ? string.Empty : record.GetString(record.GetOrdinal("comment"));
            var updatedate = record.IsDBNull(record.GetOrdinal("updatedate")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("updatedate"));
            var createdate = record.IsDBNull(record.GetOrdinal("createdate")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("createdate"));
            return new Configure
            {
                id = id,
                config_name = config_name,
                config_value = config_value,
                system_id = system_id,
                status = status,
                comment = comment,
                updatedate = updatedate,
                createdate = createdate,
            };
        }
    }
}
