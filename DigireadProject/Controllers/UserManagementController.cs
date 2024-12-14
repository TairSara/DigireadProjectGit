using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DigireadProject.Models.ViewModels;

namespace DigireadProject.Controllers
{
    [Authorize]
    public class UserManagementController : Controller
    {
        private readonly libraryProject_digireadEntities db;

        public UserManagementController()
        {
            db = new libraryProject_digireadEntities();
        }

        public async Task<ActionResult> Index()
        {
            if (!(User.Identity.IsAuthenticated && await IsUserAdmin()))
            {
                return RedirectToAction("Login", "Account");
            }

            var users = await db.Users
                .Select(u => new UserManagementViewModel
                {
                    UserId = u.UserID,
                    Username = u.Username,
                    Email = u.Email,
                    IsActive = u.IsActive ?? false,
                    IsAdmin = u.IsAdmin ?? false,
                    RegistrationDate = u.RegistrationDate ?? DateTime.Now
                })
                .ToListAsync();

            return View(users);
        }

        private async Task<bool> IsUserAdmin()
        {
            var username = User.Identity.Name;
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            return user != null && user.IsAdmin == true;
        }

        [HttpPost]
        public async Task<ActionResult> ToggleStatus(int userId)
        {
            if (!await IsUserAdmin())
            {
                return Json(new { success = false, message = "אין הרשאת מנהל" });
            }

            var user = await db.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await db.SaveChangesAsync();
                return Json(new { success = true, isActive = user.IsActive });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<ActionResult> ToggleAdmin(int userId)
        {
            if (!await IsUserAdmin())
            {
                return Json(new { success = false, message = "אין הרשאת מנהל" });
            }

            var user = await db.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsAdmin = !user.IsAdmin;
                await db.SaveChangesAsync();
                return Json(new { success = true, isAdmin = user.IsAdmin });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteUser(int userId)
        {
            if (!await IsUserAdmin())
            {
                return Json(new { success = false, message = "אין הרשאת מנהל" });
            }

            var user = await db.Users.FindAsync(userId);
            if (user != null)
            {
                db.Users.Remove(user);
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<ActionResult> UpdateUser(int userId, string username, string email)
        {
            if (!await IsUserAdmin())
            {
                return Json(new { success = false, message = "אין הרשאת מנהל" });
            }

            var user = await db.Users.FindAsync(userId);
            if (user != null)
            {
                user.Username = username;
                user.Email = email;
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }
    }
}