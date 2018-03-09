using System;
using System.Data;

namespace DSE.DataAccess.Models
{
    public class Role
    {
        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public bool IsActive { get; set; }

        public DateTime? ModificationDate { get; set; }

        public DateTime CreateDate { get; set; }

        public static Role Builder(IDataRecord record)
        {
            return new Role
            {
                RoleId = record.GetInt32(record.GetOrdinal("o_role_id")),
                RoleName = record.GetString(record.GetOrdinal("role_name")),
                IsActive = record.GetInt32(record.GetOrdinal("is_active")) == 1,
                ModificationDate = record.IsDBNull(record.GetOrdinal("modification_date")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("modification_date")),
                CreateDate = record.GetDateTime(record.GetOrdinal("modification_date"))
            };
        }
    }
}
