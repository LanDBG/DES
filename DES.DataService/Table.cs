using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DES.DataService
{
    public class Table
    {
        public string Name { get; set; }
        
        public string API { get; set; }
        public string GetSetting { get; set; }
        public string UpdateSetting { get; set; }

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
