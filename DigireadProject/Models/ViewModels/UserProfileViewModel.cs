using System;
using System.ComponentModel.DataAnnotations;

namespace DigireadProject.Models.ViewModels
{
    public class UserProfileViewModel
    {
        [Display(Name = "שם משתמש")]
        public string Username { get; set; }

        [Required(ErrorMessage = "שדה אימייל הוא חובה")]
        [EmailAddress(ErrorMessage = "אנא הזן כתובת אימייל תקינה")]
        [Display(Name = "אימייל")]
        public string Email { get; set; }

        [Display(Name = "תאריך הרשמה")]
        public DateTime RegistrationDate { get; set; }

        [Display(Name = "הרשאות מנהל")]
        public bool IsAdmin { get; set; }
    }
}