using System;
using System.Collections.Generic;

namespace DigireadProject.Models.ViewModels
{
    public class PopularBookViewModel
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string MainAuthor { get; set; }
        public string ImageSrc { get; set; }
        public string Description { get; set; }  
        public decimal OriginalPrice { get; set; }
        public Nullable<decimal> PurchasePrice { get; set; }

        public decimal AverageRating { get; set; }  
        public int ReviewCount { get; set; }
        public List<ReviewDetails> Reviews { get; set; }

        public PopularBookViewModel()
        {
            Reviews = new List<ReviewDetails>();
        }
    }

    public class ReviewDetails
    {
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime ReviewDate { get; set; }
    }

    public class HomeViewModel
    {
        public List<PopularBookViewModel> PopularBooks { get; set; }
        public List<PopularBookViewModel> OnSaleBooks { get; set; } 
        public List<WebsiteReviewViewModel> WebsiteReviews { get; set; }
        public List<PopularBookViewModel> RomanceSeries { get; set; }
        public List<PopularBookViewModel> SciFiSeries { get; set; }

        public HomeViewModel()
        {
            PopularBooks = new List<PopularBookViewModel>();
            OnSaleBooks = new List<PopularBookViewModel>(); 
            WebsiteReviews = new List<WebsiteReviewViewModel>();
            
        }
    }

    public class WebsiteReviewViewModel
    {
        public string Username { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }
        public DateTime ReviewDate { get; set; }
    }
}