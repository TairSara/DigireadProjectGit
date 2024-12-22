using System.ComponentModel.DataAnnotations;

namespace DigireadProject.Models
{
    public class PaymentViewModel
    {
        [Required(ErrorMessage = "אנא הזן מספר כרטיס")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "מספר כרטיס אשראי צריך להכיל 16 ספרות")]
        [Display(Name = "מספר כרטיס אשראי")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "אנא הזן חודש תפוגה")]
        [Range(1, 12, ErrorMessage = "חודש תפוגה לא תקין")]
        [Display(Name = "חודש תפוגה")]
        public int ExpiryMonth { get; set; }

        [Required(ErrorMessage = "אנא הזן שנת תפוגה")]
        [Range(24, 99, ErrorMessage = "שנת תפוגה לא תקינה")]
        [Display(Name = "שנת תפוגה")]
        public int ExpiryYear { get; set; }

        [Required(ErrorMessage = "אנא הזן CVV")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV חייב להכיל 3 ספרות")]
        [Display(Name = "CVV")]
        public string Cvv { get; set; }

        [Display(Name = "סה״כ לתשלום")]
        public decimal TotalAmount { get; set; }
    }
}