using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSE.DataAccess.Models
{
    public class Archive
    {
        [Display(Name = "Archived name")]
        public string ArchivedName { get; set; }
        public DateTime? Date { get; set; }

        public static Archive Builder(IDataRecord record)
        {
            var archived_name = record.GetString(record.GetOrdinal("archived_name"));
            var Date = record.IsDBNull(record.GetOrdinal("Date")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("Date"));

            return new Archive
            {
                ArchivedName = archived_name,
                Date = Date,
            };
        }
    }
}
