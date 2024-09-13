using System.ComponentModel.DataAnnotations;

namespace AuthTask.ViewModels
{
    public class UserLogin
    {
        [Required, DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
