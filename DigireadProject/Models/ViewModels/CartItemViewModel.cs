namespace DigireadProject.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string BookImageSrc { get; set; }
        public decimal Price { get; set; }
        public decimal PurchasePrice { get; set; }  
        public decimal RentalPrice { get; set; }    
        public int Quantity { get; set; }
        public bool IsRental { get; set; }
        public bool CanBeRented { get; set; }       
    }
}