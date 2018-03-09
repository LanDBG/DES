using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSE.DataAccess.Models
{
    public class LogViewModel
    {
        [Display(Name = "Job Id")]
        public Guid o_job_id { get; set; }

        [Display(Name = "Job")]
        public string job_name { get; set; }

        [Display(Name = "Status")]
        public string run_status { get; set; }

        [Display(Name = "Date")]
        public DateTime run_date { get; set; }

        [Display(Name = "Time")]
        public string run_time { get; set; }

        [Display(Name = "Duration")]
        public string run_duration { get; set; }

        [Display(Name = "User")]
        public string user_name { get; set; }

        public static LogViewModel Builder(IDataRecord record)
        {
            var o_job_id = record.GetGuid(record.GetOrdinal("o_job_id"));
            var job_name = record.GetString(record.GetOrdinal("job_name"));
            var run_status = record.GetString(record.GetOrdinal("run_status"));
            var run_date = record.GetDateTime(record.GetOrdinal("run_date"));
            var run_time = record["run_time"].ToString();
            var run_duration = record["run_duration"]?.ToString() ?? string.Empty;
            var user_name = record["user_name"]?.ToString() ?? string.Empty;
            return new LogViewModel
            {
                o_job_id = o_job_id,
                job_name = job_name,
                run_status = run_status,
                run_date = run_date,
                run_time = run_time,
                run_duration = run_duration,
                user_name = user_name
            };
        }
    }
}
