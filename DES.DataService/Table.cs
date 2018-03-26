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
        public string QueryString { get; set; }
        public string API { get; set; }
        public bool Truncate { get; set; }

    }
    public class SubmitRequest
    {
        public bool Truncate { get; set; }
        public string Data { get; set; }
 

    }
}
