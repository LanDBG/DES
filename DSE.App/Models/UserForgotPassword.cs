using System.ComponentModel.DataAnnotations;

namespace DSE.App.Models
{
    public class UserForgotPassword
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}