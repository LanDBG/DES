using System;

namespace DSE.DataAccess.Models
{
    public class DSEUser
    {

        public Guid O_user_id { get; set; }

        public int O_role_id { get; set; }

        public int? O_client_id { get; set; }

        public string First_name { get; set; }

        public string Last_name { get; set; }

        public string User_name { get; set; }

        public byte[] Password { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public byte? Is_active { get; set; }

        public DateTime? Last_login_date { get; set; }

        public DateTime? Modification_date { get; set; }

        public DateTime? Create_date { get; set; }
    }
}
