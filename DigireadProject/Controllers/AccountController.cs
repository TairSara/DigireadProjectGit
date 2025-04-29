
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Text;
using System.Security.Cryptography;
using System.Web.Security;
using System.Diagnostics;
using DigireadProject.Models.Services;
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

            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                if (!IsValidCreditCardDate(model.ValidDate))
                {
                    ModelState.AddModelError("ValidDate", "תוקף כרטיס אשראי אינו תקין או פג תוקף.");
                    return View(model);
                }

                var existingUsername = await db.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == model.Username.ToLower());
                if (existingUsername != null)
                {
                    ModelState.AddModelError("Username", "שם המשתמש כבר קיים במערכת.");
                    return View(model);
                }

                var existingEmail = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "כתובת האימייל כבר קיימת במערכת.");
                    return View(model);
                }

                var newUser = new Users
                {
                    Username = model.Username.Trim(),
                    Email = model.Email.Trim(),
                    Password = HashPassword(model.Password), // הצפנה מיוחדת על פי דרישות העבודה 
                    RegistrationDate = DateTime.Now,
                    IsActive = true,
                    IsAdmin = false,
                    PasswordReset = null,
                    FirstName = model.FirstName?.Trim(),
                    LastName = model.LastName?.Trim(),
                    IDNumber = model.IDNumber?.Trim(),
                    CreditCardNumber = model.CreditCardNumber?.Trim(),
                    ValidDate = model.ValidDate?.Trim(),
                    CVC = model.CVC?.Trim()
                };

                db.Users.Add(newUser);
                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = "ההרשמה בוצעה בהצלחה! אנא התחבר למערכת.";
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error during registration: " + ex.Message);
                ModelState.AddModelError("", "אירעה שגיאה בתהליך ההרשמה. אנא נסה שוב מאוחר יותר.");
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
                    return View(model);

                var user = await db.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == model.Username.ToLower());

                if (user == null || user.Password != HashPassword(model.Password))
                {
                    ModelState.AddModelError("", "שם משתמש או סיסמה שגויים.");
                    return View(model);
                }

                if ((bool)!user.IsActive)
                {
                    ModelState.AddModelError("", "החשבון אינו פעיל. אנא פנה למנהל המערכת.");
                    return View(model);
                }

                Session["UserID"] = user.UserID;
                Session["IsAdmin"] = user.IsAdmin;
                FormsAuthentication.SetAuthCookie(user.Username, model.RememberMe);

                TempData["SuccessMessage"] = "ברוך הבא " + user.Username + "!";

                if (user.IsAdmin == true)
                    return RedirectToAction("Dashboard", "Admin");

                return RedirectToAction("HomePage", "Home");
            }
            catch
            {
                ModelState.AddModelError("", "אירעה שגיאה בתהליך ההתחברות.");
                return View(model);
            }
        }

//  This method is intentionally vulnerable to demonstrate SQL Injection for educational purposes only!
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginForInjection(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("Login");

                //אין פה הגנה אני מדגימה פריצה למערכת 
                string query = $"SELECT * FROM Users WHERE Username = '{model.Username}' AND Password = '{model.Password}'";
                var user = await db.Users.SqlQuery(query).FirstOrDefaultAsync();

                if (user == null)
                {
                    ModelState.AddModelError("", "שם משתמש או סיסמה שגויים.");
                    return View("Login");
                }

                Session["UserID"] = user.UserID;
                Session["IsAdmin"] = user.IsAdmin;
                FormsAuthentication.SetAuthCookie(user.Username, model.RememberMe);

                TempData["SuccessMessage"] = "ברוך הבא " + user.Username + "!";

                if (user.IsAdmin == true)
                    return RedirectToAction("Dashboard", "Admin");

                return RedirectToAction("HomePage", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "אירעה שגיאה בתהליך ההתחברות.");
                Debug.WriteLine("SQL Injection Demo Error: " + ex.Message);
                return View("Login");
            }
        }


        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            TempData["SuccessMessage"] = "התנתקת בהצלחה!";
            return RedirectToAction("HomePage", "Home");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private bool IsValidCreditCardDate(string validDate)
        {
            if (string.IsNullOrWhiteSpace(validDate))
                return false;

            try
            {
                var parts = validDate.Split('/');
                if (parts.Length != 2)
                    return false;

                int month = int.Parse(parts[0]);
                int year = int.Parse(parts[1]);

                if (year < 100)
                    year += 2000;

                var expiration = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                return expiration >= DateTime.Now.Date;
            }
            catch
            {
                return false;
            }
        }
        [Authorize]
        public async Task<ActionResult> Profile()
        {
            var username = User.Identity.Name;
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return RedirectToAction("Login");

            var model = new UserProfileViewModel
            {
                Username = user.Username,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate ?? DateTime.Now,
                IsAdmin = user.IsAdmin ?? false
            };

            return View(model);
        }
        //  This method is intentionally vulnerable to demonstrate SQL Injection for educational purposes only!

        [HttpGet]
        public async Task<ActionResult> ProfileInjection(string username)
        {
            if (string.IsNullOrEmpty(username))
                return Content("חובה להזין שם משתמש");
            //אין פה הגנה אני מדגימה פריצה למערכת 
            string query = $"SELECT * FROM Users WHERE Username = '{username}'";

            var user = await db.Users.SqlQuery(query).FirstOrDefaultAsync();

            if (user == null)
                return Content("המשתמש לא נמצא");

            var model = new UserProfileViewModel
            {
                Username = user.Username,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate ?? DateTime.Now,
                IsAdmin = user.IsAdmin ?? false
            };

            return View("Profile", model);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}