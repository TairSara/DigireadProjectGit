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
        
        // Navigation Properties
        public virtual Books Book { get; set; }
        public virtual Users User { get; set; }
        
    }
}