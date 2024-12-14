using System.ComponentModel.DataAnnotations;

namespace DigireadProject.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "שדה שם משתמש הינו חובה")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "שם המשתמש חייב להיות בין 3 ל-50 תווים")]
        [RegularExpression(@"^[א-תa-zA-Z0-9._]+$", ErrorMessage = "שם משתמש יכול להכיל רק אותיות, מספרים ונקודות")]
        [Display(Name = "שם משתמש")]
        public string Username { get; set; }

        [Required(ErrorMessage = "שדה אימייל הינו חובה")]
        [EmailAddress(ErrorMessage = "כתובת האימייל אינה בפורמט תקין")]
        [Display(Name = "אימייל")]
        public string Email { get; set; }

        [Required(ErrorMessage = "שדה סיסמא הינו חובה")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "הסיסמא חייבת להיות לפחות 8 תווים")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "הסיסמא חייבת להכיל לפחות: אות גדולה, אות קטנה, מספר ותו מיוחד")]
        [DataType(DataType.Password)]
        [Display(Name = "סיסמא")]
        public string Password { get; set; }

        [Required(ErrorMessage = "יש לאמת את הסיסמא")]
        [Compare("Password", ErrorMessage = "הסיסמאות אינן תואמות")]
        [DataType(DataType.Password)]
        [Display(Name = "אימות סיסמא")]
        public string ConfirmPassword { get; set; }
    }
}