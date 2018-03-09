using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSE.DataAccess.Models
{
    public class KpiDataType
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }

        public static KpiDataType Builder(IDataRecord record)
        {
            var tableName = record.IsDBNull(record.GetOrdinal("TABLE_NAME")) ? string.Empty : record.GetString(record.GetOrdinal("TABLE_NAME"));
            var columnName = record.IsDBNull(record.GetOrdinal("COLUMN_NAME")) ? string.Empty : record.GetString(record.GetOrdinal("COLUMN_NAME"));
            var dataType = record.IsDBNull(record.GetOrdinal("DATA_TYPE")) ? string.Empty : record.GetString(record.GetOrdinal("DATA_TYPE"));

            return new KpiDataType
            {
                TableName = tableName,
                ColumnName = columnName,
                DataType = dataType
            };
        }
    }
}
