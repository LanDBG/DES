using System.ComponentModel.DataAnnotations;

namespace DSE.DataAccess.Models
{
    public class UserLogin
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public string IP { get; set; }
        public int SubmitCount { get; set; }
        public string ErrorMessage { get; set; }
    }
}