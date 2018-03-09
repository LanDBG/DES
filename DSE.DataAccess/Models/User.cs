using System;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace DSE.DataAccess.Models
{
    public class User
    {
        public string ErrorMessage { get; set; }

        public Guid? UserId { get; set; }

        [Display(Name = "Client")]
        public int? ClientId { get; set; }

        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4,6})$", ErrorMessage = "Not a valid Phone number")]
        public string Phone { get; set; }

        [Display(Name = "Active")]
        public bool? IsActive { get; set; }

        [Display(Name = "Last login date")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "Modification date")]
        public DateTime? ModificationDate { get; set; }

        [Display(Name = "Create date")]
        public DateTime? CreateDate { get; set; }

        public bool Updated { get; set; }
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }
        public int ErrorCode { get; internal set; }

        public static User Builder(IDataRecord record)
        {
            var UserId = record.GetGuid(record.GetOrdinal("o_user_id"));
            var ClientId = record.IsDBNull(record.GetOrdinal("o_client_id")) ? null : (int?)record.GetInt32(record.GetOrdinal("o_client_id"));
            var FirstName = record.IsDBNull(record.GetOrdinal("first_name")) ? string.Empty : record.GetString(record.GetOrdinal("first_name"));
            var LastName = record.IsDBNull(record.GetOrdinal("last_name")) ? string.Empty : record.GetString(record.GetOrdinal("last_name"));
            var UserName = record.GetString(record.GetOrdinal("user_name"));
            var Password = record.GetString(record.GetOrdinal("user_name"));
            var Address = record.IsDBNull(record.GetOrdinal("address")) ? string.Empty : record.GetString(record.GetOrdinal("address"));
            var Email = record.IsDBNull(record.GetOrdinal("email")) ? string.Empty : record.GetString(record.GetOrdinal("email"));
            var Phone = record.IsDBNull(record.GetOrdinal("phone")) ? string.Empty : record.GetString(record.GetOrdinal("phone"));
            var IsActive = record.GetByte(record.GetOrdinal("is_active")) != 0;
            var LastLoginDate = record.IsDBNull(record.GetOrdinal("last_login_date")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("last_login_date"));
            var ModificationDate = record.IsDBNull(record.GetOrdinal("modification_date")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("modification_date"));
            var CreateDate = record.IsDBNull(record.GetOrdinal("create_date")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("create_date"));

            return new User
            {
                UserId = UserId,
                ClientId = ClientId,
                FirstName = FirstName,
                LastName = LastName,
                UserName = UserName,
                Password = Password,
                Address = Address,
                Email = Email,
                Phone = Phone,
                IsActive = IsActive,
                LastLoginDate = LastLoginDate,
                ModificationDate = ModificationDate,
                CreateDate = CreateDate
            };
        }
    }
}