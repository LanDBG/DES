using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DSE.App.Models
{
    public class DataTableRequest
    {
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public List<object> columns { get; set; }
        public List<Order> order { get; set; }
        public Search search { get; set; }
        public string city { get; set; }
        public string price { get; set; }
        public string area { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public int businessId { get; set; } = -1;
        public string lang { get; set; } = "en";
        public int _ { get; set; }
    }

    public class Search
    {
        public string value { get; set; }
        public string regex { get; set; }
    }

    public class Order
    {
        public string column { get; set; }
        public string dir { get; set; }
    }

    public class Column
    {
        public string data { get; set; }
        public string name { get; set; }
        public bool searchable { get; set; }
        public bool orderable { get; set; }
        public Search search { get; set; }
    }
}