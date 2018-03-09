using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSE.DataAccess.Models
{
    public class UserFuncRole
    {
        public int o_role_user_id { get; set; }
        public int o_role_id { get; set; }
        public int o_function_id { get; set; }
        public Guid o_user_id { get; set; }
        public bool is_active { get; set; }
        public DateTime? modification_date { get; set; }
        public DateTime create_date { get; set; }

        public static UserFuncRole Builder(IDataRecord record)
        {
            return new UserFuncRole
            {
                o_role_user_id = record.GetInt32(record.GetOrdinal("o_role_user_id")),
                o_role_id = record.GetInt32(record.GetOrdinal("o_role_id")),
                o_function_id = record.GetInt32(record.GetOrdinal("o_function_id")),
                o_user_id = record.GetGuid(record.GetOrdinal("o_user_id")),
                is_active = record.GetInt32(record.GetOrdinal("is_active")) == 1,
                modification_date = record.IsDBNull(record.GetOrdinal("modification_date")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("modification_date")),
                create_date = record.GetDateTime(record.GetOrdinal("modification_date"))
            };
        }
    }
}
