using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSE.DataAccess.Models
{
    public class Function
    {
        public int o_function_id { get; set; }
        public string function_name { get; set; }
        public bool is_active { get; set; }
        public DateTime? modification_date { get; set; }
        public DateTime create_date { get; set; }

        public static Function Builder(IDataRecord record)
        {
            return new Function
            {
                o_function_id = record.GetInt32(record.GetOrdinal("o_function_id")),
                function_name = record.GetString(record.GetOrdinal("function_name")),
                is_active = record.GetInt32(record.GetOrdinal("is_active")) == 1,
                modification_date = record.IsDBNull(record.GetOrdinal("modification_date")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("modification_date")),
                create_date = record.GetDateTime(record.GetOrdinal("modification_date"))
            };
        }
    }
}
