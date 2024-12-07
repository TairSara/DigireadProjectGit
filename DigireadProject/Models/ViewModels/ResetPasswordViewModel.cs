using System.ComponentModel.DataAnnotations;
namespace DigireadProject.Models.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "נדרשת סיסמה חדשה")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "הסיסמה חייבת להכיל לפחות {2} תווים")]
        [DataType(DataType.Password)]
        [Display(Name = "סיסמה חדשה")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "אימות סיסמה")]
        [Compare("Password", ErrorMessage = "הסיסמאות אינן תואמות")]
        public string ConfirmPassword { get; set; }

        public string Token { get; set; }
    }
}