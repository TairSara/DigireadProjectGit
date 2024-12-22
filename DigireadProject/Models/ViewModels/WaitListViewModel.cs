using System;

namespace DigireadProject.Models.ViewModels
{
    public class WaitListViewModel
    {
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
        public int StockQuantity { get; set; }        // לרכישה
        public int StockQuantityRent { get; set; }    // להשאלה

        // מאפיין חדש שבודק אם הספר זמין בהתאם לסוג ההמתנה
        public bool IsActuallyAvailable 
        {
            get
            {
                if (IsRental)
                {
                    return StockQuantityRent > 0;  // בודק רק את מלאי ההשאלות
                }
                return StockQuantity > 0;  // בודק רק את מלאי הרכישות
            }
        }
        
        // מאפיין חדש שבודק אם הספר זמין למשתמש הספציפי
        public bool IsAvailableForUser 
        {
            get
            {
                // הספר זמין רק אם המשתמש במקום הראשון והספר במלאי
                if (WaitPosition == 1)
                {
                    if (IsRental)
                    {
                        return StockQuantityRent > 0;
                    }
                    return StockQuantity > 0;
                }
                return false;  // לא זמין למשתמשים במקום 2 ומעלה
            }
        }
    }
        
    }
