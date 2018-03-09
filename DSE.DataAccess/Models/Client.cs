using System;
using System.Data;

namespace DSE.DataAccess.Models
{
    public class Client
    {
        public int ClientId { get; set; }

        public string ClientName { get; set; }

        public bool IsActive { get; set; }

        public DateTime? ModificationDate { get; set; }

        public DateTime CreateDate { get; set; }

        public static Client Builder(IDataRecord record)
        {
            return new Client
            {
                ClientId = record.GetInt32(record.GetOrdinal("o_client_id")),
                ClientName = record.GetString(record.GetOrdinal("client_name")),
                IsActive = record.GetInt32(record.GetOrdinal("is_active")) == 1,
                ModificationDate = record.IsDBNull(record.GetOrdinal("modification_date")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("modification_date")),
                CreateDate = record.GetDateTime(record.GetOrdinal("modification_date"))
            };
        }
    }
}