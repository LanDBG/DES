using System.ComponentModel.DataAnnotations;

namespace DSE.DataAccess.Models
{
    public class UserChangePassword
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Old password")]
        public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password ")]
        public string ConfirmNewPassword { get; set; }

        public int Updated { get; set; }
        public string ErrorMessage { get; internal set; }
    }

    public enum ChanPasswordStatus
    {
        UnSuccess = 0,
        Duplicate = 1,
        WrongConfirm = 2,
        Success = 3
    }
}
