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
            // הוספת logging של Entity Framework
            db.Database.Log = message => Debug.WriteLine(message);
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
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

                // בדיקת שם משתמש קיים
                var existingUsername = await db.Users
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == model.Username.ToLower());

                if (existingUsername != null)
                {
                    ModelState.AddModelError("Username", "שם המשתמש כבר קיים במערכת");
                    return View(model);
                }

                // בדיקת אימייל קיים
                var existingEmail = await db.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "כתובת האימייל כבר קיימת במערכת");
                    return View(model);
                }

                // יצירת משתמש חדש
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

                // הוספת המשתמש למסד הנתונים
                db.Users.Add(newUser);

                // שמירת השינויים
                int result = await db.SaveChangesAsync();
                Debug.WriteLine($"SaveChanges Result: {result}");

                if (result > 0)
                {
                    Debug.WriteLine("User successfully added to database");
                    TempData["SuccessMessage"] = "ההרשמה בוצעה בהצלחה! אנא התחבר למערכת";
                    return RedirectToAction("HomePage", "Home");
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

        // GET: Account/Login
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            Debug.WriteLine("Starting Login Process");
            Debug.WriteLine($"Login attempt for username: {model.Username}");

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

                // חיפוש המשתמש לפי שם משתמש
                var user = await db.Users
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == model.Username.ToLower());

                if (user == null)
                {
                    Debug.WriteLine("User not found");
                    ModelState.AddModelError("", "שם משתמש או סיסמה שגויים");
                    return View(model);
                }

                // בדיקה האם המשתמש פעיל
                if ((bool)!user.IsActive)
                {
                    Debug.WriteLine("User account is not active");
                    ModelState.AddModelError("", "החשבון אינו פעיל. אנא פנה למנהל המערכת");
                    return View(model);
                }

                // אימות הסיסמה
                string hashedPassword = HashPassword(model.Password);
                if (user.Password != hashedPassword)
                {
                    Debug.WriteLine("Invalid password");
                    ModelState.AddModelError("", "שם משתמש או סיסמה שגויים");
                    return View(model);
                }

                // יצירת עוגייה מאובטחת
                FormsAuthentication.SetAuthCookie(user.Username, model.RememberMe);

                Debug.WriteLine("Login successful");
                TempData["SuccessMessage"] = "התחברת בהצלחה!";

                return RedirectToAction("HomePage", "Home");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during login: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ModelState.AddModelError("", "אירעה שגיאה בתהליך ההתחברות. אנא נסה שנית מאוחר יותר.");
                return View(model);
            }
        }

        // Logout Action
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
    }
}