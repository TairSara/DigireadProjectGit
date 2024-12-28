using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DigireadProject.Models.ViewModels;

namespace DigireadProject.Controllers
{
   public class RentalsManagementController : Controller
   {
       private readonly libraryProject_digireadEntities db;
       private readonly EmailService _emailService;

       public RentalsManagementController()
       {
           db = new libraryProject_digireadEntities();
           _emailService = new EmailService();
       }

       [Authorize]
       public async Task<ActionResult> ManageRentals()
       {
           if (!await IsUserAdmin())
           {
               return RedirectToAction("Login", "Account");
           }

           var rentals = await db.Rentals
               .Include(r => r.Books)
               .Include(r => r.Users)
               .ToListAsync();

           var rentalViewModels = rentals.Select(r => new RentalManagementViewModel
           {
               RentalID = r.RentalID,
               Username = r.Users.Username,
               BookTitle = r.Books.Title,
               RentalDate = r.RentalDate ?? DateTime.Now,
               ReturnDeadline = r.RentalDate.HasValue ? r.RentalDate.Value.AddDays(31) : DateTime.Now,
               RemainingDays = r.RentalDate.HasValue ? (int)(r.RentalDate.Value.AddDays(31) - DateTime.Now).TotalDays : 0,
               Status = r.ReturnDate.HasValue ? "הוחזר" :
                        (r.RentalDate.HasValue && r.RentalDate.Value.AddDays(31) < DateTime.Now ? "פג תוקף" : "פעיל")
           })
           .ToList();

           return View(rentalViewModels);
       }

       [Authorize]
       public async Task<ActionResult> ManageWaitList()
       {
           if (!await IsUserAdmin())
           {
               return RedirectToAction("Login", "Account");
           }

           var waitList = await db.WaitList
               .Include(w => w.Books)
               .Include(w => w.Users)
               .Select(w => new WaitListManagementViewModel
               {
                   WaitListID = w.WaitListID,
                   Username = w.Users.Username,
                   Email = w.Users.Email,
                   BookTitle = w.Books.Title,
                   WaitPosition = w.WaitPosition ?? 0,
                   AddedDate = w.AddedDate ?? DateTime.Now,
                   EmailNotificationSent = w.EmailNotificationSent ?? false
               })
               .OrderBy(w => w.WaitPosition)
               .ToListAsync();

           return View(waitList);
       }

       private async Task<bool> IsUserAdmin()
       {
           var username = User.Identity.Name;
           var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
           return user != null && user.IsAdmin == true;
       }
   }
}