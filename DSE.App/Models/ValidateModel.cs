using DSE.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace DSE.App.Models
{
    public class ValidateModel
    {
        public int SourceType { get; set; }

        public Guid SourceId { get; set; }
        public string SourceName { get; set; }
        public List<ValidateItem> ValidationResult { get; set; }
        public DataTable ValidationTable { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}