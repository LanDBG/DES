using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSE.DataAccess.Models
{
    public class ValidateItem
    {
        [Display(Name = "Table Name")]
        public string TableName { get; set; }
               
        [Display(Name = "Column Name")]
        public string ColumnName { get; set; }

        [Display(Name = "Data Value")]
        public string DataValue { get; set; }

        [Display(Name = "Data Issue")]
        public string DataIssue { get; set; }

        [Display(Name = "Comment")]
        public string Comment { get; set; }

        [Display(Name = "Number of records")]
        public string NumOfRecord { get; set; }

        [Display(Name = "Create date")]
        public DateTime CreateDate { get; set; }

        public static ValidateItem Builder(IDataRecord record)
        {
            return new ValidateItem
            {
                TableName = record.IsDBNull(record.GetOrdinal("table_name")) ? null : record.GetString(record.GetOrdinal("table_name")),
                ColumnName = record.IsDBNull(record.GetOrdinal("column_name")) ? null : record.GetString(record.GetOrdinal("column_name")),
                DataValue = record.IsDBNull(record.GetOrdinal("data_value")) ? null : record.GetString(record.GetOrdinal("data_value")),
                DataIssue = record.IsDBNull(record.GetOrdinal("data_issue")) ? null : record.GetString(record.GetOrdinal("data_issue")),
                NumOfRecord = record.IsDBNull(record.GetOrdinal("number_of_record")) ? null : record.GetString(record.GetOrdinal("number_of_record")),
                Comment = record.IsDBNull(record.GetOrdinal("comment")) ? null : record.GetString(record.GetOrdinal("comment")),
                CreateDate = record.GetDateTime(record.GetOrdinal("create_date"))
            };
        }
    }
}
