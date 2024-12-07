using System.ComponentModel.DataAnnotations;

namespace DigireadProject.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "נדרש אימייל")]
        [EmailAddress(ErrorMessage = "אימייל לא תקין")]
        [Display(Name = "אימייל")]
        public string Email { get; set; }
    }
}