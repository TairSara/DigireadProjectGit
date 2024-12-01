using System.ComponentModel.DataAnnotations;

namespace DigireadProject.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "שדה שם משתמש הינו חובה")]
        [Display(Name = "שם משתמש")]
        public string Username { get; set; }

        [Required(ErrorMessage = "שדה סיסמה הינו חובה")]
        [DataType(DataType.Password)]
        [Display(Name = "סיסמה")]
        public string Password { get; set; }

        [Display(Name = "זכור אותי")]
        public bool RememberMe { get; set; }
    }
}