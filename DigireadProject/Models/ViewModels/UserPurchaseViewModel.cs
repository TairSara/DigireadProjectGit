using System;

namespace DigireadProject.Models.ViewModels
{
    public class UserPurchaseViewModel
    {
        public int PurchaseID { get; set; }
        public int? BookID { get; set; }
        public int? UserID { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public bool? PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }

        // Navigation properties
        public virtual Users User { get; set; }
        public virtual Books Book { get; set; }
    }
}