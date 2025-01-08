using System;

namespace DigireadProject.Models.ViewModels
{
    public class WaitListViewModel
    {
        // קיים כבר
        public int WaitListID { get; set; }
        public int BookID { get; set; }
        public int UserID { get; set; }
        public int WaitPosition { get; set; }
        public DateTime? AddedDate { get; set; }
        public bool? EmailNotificationSent { get; set; }
        public string BookTitle { get; set; }
        public string ImageSrc { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsRental { get; set; }
        public int StockQuantity { get; set; }        
        public int StockQuantityRent { get; set; }    

        public DateTime? ReservationExpiryTime { get; set; } 
        public bool IsReserved { get; set; } 
    
        public bool IsActuallyAvailable 
        {
            get
            {
                if (IsRental)
                {
                    return StockQuantityRent > 0;  
                }
                return StockQuantity > 0;  
            }
        }
    
        public bool IsAvailableForUser 
        {
            get
            {
                if (WaitPosition == 1 && IsReserved)
                {
                    if (IsRental)
                    {
                        return StockQuantityRent > 0 && DateTime.Now <= ReservationExpiryTime;
                    }
                    return StockQuantity > 0 && DateTime.Now <= ReservationExpiryTime;
                }
                return false;
            }
        }
    }
        
    }
