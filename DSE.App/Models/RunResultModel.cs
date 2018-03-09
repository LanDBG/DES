using DSE.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace DSE.App.Models
{
    public class RunResultModel
    {
        public Guid SourceId { get; set; }
        public string SourceName { get; set; }
        public List<ValidateItem> ValidationResult { get; set; }

        public DataTable ValidationTable { get; set; }

        public DateTime LastUpdate { get; set; }
        public bool Success { get; set; }

        public Source Source { get; set; }
    }
}