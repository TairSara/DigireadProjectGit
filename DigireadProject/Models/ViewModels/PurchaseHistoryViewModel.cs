using System;

namespace DigireadProject.Models.ViewModels
{
    public class PurchaseHistoryViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ImageSrc { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}