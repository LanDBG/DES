using DSE.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace DSE.App.Models
{
    public class SourceMappingModel
    {
        public Guid SourceId { get; set; }
        public string SourceName { get; set; }
        public List<SourceMapping> MappingData { get; set; }
        public int SourceType { get; set; }
        public DataTable MappingTable { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}