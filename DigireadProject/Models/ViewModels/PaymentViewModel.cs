using System.ComponentModel.DataAnnotations;

namespace DigireadProject.Models.ViewModels
{
    public class PaymentViewModel
    {
        [Required(ErrorMessage = "נא להזין מספר כרטיס אשראי")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "מספר כרטיס אשראי לא תקין")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "נא להזין שם בעל הכרטיס")]
        public string CardHolderName { get; set; }

        [Required(ErrorMessage = "נא להזין תוקף")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "תוקף לא תקין (MM/YY)")]
        public string ExpiryDate { get; set; }

        [Required(ErrorMessage = "נא להזין קוד CVV")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "קוד CVV לא תקין")]
        public string CVV { get; set; }
        
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string BookImageSrc { get; set; }
        public decimal Price { get; set; }
        public bool IsRental { get; set; }
    }
}