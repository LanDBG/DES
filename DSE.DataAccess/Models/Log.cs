using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSE.DataAccess.Models
{
    public class Log
    {
        public Guid Id { get; set; }

        public string TaskName { get; set; }

        public string Status { get; set; }

        public DateTime? ActionTime { get; set; }

        public string Description { get; set; }

        public Guid? ServiceID { get; set; }

        public static Log Builder(IDataRecord record)
        {
            return new Log
            {
                Id = record.GetGuid(record.GetOrdinal("Id")),
                TaskName = record.GetString(record.GetOrdinal("TaskName")),
                Status = record.GetString(record.GetOrdinal("Status")),
                ActionTime = record.GetDateTime(record.GetOrdinal("ActionTime")),
                Description = record.GetString(record.GetOrdinal("Description")),
                ServiceID = record.GetGuid(record.GetOrdinal("ServiceID"))
            };
        }
    }

}
