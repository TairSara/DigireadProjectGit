using System;

namespace DigireadProject.Models.ViewModels
{
    public class WaitListManagementViewModel
    {
        public int WaitListID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string BookTitle { get; set; }
        public int WaitPosition { get; set; }
        public DateTime AddedDate { get; set; }
        public bool EmailNotificationSent { get; set; }
    }
}