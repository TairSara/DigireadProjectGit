using System;

namespace DigireadProject.Models.ViewModels
{

    public class UserRentalViewModel
    {
        public int RentalID { get; set; }
        public int BookID { get; set; }
        public string BookTitle { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; }
        public int DaysOverdue { get; internal set; }
        public string ImageSrc { get; set; }

    }
}