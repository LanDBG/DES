using System;
using System.Data;

namespace DSE.DataAccess.Models
{
    public class SourceMapping
    {
        public string Source_view { get; set; }

        public string Source_table { get; set; }

        public string Source_column { get; set; }

        public string Source_ref_table { get; set; }

        public string Source_ref_column { get; set; }

        public string Target_table { get; set; }

        public string Target_column { get; set; }

        public string Target_datatype { get; set; }

        public bool? Target_is_primary_id { get; set; }

        public bool? Target_is_allow_not_null { get; set; }

        public bool? Target_is_duplicate_value { get; set; }

        public bool? Target_is_numeric { get; set; }

        public bool? Target_is_date { get; set; }

        public static SourceMapping Builder(IDataRecord record)
        {
            return new SourceMapping
            {
                Source_view = record.IsDBNull(record.GetOrdinal("source_view")) ? null : record.GetString(record.GetOrdinal("source_view")),
                Source_table = record.IsDBNull(record.GetOrdinal("source_table")) ? null : record.GetString(record.GetOrdinal("source_table")),
                Source_column = record.IsDBNull(record.GetOrdinal("source_column")) ? null : record.GetString(record.GetOrdinal("source_column")),
                Source_ref_table = record.IsDBNull(record.GetOrdinal("source_ref_table")) ? null : record.GetString(record.GetOrdinal("source_ref_table")),
                Source_ref_column = record.IsDBNull(record.GetOrdinal("source_ref_column")) ? null : record.GetString(record.GetOrdinal("source_ref_column")),
                Target_table = record.IsDBNull(record.GetOrdinal("target_table")) ? null : record.GetString(record.GetOrdinal("target_table")),
                Target_column = record.IsDBNull(record.GetOrdinal("target_column")) ? null : record.GetString(record.GetOrdinal("target_column")),
                Target_datatype = record.IsDBNull(record.GetOrdinal("target_datatype")) ? null : record.GetString(record.GetOrdinal("target_datatype")),
                Target_is_primary_id = record.IsDBNull(record.GetOrdinal("target_is_primary_id")) ? false : record.GetBoolean(record.GetOrdinal("target_is_primary_id")),
                Target_is_allow_not_null = record.IsDBNull(record.GetOrdinal("target_is_allow_not_null")) ? false : record.GetBoolean(record.GetOrdinal("target_is_allow_not_null")),
                Target_is_duplicate_value = record.IsDBNull(record.GetOrdinal("target_is_duplicate_value")) ? false : record.GetBoolean(record.GetOrdinal("target_is_duplicate_value")),
                Target_is_numeric = record.IsDBNull(record.GetOrdinal("target_is_numeric")) ? false : record.GetBoolean(record.GetOrdinal("target_is_numeric")),
                Target_is_date = record.IsDBNull(record.GetOrdinal("target_is_date")) ? false : record.GetBoolean(record.GetOrdinal("target_is_date"))
            };
        }
    }
}
