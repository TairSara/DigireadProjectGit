using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DigireadProject.Models;

namespace DigireadProject.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly libraryProject_digireadEntities db;

        public AdminController()
        {
            db = new libraryProject_digireadEntities();
        }

        public async Task<ActionResult> Dashboard()
        {
            if (!await IsUserAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // סך כל המשתמשים
                ViewBag.TotalUsers = await db.Users.CountAsync();
                // ספירת השאלות הפעילות
                ViewBag.TotalRentals = await db.Rentals
                    .CountAsync(r => r.ReturnDate == null);
                // סך כל הספרים
                ViewBag.TotalBooks = await db.Books.CountAsync();

                // משתמשים פעילים (IsActive = 1)
                ViewBag.ActiveUsers = await db.Users
                    .Where(u => u.IsActive.HasValue && u.IsActive.Value)
                    .CountAsync();

                // מנהלי מערכת (IsAdmin = 1)
                ViewBag.AdminUsers = await db.Users
                    .Where(u => u.IsAdmin.HasValue && u.IsAdmin.Value)
                    .CountAsync();

                // משתמשים חדשים מהיום
                var today = DateTime.Today;
                ViewBag.NewUsersToday = await db.Users
                    .CountAsync(u => u.RegistrationDate.HasValue &&
                                   DbFunctions.TruncateTime(u.RegistrationDate) == today);

                // לוג לבדיקה
                System.Diagnostics.Debug.WriteLine($"Total Users: {ViewBag.TotalUsers}");
                System.Diagnostics.Debug.WriteLine($"Active Users: {ViewBag.ActiveUsers}");
                System.Diagnostics.Debug.WriteLine($"Admin Users: {ViewBag.AdminUsers}");
                System.Diagnostics.Debug.WriteLine($"New Users Today: {ViewBag.NewUsersToday}");
            }
            catch (Exception ex)
            {
                // לוג השגיאה
                System.Diagnostics.Debug.WriteLine($"Error in Dashboard: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                // ערכי ברירת מחדל במקרה של שגיאה
                ViewBag.TotalUsers = 0;
                ViewBag.TotalBooks = 0;
                ViewBag.ActiveUsers = 0;
                ViewBag.AdminUsers = 0;
                ViewBag.NewUsersToday = 0;
            }

            return View();
        }

        private async Task<bool> IsUserAdmin()
        {
            var username = User.Identity.Name;
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            return user != null && user.IsAdmin.HasValue && user.IsAdmin.Value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}