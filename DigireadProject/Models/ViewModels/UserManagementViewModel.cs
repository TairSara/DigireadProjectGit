using System;

namespace DigireadProject.Models.ViewModels
{
    public class UserManagementViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}