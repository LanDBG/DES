using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSE.DataAccess.Models
{
    public class LogDetailViewModel
    {
        [Display(Name = "Job")]
        public string job_name { get; set; }

        [Display(Name = "Step")]
        public string step_name { get; set; }

        [Display(Name = "Status")]
        public string run_status { get; set; }

        [Display(Name = "Duration")]
        public string run_duration { get; set; }

        public static LogDetailViewModel Builder(IDataRecord record)
        {
            var job_name = record.GetString(record.GetOrdinal("job_name"));
            var step_name = record.GetString(record.GetOrdinal("step_name"));
            var run_status = record.GetString(record.GetOrdinal("run_status"));
            var run_duration = record["run_duration"]?.ToString() ?? string.Empty;

            return new LogDetailViewModel
            {
                job_name = job_name,
                step_name = step_name,
                run_status = run_status,
                run_duration = run_duration,
            };
        }
    }
}
