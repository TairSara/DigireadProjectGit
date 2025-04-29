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

        [Required(ErrorMessage = "שדה סיסמה הינו חובה")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "הסיסמה חייבת להיות לפחות 8 תווים")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "הסיסמה חייבת להכיל אות גדולה, אות קטנה, מספר ותו מיוחד")]
        [DataType(DataType.Password)]
        [Display(Name = "סיסמה")]
        public string Password { get; set; }

        [Required(ErrorMessage = "יש לאמת את הסיסמה")]
        [Compare("Password", ErrorMessage = "הסיסמאות אינן תואמות")]
        [DataType(DataType.Password)]
        [Display(Name = "אימות סיסמה")]
        public string ConfirmPassword { get; set; }

        // 🆕 שדות חדשים

        [Required(ErrorMessage = "שדה שם פרטי הינו חובה")]
        [Display(Name = "שם פרטי")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "שדה שם משפחה הינו חובה")]
        [Display(Name = "שם משפחה")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "שדה תעודת זהות הינו חובה")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "תעודת זהות חייבת להיות 9 ספרות")]
        [Display(Name = "תעודת זהות")]
        public string IDNumber { get; set; }

        [Required(ErrorMessage = "שדה מספר כרטיס אשראי הינו חובה")]
        [RegularExpression(@"^\d{13,19}$", ErrorMessage = "מספר כרטיס אשראי חייב להיות בין 13 ל-19 ספרות")]
        [Display(Name = "מספר כרטיס אשראי")]
        public string CreditCardNumber { get; set; }

        [Required(ErrorMessage = "שדה תוקף הכרטיס הינו חובה")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "התוקף חייב להיות בפורמט MM/YY")]
        [Display(Name = "תוקף כרטיס אשראי")]
        public string ValidDate { get; set; }

        [Required(ErrorMessage = "שדה CVC הינו חובה")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVC חייב להיות בדיוק 3 ספרות")]
        [Display(Name = "CVC")]
        public string CVC { get; set; }
    }
}


