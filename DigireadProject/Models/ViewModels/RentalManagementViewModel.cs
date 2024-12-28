using System;

namespace DigireadProject.Models.ViewModels
{
    public class RentalManagementViewModel
    {
        public int RentalID { get; set; }
        public string Username { get; set; }
        public string BookTitle { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime ReturnDeadline { get; set; }
        public int RemainingDays { get; set; }
        public string Status { get; set; }
    }

}