using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DigireadProject.Models.ViewModels
{
    public class PaymentViewModel
    {
        public PaymentViewModel()
        {
            CartItems = new List<CartItemViewModel>();
        }

        [Required(ErrorMessage = "נא להזין מספר כרטיס אשראי")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "מספר כרטיס אשראי חייב להכיל 16 ספרות בדיוק")]
        [CreditCardValidation(ErrorMessage = "מספר כרטיס אשראי לא תקין")]
        [Display(Name = "מספר כרטיס אשראי")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "נא להזין שם בעל הכרטיס")]
        [RegularExpression(@"^[\u0590-\u05FF\s]{2,50}$", ErrorMessage = "יש להזין שם תקין בעברית")]
        [Display(Name = "שם בעל הכרטיס")]
        [MinLength(2, ErrorMessage = "שם חייב להכיל לפחות 2 תווים")]
        [MaxLength(50, ErrorMessage = "שם ארוך מדי")]
        public string CardHolderName { get; set; }

        [Required(ErrorMessage = "נא להזין תוקף")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "תוקף לא תקין (MM/YY)")]
        [ExpiryDateValidation(ErrorMessage = "תאריך התפוגה חייב להיות בעתיד")]
        [Display(Name = "תוקף")]
        public string ExpiryDate { get; set; }

        [Required(ErrorMessage = "נא להזין קוד CVV")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "קוד CVV חייב להכיל 3 ספרות בדיוק")]
        [Display(Name = "CVV")]
        public string CVV { get; set; }

        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string BookImageSrc { get; set; }
        [Required(ErrorMessage = "סכום לתשלום לא תקין")]
        [Range(0.01, 10000, ErrorMessage = "סכום לתשלום חייב להיות בין 0.01 ל-10000")]
        public decimal? Price { get; set; }
        public bool IsRental { get; set; }
        public List<CartItemViewModel> CartItems { get; set; }
    }

    // בדיקת תקינות לתאריך תפוגה
    public class ExpiryDateValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null) return false;

            string expiryDate = value.ToString();
            if (!Regex.IsMatch(expiryDate, @"^(0[1-9]|1[0-2])\/([0-9]{2})$"))
                return false;

            try
            {
                string[] parts = expiryDate.Split('/');
                int month = int.Parse(parts[0]);
                int year = int.Parse("20" + parts[1]);

                DateTime cardExpiry = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
                return cardExpiry > DateTime.Now;
            }
            catch
            {
                return false;
            }
        }
    }

    // בדיקת תקינות למספר כרטיס אשראי (אלגוריתם Luhn)
    public class CreditCardValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null) return false;

            string cardNumber = value.ToString().Replace(" ", "");
            if (!Regex.IsMatch(cardNumber, @"^\d{16}$")) return false;

            int sum = 0;
            bool alternate = false;
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int n = int.Parse(cardNumber[i].ToString());
                if (alternate)
                {
                    n *= 2;
                    if (n > 9)
                    {
                        n = (n % 10) + 1;
                    }
                }
                sum += n;
                alternate = !alternate;
            }

            return (sum % 10 == 0);
        }
    }
}