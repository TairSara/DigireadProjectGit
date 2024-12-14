using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Text;
using System.Security.Cryptography;
using System.Web.Security;
using System.Diagnostics;
using DigireadProject.Models.ViewModels;

namespace DigireadProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly libraryProject_digireadEntities db;

        public AccountController()
        {
            db = new libraryProject_digireadEntities();
            db.Database.Log = message => Debug.WriteLine(message);
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            Debug.WriteLine("Starting Registration Process");
            Debug.WriteLine($"Received Data - Username: {model.Username}, Email: {model.Email}");

            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Debug.WriteLine($"Validation Error: {modelError.ErrorMessage}");
                    }
                    return View(model);
                }

                var existingUsername = await db.Users
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == model.Username.ToLower());

                if (existingUsername != null)
                {
                    ModelState.AddModelError("Username", "שם המשתמש כבר קיים במערכת");
                    return View(model);
                }

                var existingEmail = await db.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "כתובת האימייל כבר קיימת במערכת");
                    return View(model);
                }

                var newUser = new Users
                {
                    Username = model.Username.Trim(),
                    Email = model.Email.Trim(),
                    Password = HashPassword(model.Password),
                    RegistrationDate = DateTime.Now,
                    IsActive = true,
                    IsAdmin = false,
                    PasswordReset = null
                };

                Debug.WriteLine($"Attempting to add new user: {newUser.Username}");

                db.Users.Add(newUser);

                int result = await db.SaveChangesAsync();
                Debug.WriteLine($"SaveChanges Result: {result}");

                if (result > 0)
                {
                    Debug.WriteLine("User successfully added to database");
                    TempData["SuccessMessage"] = "ההרשמה בוצעה בהצלחה! אנא התחבר למערכת";
                    return View();
                }
                else
                {
                    Debug.WriteLine("SaveChanges returned 0 - no rows affected");
                    throw new Exception("Failed to save user to database");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during registration: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ModelState.AddModelError("", "אירעה שגיאה בתהליך ההרשמה. אנא נסה שנית מאוחר יותר.");
                return View(model);
            }
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await db.Users
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == model.Username.ToLower());

                if (user == null)
                {
                    ModelState.AddModelError("", "שם משתמש או סיסמה שגויים");
                    return View(model);
                }

                if ((bool)!user.IsActive)
                {
                    ModelState.AddModelError("", "החשבון אינו פעיל. אנא פנה למנהל המערכת");
                    return View(model);
                }

                string hashedPassword = HashPassword(model.Password);
                if (user.Password != hashedPassword)
                {
                    ModelState.AddModelError("", "שם משתמש או סיסמה שגויים");
                    return View(model);
                }

                FormsAuthentication.SetAuthCookie(user.Username, model.RememberMe);
                TempData["SuccessMessage"] = "התחברת בהצלחה!";

                if ((bool)user.IsAdmin)
                {
                    return RedirectToAction("Dashboard", "Admin");
                }

                return RedirectToAction("HomePage", "Home",null);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "אירעה שגיאה בתהליך ההתחברות. אנא נסה שנית מאוחר יותר.");
                return View(model);
            }
        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            TempData["SuccessMessage"] = "התנתקת בהצלחה!";
            return RedirectToAction("HomePage", "Home");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && db != null)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [Authorize]
        public async Task<ActionResult> Profile()
        {
            var username = User.Identity.Name;
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) return RedirectToAction("Login");

            var model = new UserProfileViewModel
            {
                Username = user.Username,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate ?? DateTime.Now,
                IsAdmin = user.IsAdmin ?? false
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
    
        public async Task<ActionResult> UpdateProfile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
            if (user == null) return Json(new { success = false, redirect = Url.Action("Login") });

            var usernameExists = await db.Users
                .AnyAsync(u => u.Username == model.Username && u.UserID != user.UserID);
            var emailExists = await db.Users
                .AnyAsync(u => u.Email == model.Email && u.UserID != user.UserID);

            if (usernameExists)
                return Json(new { success = false, error = "שם המשתמש כבר קיים במערכת" });
            if (emailExists)
                return Json(new { success = false, error = "כתובת האימייל כבר קיימת במערכת" });

            user.Username = model.Username;
            user.Email = model.Email;
            await db.SaveChangesAsync();

            FormsAuthentication.SetAuthCookie(model.Username, true);
            return Json(new { success = true });
        }
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user != null)
            {
                var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                user.PasswordReset = token;
                await db.SaveChangesAsync();

                var resetLink = Url.Action("ResetPassword", "Account",
                    new { token }, Request.Url.Scheme);

                var emailService = new EmailService();
                await emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
            }

            TempData["SuccessMessage"] = "אם האימייל קיים במערכת, נשלח אליך קישור לאיפוס סיסמה";
            return RedirectToAction("Login");
        }

        public async Task<ActionResult> ResetPassword(string token)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.PasswordReset == token);
            if (user == null) return RedirectToAction("Login");

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await db.Users.FirstOrDefaultAsync(u => u.PasswordReset == model.Token);
            if (user == null) return RedirectToAction("Login");

            user.Password = HashPassword(model.Password);
            user.PasswordReset = null;
            await db.SaveChangesAsync();

            TempData["SuccessMessage"] = "הסיסמה אופסה בהצלחה";
            return RedirectToAction("HomePage", "Home");
        }
    }
}