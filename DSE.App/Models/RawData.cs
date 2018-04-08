using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSE.App.Models
{
    public class RawData
    {
        public bool Truncate { get; set; }
        public string Data { get; set; }
    }
    public class TableSetting
    {
        public string ConfigName { get; set; }
        public string WhereColumn { get; set; }
        public string StartValue { get; set; }
        public string EndValue { get; set; }
        public int Sequence { get; set; }
        public string LastValue { get; set; }

        public string Columns { get; set; }
        public string SourceTable { get; set; }
        public string PrimaryKey { get; set; }
        public int RowPerRound { get; set; }
        public bool Truncate { get; set; }
        public bool Active { get; set; }
    }
}