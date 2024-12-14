using System;
using System.ComponentModel.DataAnnotations;

namespace DigireadProject.Models.ViewModels
{
    public class UserProfileViewModel
    {
        [Display(Name = "שם משתמש")]
        [Required(ErrorMessage = "שדה שם משתמש הוא חובה")]
        [StringLength(50, ErrorMessage = "שם המשתמש צריך להיות באורך של עד 50 תווים")]
        public string Username { get; set; }

        [Required(ErrorMessage = "שדה אימייל הוא חובה")]
        [EmailAddress(ErrorMessage = "אנא הזן כתובת אימייל תקינה")]
        [Display(Name = "אימייל")]
        [StringLength(100, ErrorMessage = "כתובת האימייל צריכה להיות באורך של עד 100 תווים")]
        public string Email { get; set; }

        [Display(Name = "תאריך הרשמה")]
        public DateTime RegistrationDate { get; set; }

        [Display(Name = "הרשאות מנהל")]
        public bool IsAdmin { get; set; }

        [Display(Name = "סיסמה נוכחית")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Display(Name = "סיסמה חדשה")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "הסיסמה צריכה להיות באורך של לפחות 6 תווים", MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Display(Name = "אימות סיסמה חדשה")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "הסיסמאות החדשות אינן תואמות")]
        public string ConfirmNewPassword { get; set; }
    }
}