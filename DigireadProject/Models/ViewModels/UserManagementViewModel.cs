using System;

namespace DigireadProject.Models.ViewModels
{
    public class UserManagementViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IDNumber { get; set; }
        public string CreditCardNumber { get; set; }
        public string ValidDate { get; set; }
        public string CVC { get; set; }
    }

}