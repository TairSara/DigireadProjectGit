using System;
using System.Collections.Generic;  
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DigireadProject.Models;
using DigireadProject.Models.ViewModels;

namespace DigireadProject.Controllers
{
    [Authorize]
    public class BookManagementController : Controller
    {
        private readonly libraryProject_digireadEntities db;
        private readonly EmailService _emailService;

        public BookManagementController()
        {
            db = new libraryProject_digireadEntities();
            _emailService = new EmailService();
            
            System.Diagnostics.Debug.WriteLine($"EmailService created: {_emailService != null}");
            System.Diagnostics.Debug.WriteLine("BookManagementController נוצר");
            System.Diagnostics.Debug.WriteLine($"EmailService status: {_emailService != null}");
        }

        public async Task<ActionResult> ManageBooks()
        {
            if (!await IsUserAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var books = await db.Books.ToListAsync();
            var viewModels = books.Select(b => MapToViewModel(b)).ToList();
            return View(viewModels);
        }

        [HttpGet]
        public async Task<ActionResult> AddBook()
        {
            if (!await IsUserAdmin())
            {
                return RedirectToAction("Login", "Account");
            }
            return View(new BookViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

public async Task<ActionResult> AddBook(BookViewModel viewModel)
{
    if (!await IsUserAdmin())
    {
        return Json(new { success = false, message = "אין הרשאת מנהל" });
    }

    if (ModelState.IsValid)
    {
        try
        {
            // בדיקה אם כבר קיים ספר עם אותו שם וסופר
            var existingBook = await db.Books
                .FirstOrDefaultAsync(b => b.Title.ToLower() == viewModel.Title.ToLower() 
                                     && b.MainAuthor.ToLower() == viewModel.MainAuthor.ToLower());
            
            if (existingBook != null)
            {
                return Json(new { success = false, message = "ספר זה כבר קיים במערכת" });
            }

            var book = new Books
            {
                Title = viewModel.Title,
                MainAuthor = viewModel.MainAuthor,
                Publisher = viewModel.Publisher,
                PublishYear = viewModel.PublishYear,
                RentalPrice = viewModel.RentalPrice,
                PurchasePrice = viewModel.PurchasePrice,
                AgeRestriction = viewModel.AgeRestriction,
                Genre = viewModel.Genre,
                IsAvailable = viewModel.IsAvailable.GetValueOrDefault(),
                IsForRent = viewModel.IsForRent.GetValueOrDefault(),
                OriginalPrice = viewModel.OriginalPrice,
                DiscountEndDate = viewModel.DiscountEndDate,
                IsEPUBAvailable = viewModel.IsEPUBAvailable.GetValueOrDefault(),
                IsF2BAvailable = viewModel.IsF2BAvailable.GetValueOrDefault(),
                IsMobiAvailable = viewModel.IsMobiAvailable.GetValueOrDefault(),
                IsPDFAvailable = viewModel.IsPDFAvailable.GetValueOrDefault(),
                CreatedDate = DateTime.Now,
                StockQuantity = viewModel.IsAvailable == true ? viewModel.StockQuantity : 0,
                ImageSrc = viewModel.ImageSrc,
                Description = viewModel.Description
            };

            db.Books.Add(book);
            await db.SaveChangesAsync();
            return Json(new { success = true, bookId = book.BookID });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "אירעה שגיאה בשמירת הספר: " + ex.Message });
        }
    }

    return Json(new { success = false, message = "נתונים לא תקינים" });
}

        [HttpGet]
        public async Task<ActionResult> EditBook(int id)
        {
            if (!await IsUserAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var book = await db.Books.FindAsync(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(MapToViewModel(book));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditBook(BookViewModel viewModel)
        {
            if (!await IsUserAdmin())
            {
                return Json(new { success = false, message = "אין הרשאת מנהל" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var book = await db.Books.FindAsync(viewModel.BookID);
                    if (book == null)
                    {
                        return Json(new { success = false, message = "הספר לא נמצא" });
                    }

                    int oldStock = book.StockQuantityRent.GetValueOrDefault(0);
                    bool wasAvailable = book.IsAvailable ?? false;

                    // עדכון כל השדות של הספר
                    book.Title = viewModel.Title;
                    book.MainAuthor = viewModel.MainAuthor;
                    book.Publisher = viewModel.Publisher;
                    book.PublishYear = viewModel.PublishYear;
                    book.RentalPrice = viewModel.RentalPrice;
                    book.PurchasePrice = viewModel.PurchasePrice;
                    book.AgeRestriction = viewModel.AgeRestriction;
                    book.Genre = viewModel.Genre;
                    book.IsAvailable = viewModel.IsAvailable ?? false;
                    book.IsForRent = viewModel.IsForRent ?? false;
                    book.OriginalPrice = viewModel.OriginalPrice;
                    book.DiscountEndDate = viewModel.DiscountEndDate;
                    book.IsEPUBAvailable = viewModel.IsEPUBAvailable ?? false;
                    book.IsF2BAvailable = viewModel.IsF2BAvailable ?? false;
                    book.IsMobiAvailable = viewModel.IsMobiAvailable ?? false;
                    book.IsPDFAvailable = viewModel.IsPDFAvailable ?? false;
                    book.StockQuantity = (viewModel.IsAvailable ?? false) ? viewModel.StockQuantity : 0;
                    book.ImageSrc = viewModel.ImageSrc;
                    book.Description = viewModel.Description;
                    book.StockQuantityRent = (viewModel.IsForRent ?? false) ? viewModel.StockQuantityRent : 0;

                    await db.SaveChangesAsync();

                    // אם הספר הפך לזמין או שכמות המלאי גדלה
                    if ((book.StockQuantityRent > oldStock) || (!wasAvailable && book.IsAvailable == true))
                    {
                        var firstWaitingUser = await db.WaitList
                            .Where(w => w.BookID == book.BookID)
                            .OrderBy(w => w.WaitPosition)
                            .Include(w => w.Users)
                            .FirstOrDefaultAsync();

                        if (firstWaitingUser != null && firstWaitingUser.Users?.Email != null)
                        {
                            await _emailService.SendBookAvailableNotificationAsync(
                                firstWaitingUser.Users.Email,
                                book.Title
                            );
                            System.Diagnostics.Debug.WriteLine($"נשלח מייל למשתמש {firstWaitingUser.Users.Email} על זמינות הספר {book.Title}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("לא נמצאו משתמשים ברשימת ההמתנה או שאין אימייל למשתמש הראשון");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"לא נשלח מייל כי המלאי לא גדל. מלאי ישן: {oldStock}, מלאי חדש: {book.StockQuantityRent}");
                    }

                    return Json(new { success = true, message = "הספר עודכן בהצלחה" });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"שגיאה בעדכון הספר: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                    return Json(new { success = false, message = "אירעה שגיאה בשמירת הספר: " + ex.Message });
                }
            }

            return Json(new { success = false, message = "נתונים לא תקינים" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteBook(int id)
        {
            if (!await IsUserAdmin())
            {
                return Json(new { success = false, message = "אין הרשאת מנהל" });
            }

            try
            {
                var book = await db.Books.FindAsync(id);
                if (book != null)
                {
                    // בדיקה אם יש השאלות פעילות
                    if (await db.Rentals.AnyAsync(r => r.BookID == id && r.ReturnDate == null))
                    {
                        return Json(new { success = false, message = "לא ניתן למחוק ספר שיש לו השאלות פעילות" });
                    }

                    db.Books.Remove(book);
                    await db.SaveChangesAsync();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "הספר לא נמצא" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "אירעה שגיאה במחיקת הספר: " + ex.Message });
            }
        }

        private BookViewModel MapToViewModel(Books book)
        {
            return new BookViewModel
            {
                BookID = book.BookID,
                Title = book.Title,
                MainAuthor = book.MainAuthor,
                Publisher = book.Publisher,
                PublishYear = book.PublishYear,
                RentalPrice = book.RentalPrice,
                PurchasePrice = book.PurchasePrice,
                AgeRestriction = book.AgeRestriction,
                Genre = book.Genre,
                IsAvailable = book.IsAvailable,
                IsForRent = book.IsForRent,
                OriginalPrice = book.OriginalPrice,
                DiscountEndDate = book.DiscountEndDate,
                IsRented = book.IsRented,
                IsEPUBAvailable = book.IsEPUBAvailable,
                IsF2BAvailable = book.IsF2BAvailable,
                IsMobiAvailable = book.IsMobiAvailable,
                IsPDFAvailable = book.IsPDFAvailable,
                StockQuantity = book.StockQuantity,
                ImageSrc = book.ImageSrc,
                Description = book.Description,
                StockQuantityRent = book.StockQuantityRent
            };
        }

        private async Task<bool> IsUserAdmin()
        {
            var username = User.Identity.Name;
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            return user != null && user.IsAdmin == true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }

        [AllowAnonymous]
        public async Task<ActionResult> Gallery()
        {
            var books = await db.Books.ToListAsync();
            var genres = books.Select(b => b.Genre).Distinct().ToList();

            // בדיקה אם המשתמש מחובר
            var userWishlist = new List<int>();
            if (User.Identity.IsAuthenticated)
            {
                var userId = GetCurrentUserId();
                userWishlist = await db.Wishlist
                    .Where(w => w.UserID == userId)
                    .Select(w => w.BookID)
                    .ToListAsync();
            }

            var viewModel = new GalleryViewModel
            {
                Genres = genres,
                Books = books,
                UserWishlist = userWishlist // נוסיף את זה למודל
            };

            return View(viewModel);
        }

        [AllowAnonymous]
        public async Task<ActionResult> GenreBooks(string genre)
        {
            var books = await db.Books
                .Where(b => b.IsAvailable == true && b.Genre == genre)
                .ToListAsync();

            var allGenres = await db.Books
                .Where(b => b.IsAvailable == true)
                .Select(b => b.Genre)
                .Distinct()
                .ToListAsync();

            // בדיקה אם המשתמש מחובר
            var userWishlist = new List<int>();
            if (User.Identity.IsAuthenticated)
            {
                var userId = GetCurrentUserId();
                userWishlist = await db.Wishlist
                    .Where(w => w.UserID == userId)
                    .Select(w => w.BookID)
                    .ToListAsync();
            }

            var viewModel = new GalleryViewModel
            {
                Genres = allGenres,
                Books = books,
                SelectedGenre = genre,
                UserWishlist = userWishlist // נוסיף את זה גם כאן
            };

            return View("Gallery", viewModel);
        }
        [AllowAnonymous]
        public async Task<ActionResult> BookDetails(int id)
        {
            var book = await db.Books.FindAsync(id);
            if (book == null)
            {
                return HttpNotFound("הספר לא נמצא.");
            }

            var viewModel = MapToViewModel(book);
    
            // הוספת הדירוגים למודל
            viewModel.Reviews = await db.Reviews
                .Where(r => r.BookID == id && r.RatingBook.HasValue)
                .OrderByDescending(r => r.ReviewDateBook)
                .Select(r => new BookReviewViewModel
                {
                    Username = r.Users.Username,
                    Rating = r.RatingBook ?? 0,
                    Comment = r.ReviewTextBook,
                    ReviewDate = r.ReviewDateBook ?? DateTime.Now
                })
                .ToListAsync();

            viewModel.AverageRating = viewModel.Reviews.Any() 
                ? (decimal)viewModel.Reviews.Average(r => r.Rating) 
                : 0m;
            viewModel.ReviewCount = viewModel.Reviews.Count;

            return View(viewModel);
        }
        [AllowAnonymous]
        public ActionResult MainGallery()
        {
            var genres = db.Books
                .Select(b => b.Genre)
                .Distinct()
                .ToList();

            return View(genres);
        }

        public async Task<ActionResult> UserRentals(int? userId)
        {
            if (!(User.Identity.IsAuthenticated && await IsUserAdmin()))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!userId.HasValue)
            {
                return RedirectToAction("ManageBooks");
            }

            var now = DateTime.Now;
            var userRentals = await db.Rentals
                .Where(r => r.UserID == userId.Value)
                .Select(r => new UserRentalViewModel
                {
                    RentalID = r.RentalID,
                    BookTitle = r.Books.Title,
                    RentalDate = r.RentalDate ?? DateTime.Now,
                    ReturnDate = r.ReturnDate,
                    BookID = r.BookID ?? 0,
                    Status = r.ReturnDate.HasValue ? "הוחזר" :
                             (r.RentalDate.Value.AddDays(31) < now ? "פג תוקף" : "פעיל"),//לשנות
                    DaysOverdue = r.ReturnDate == null && r.RentalDate.Value.AddDays(31) < now
                        ? (int)(now - r.RentalDate.Value.AddDays(31)).TotalDays
                        : 0
                })
                .ToListAsync();

            ViewBag.Username = await db.Users
                .Where(u => u.UserID == userId.Value)
                .Select(u => u.Username)
                .FirstOrDefaultAsync();

            return View(userRentals);
        }
        [HttpPost]
        public async Task<ActionResult> CancelRental(int rentalId)
        {
            if (!(User.Identity.IsAuthenticated && await IsUserAdmin()))
            {
                return Json(new { success = false, message = "אין הרשאת מנהל" });
            }

            var rental = await db.Rentals.FindAsync(rentalId);
            if (rental != null)
            {
                // אם ההשאלה טרם הוחזרה
                if (rental.ReturnDate == null)
                {
                    // החזר את הספר לזמינות
                    var book = await db.Books.FindAsync(rental.BookID);
                    if (book != null)
                    {
                        book.IsAvailable = true;
                    }

                    // מחק את ההשאלה
                    db.Rentals.Remove(rental);
                    await db.SaveChangesAsync();

                    return Json(new { success = true });
                }
            }
            return Json(new { success = false, message = "לא ניתן לבטל השאלה זו" });
        }

        // שיטה להחזרת ספר
        [HttpPost]
        public async Task<ActionResult> ReturnBook(int rentalId)
        {
            if (!(User.Identity.IsAuthenticated && await IsUserAdmin()))
            {
                return Json(new { success = false, message = "אין הרשאת מנהל" });
            }

            try 
            {
                var rental = await db.Rentals.FindAsync(rentalId);
                if (rental != null && rental.ReturnDate == null)
                {
                    rental.ReturnDate = DateTime.Now;

                    var book = await db.Books.FindAsync(rental.BookID);
                    if (book != null)
                    {
                        // עדכון המלאי
                        book.StockQuantityRent += 1;
                        await db.SaveChangesAsync(); // שומר את השינויים במלאי קודם

                        // שליחת התראה למשתמש הראשון ברשימת ההמתנה
                        if (book.StockQuantityRent > 0)
                        {
                            var firstWaitingUser = await db.WaitList
                                .Where(w => w.BookID == book.BookID)
                                .OrderBy(w => w.WaitPosition)
                                .Include(w => w.Users)
                                .FirstOrDefaultAsync();

                            if (firstWaitingUser != null && firstWaitingUser.Users?.Email != null)
                            {
                                await _emailService.SendBookAvailableNotificationAsync(
                                    firstWaitingUser.Users.Email,
                                    book.Title
                                );
                            }
                        }

                        await RemoveFromWaitListAfterRental(book.BookID, rental.UserID ?? 0);
                    }

                    await db.SaveChangesAsync();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "לא ניתן להחזיר השאלה זו" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "אירעה שגיאה בהחזרת הספר" });
            }
        }
        
        public async Task<ActionResult> MyLibrary()
        {
            var userId = GetCurrentUserId();
            var result = new List<MyLibraryViewModel>();
            var now = DateTime.Now;

            // Get purchased books
            var purchasedBookIds = await db.Purchases
                .Where(p => p.UserID == userId)
                .Select(p => p.BookID)
                .ToListAsync();

            // Get active rentals (not expired and not returned)
            var rentalsWithDates = await db.Rentals
                .Where(r => r.UserID == userId && 
                            r.ReturnDate == null && 
                            DbFunctions.AddDays(r.RentalDate, 31) >= now)//לשנות
                .Select(r => new { r.BookID, r.RentalDate })
                .ToListAsync();

            var rentedBookIds = rentalsWithDates.Select(r => r.BookID).ToList();

            // Get all relevant books
            var allRelevantBooks = await db.Books
                .Where(b => purchasedBookIds.Contains(b.BookID) || rentedBookIds.Contains(b.BookID))
                .ToListAsync();

            // Add purchased books to result
            foreach (var bookId in purchasedBookIds)
            {
                var book = allRelevantBooks.FirstOrDefault(b => b.BookID == bookId);
                if (book != null)
                {
                    result.Add(new MyLibraryViewModel
                    {
                        BookId = book.BookID,
                        Title = book.Title,
                        Author = book.MainAuthor,
                        ImageSrc = book.ImageSrc,
                        Type = "רכישה"
                    });
                }
            }
            
        // Add active rentals to result
            foreach (var rental in rentalsWithDates)
            {
                var book = allRelevantBooks.FirstOrDefault(b => b.BookID == rental.BookID);
                if (book != null)
                {
                    result.Add(new MyLibraryViewModel
                    {
                        BookId = book.BookID,
                        Title = book.Title,
                        Author = book.MainAuthor,
                        ImageSrc = book.ImageSrc,
                        Type = "השאלה",
                        ReturnDate = rental.RentalDate?.AddDays(31) //לשנות
                    });
                }
            }

            // Automatically handle expired rentals
            await HandleExpiredRentals(userId);
            await CheckAndSendExpirationAlerts();

            return View(result);
        }


        private int GetCurrentUserId()
        {
            var username = User.Identity.Name;
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            return user?.UserID ?? 0;
        }
        
        private bool IsRentalExpired(DateTime? rentalDate)
        {
            if (!rentalDate.HasValue) return false;
            return (DateTime.Now - rentalDate.Value).TotalDays >= 31; //לשנות
        }
        
        //Automatically handles expired questions
        private async Task HandleExpiredRentals(int userId)
        {
            var now = DateTime.Now;
            var expiredRentals = await db.Rentals
                .Where(r => r.UserID == userId && 
                            r.ReturnDate == null && 
                            DbFunctions.AddDays(r.RentalDate, 31) < now)//לשנות
                .ToListAsync();

            if (expiredRentals.Any())
            {
                foreach (var rental in expiredRentals)
                {
                    rental.ReturnDate = now;
            
                    var book = await db.Books.FindAsync(rental.BookID);
                    if (book != null)
                    {
                        book.IsAvailable = true;
                        book.StockQuantityRent += 1; // החזרת הספר למלאי ההשאלות
                    }
                }

                await db.SaveChangesAsync();
                TempData["ExpiredRentals"] = true;
            }
        }
        
        private async Task CheckAndSendExpirationAlerts()
        {
            var now = DateTime.Now;
            var fiveDaysFromNow = now.AddDays(5);

            // מצא את כל ההשאלות שיפוגו בעוד 5 ימים
            var expiringRentals = await db.Rentals
                .Where(r => r.ReturnDate == null // עדיין לא הוחזר
                            && r.RentalDate.HasValue
                            && DbFunctions.AddDays(r.RentalDate.Value, 25) <= now // עברו 25 ימים מההשאלה
                            && DbFunctions.AddDays(r.RentalDate.Value, 26) > now) // אבל לא עברו 26 ימים
                .Include(r => r.Books) // כולל מידע על הספר
                .Include(r => r.Users) // כולל מידע על המשתמש
                .ToListAsync();

            foreach (var rental in expiringRentals)
            {
                if (rental.Users?.Email != null)
                {
                    var daysLeft = 30 - (int)(now - rental.RentalDate.Value).TotalDays;
                    await _emailService.SendRentalExpirationAlertAsync(
                        rental.Users.Email,
                        rental.Books.Title,
                        daysLeft
                    );
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteFromLibrary(int bookId, string type)
        {
            try
            {
                int userId = GetCurrentUserId();
                System.Diagnostics.Debug.WriteLine($"מנסה למחוק ספר: BookId={bookId}, Type={type}, UserId={userId}");

                if (type == "השאלה")
                {
                    var rental = await db.Rentals
                        .FirstOrDefaultAsync(r => r.BookID == bookId && 
                                                  r.UserID == userId && 
                                                  r.ReturnDate == null);

                    System.Diagnostics.Debug.WriteLine($"נמצאה השאלה: {rental != null}");

                    if (rental != null)
                    {
                        var book = await db.Books.FindAsync(bookId);
                        if (book != null)
                        {
                            book.StockQuantityRent += 1;
                            System.Diagnostics.Debug.WriteLine($"עודכן מלאי השאלות: {book.StockQuantityRent}");
                        }
                        db.Rentals.Remove(rental);
                        await db.SaveChangesAsync();
                    }
                }
                else if (type == "רכישה")
                {
                    var purchase = await db.Purchases
                        .FirstOrDefaultAsync(p => p.BookID == bookId && 
                                                  p.UserID == userId);

                    System.Diagnostics.Debug.WriteLine($"נמצאה רכישה: {purchase != null}");

                    if (purchase != null)
                    {
                        db.Purchases.Remove(purchase);
                        await db.SaveChangesAsync();
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"שגיאה: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DownloadBook(int bookId, string format)
        {
            try
            {
                var userId = GetCurrentUserId();
                var purchase = await db.Purchases.FirstOrDefaultAsync(p => p.BookID == bookId && p.UserID == userId);
                var rental = await db.Rentals.FirstOrDefaultAsync(r => r.BookID == bookId && r.UserID == userId && r.ReturnDate == null);

                if (purchase == null && rental == null)
                {
                    return Json(new { success = false, message = "לא נמצאה רכישה או השאלה פעילה לספר זה" });
                }

                var book = await db.Books.FindAsync(bookId);
                if (book == null)
                {
                    return Json(new { success = false, message = "הספר לא נמצא" });
                }

                string fileName = $"{book.Title}.{format.ToLower()}";
                string sampleFileName = $"sample_book.{format.ToLower()}";
                string filePath = System.IO.Path.Combine(Server.MapPath("~/Content/SampleBooks"), sampleFileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return Json(new { success = false, message = $"הקובץ {sampleFileName} לא נמצא בנתיב {filePath}" });
                }

                var downloadUrl = Url.Action("DownloadFile", new { bookId, format });
                return Json(new { success = true, downloadUrl, fileName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "אירעה שגיאה בהורדת הספר: " + ex.Message });
            }
        }

        [HttpGet]
        public ActionResult DownloadFile(int bookId, string format)
        {
            try
            {
                var userId = GetCurrentUserId();
                var purchase = db.Purchases.FirstOrDefault(p => p.BookID == bookId && p.UserID == userId);
                var rental = db.Rentals.FirstOrDefault(r => r.BookID == bookId && r.UserID == userId && r.ReturnDate == null);

                if (purchase == null && rental == null)
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
                }

                var book = db.Books.Find(bookId);
                if (book == null)
                {
                    return HttpNotFound();
                }

                string fileName = $"{book.Title}.{format.ToLower()}";
                string sampleFileName = $"sample_book.{format.ToLower()}";
                string filePath = System.IO.Path.Combine(Server.MapPath("~/Content/SampleBooks"), sampleFileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return HttpNotFound();
                }

                return File(filePath, GetMimeType(format), fileName);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.InternalServerError);
            }
        }

        private string GetMimeType(string format)
        {
            switch (format.ToUpper())
            {
                case "PDF":
                    return "application/pdf";
                case "EPUB":
                    return "application/epub+zip";
                case "MOBI":
                    return "application/x-mobipocket-ebook";
                case "FB2":
                    return "application/xml";
                default:
                    return "application/octet-stream";
            }
        }
        
        [Authorize]
        public async Task<ActionResult> MyWaitList()
        {
            int userId = GetCurrentUserId();

            // שלב 1: הבאת הנתונים מהדאטהבייס
            var waitListItems = await db.WaitList
                .Where(w => w.UserID == userId)
                .Include(w => w.Books)
                .ToListAsync();

            // שלב 2: המרה ל-ViewModel
            var waitListViewModels = waitListItems.Select(w => new WaitListViewModel
                {
                    WaitListID = w.WaitListID,
                    BookID = w.BookID ?? 0,
                    UserID = w.UserID ?? 0,
                    WaitPosition = w.WaitPosition ?? 0,
                    AddedDate = w.AddedDate ?? DateTime.Now,
                    EmailNotificationSent = w.EmailNotificationSent ?? false,
                    BookTitle = w.Books?.Title ?? string.Empty,
                    ImageSrc = w.Books?.ImageSrc ?? string.Empty,
                    IsAvailable = w.Books?.IsAvailable ?? false,
                    IsRental = w.IsRental ?? false,
                    StockQuantity = w.Books?.StockQuantity ?? 0,
                    StockQuantityRent = w.Books?.StockQuantityRent ?? 0
                })
                .OrderBy(w => w.WaitPosition)
                .ToList();

            return View(waitListViewModels);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> RemoveFromWaitList(int waitListId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var waitListItem = await db.WaitList
                    .FirstOrDefaultAsync(w => w.WaitListID == waitListId && w.UserID == userId);

                if (waitListItem != null)
                {
                    int bookId = waitListItem.BookID ?? 0;

                    // מחיקת הפריט מרשימת ההמתנה
                    db.WaitList.Remove(waitListItem);
                    await db.SaveChangesAsync();

                    // מציאת כל המשתמשים שעדיין ממתינים לאותו ספר ומיון לפי המיקום הנוכחי
                    var remainingUsers = await db.WaitList
                        .Where(w => w.BookID == bookId)
                        .OrderBy(w => w.WaitPosition)
                        .ToListAsync();

                    // עדכון המיקומים החדשים - התחלה מ-1
                    for (int i = 0; i < remainingUsers.Count; i++)
                    {
                        remainingUsers[i].WaitPosition = i + 1;
                        remainingUsers[i].EmailNotificationSent = false;
                    }

                    await db.SaveChangesAsync();

                    // בדיקה אם יש ספר זמין במלאי ושליחת התראה למשתמש הראשון
                    var book = await db.Books.FindAsync(bookId);
                    if (book != null && book.StockQuantityRent > 0)
                    {
                        await CheckAndUpdateWaitList(bookId);
                    }

                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "פריט לא נמצא" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"שגיאה: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddToWaitList(int bookId, bool isRental)
        {
            try
            {
                // אם זו לא השאלה, אין אפשרות להצטרף לרשימת המתנה
                if (!isRental)
                {
                    return Json(new { success = false, 
                        message = "לא ניתן להצטרף לרשימת המתנה עבור רכישת ספר" });
                }

                int userId = GetCurrentUserId();

                // בדיקה אם המשתמש כבר ברשימת המתנה לספר זה
                var existingWaitListItem = await db.WaitList
                    .FirstOrDefaultAsync(w => w.BookID == bookId && w.UserID == userId);

                if (existingWaitListItem != null)
                {
                    return Json(new { success = false, 
                        message = "את/ה כבר נמצא/ת ברשימת ההמתנה לספר זה" });
                }

                var book = await db.Books.FindAsync(bookId);
                if (book == null)
                {
                    return Json(new { success = false, message = "הספר לא נמצא" });
                }

                // בדיקה שאכן מדובר בספר להשאלה ושאין מלאי
                if (!book.IsForRent == true || book.StockQuantityRent > 0)
                {
                    return Json(new { success = false, 
                        message = "הספר זמין להשאלה או שאינו מיועד להשאלה" });
                }

                // חישוב המיקום הבא ברשימת ההמתנה
                var nextPosition = await db.WaitList
                    .Where(w => w.BookID == bookId)
                    .Select(w => (int?)w.WaitPosition)
                    .MaxAsync() ?? 0;

                var waitListItem = new WaitList
                {
                    BookID = bookId,
                    UserID = userId,
                    WaitPosition = nextPosition + 1,
                    AddedDate = DateTime.Now,
                    EmailNotificationSent = false,
                    IsRental = true // תמיד true כי רק השאלות יכולות להיכנס לרשימת המתנה
                };

                db.WaitList.Add(waitListItem);
                await db.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, 
                    message = "אירעה שגיאה בהוספה לרשימת ההמתנה" });
            }
        }
        
        private async Task CheckAndUpdateWaitList(int bookId)
        {
            System.Diagnostics.Debug.WriteLine($"=== התחלת בדיקת רשימת המתנה עבור ספר {bookId} ===");

            try 
            {
                var book = await db.Books.FindAsync(bookId);
                System.Diagnostics.Debug.WriteLine($"האם נמצא ספר: {book != null}");
                System.Diagnostics.Debug.WriteLine($"כמות במלאי להשאלה: {book?.StockQuantityRent}");

                if (book != null && book.StockQuantityRent > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"הספר {book.Title} זמין להשאלה");

                    // בדיקה כמה אנשים בסך הכל ברשימת ההמתנה
                    var totalWaiting = await db.WaitList
                        .CountAsync(w => w.BookID == bookId);
                    System.Diagnostics.Debug.WriteLine($"מספר אנשים ברשימת ההמתנה: {totalWaiting}");

                    var firstWaitingUser = await db.WaitList
                        .Where(w => w.BookID == bookId)
                        .OrderBy(w => w.WaitPosition)
                        .Include(w => w.Users)
                        .FirstOrDefaultAsync();

                    System.Diagnostics.Debug.WriteLine($"נמצא משתמש ממתין: {firstWaitingUser != null}");
                    System.Diagnostics.Debug.WriteLine($"מיקום בתור: {firstWaitingUser?.WaitPosition}");
                    System.Diagnostics.Debug.WriteLine($"האם יש אימייל: {firstWaitingUser?.Users?.Email != null}");

                    if (firstWaitingUser != null && firstWaitingUser.Users?.Email != null)
                    {
                        try 
                        {
                            System.Diagnostics.Debug.WriteLine($"מנסה לשלוח מייל ל: {firstWaitingUser.Users.Email}");
                            await _emailService.SendBookAvailableNotificationAsync(
                                firstWaitingUser.Users.Email,
                                book.Title
                            );
                            System.Diagnostics.Debug.WriteLine("המייל נשלח בהצלחה");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"שגיאה בשליחת המייל: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                            throw; // זורק את השגיאה הלאה כדי שנוכל לראות אותה
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"שגיאה כללית: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
            System.Diagnostics.Debug.WriteLine("=== סיום בדיקת רשימת המתנה ===");
        }
        [HttpPost]
public async Task<ActionResult> ManageRentalAction(int rentalId, string action)
{
    if (!await IsUserAdmin())
    {
        return Json(new { success = false, message = "אין הרשאת מנהל" });
    }

    try 
    {
        var rental = await db.Rentals.FindAsync(rentalId);
        if (rental == null)
        {
            return Json(new { success = false, message = "ההשאלה לא נמצאה" });
        }

        switch (action.ToLower())
        {
            case "return":
                rental.ReturnDate = DateTime.Now;
                var book = await db.Books.FindAsync(rental.BookID);
                if (book != null)
                {
                    book.StockQuantityRent += 1;
                    await CheckAndUpdateWaitList(book.BookID);
                }
                break;

            case "extend":
                // הוספת זמן להשאלה, למשל עוד 14 יום
                rental.RentalDate = rental.RentalDate.Value.AddDays(14);
                break;

            case "cancel":
                db.Rentals.Remove(rental);
                break;

            default:
                return Json(new { success = false, message = "פעולה לא חוקית" });
        }

        await db.SaveChangesAsync();
        return Json(new { success = true, message = $"הפעולה {action} בוצעה בהצלחה" });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = $"אירעה שגיאה: {ex.Message}" });
    }
}

[HttpPost]
public async Task<ActionResult> ManageWaitListAction(int waitListId, string action)
{
    if (!await IsUserAdmin())
    {
        return Json(new { success = false, message = "אין הרשאת מנהל" });
    }

    try 
    {
        var waitListItem = await db.WaitList.FindAsync(waitListId);
        if (waitListItem == null)
        {
            return Json(new { success = false, message = "הפריט ברשימת ההמתנה לא נמצא" });
        }

        switch (action.ToLower())
        {
            case "remove":
                // הסרת הפריט מרשימת ההמתנה
                db.WaitList.Remove(waitListItem);
                await db.SaveChangesAsync();

                // עדכון מיקומים של שאר הפריטים
                var remainingUsers = await db.WaitList
                    .Where(w => w.BookID == waitListItem.BookID)
                    .OrderBy(w => w.WaitPosition)
                    .ToListAsync();

                for (int i = 0; i < remainingUsers.Count; i++)
                {
                    remainingUsers[i].WaitPosition = i + 1;
                }
                break;

            case "sendnotification":
                // שליחת התראה למשתמש
                if (waitListItem.Users?.Email != null)
                {
                    var book = await db.Books.FindAsync(waitListItem.BookID);
                    await _emailService.SendBookAvailableNotificationAsync(
                        waitListItem.Users.Email,
                        book?.Title ?? "ספר"
                    );
                    waitListItem.EmailNotificationSent = true;
                }
                break;

            default:
                return Json(new { success = false, message = "פעולה לא חוקית" });
        }

        await db.SaveChangesAsync();
        return Json(new { success = true, message = $"הפעולה {action} בוצעה בהצלחה" });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = $"אירעה שגיאה: {ex.Message}" });
    }
}
        public async Task RemoveFromWaitListAfterRental(int bookId, int userId)
        {
            try
            {
                // מוצא את הפריט ברשימת ההמתנה של המשתמש
                var waitListItem = await db.WaitList
                    .FirstOrDefaultAsync(w => w.BookID == bookId && w.UserID == userId);

                if (waitListItem != null)
                {
                    // מחיקת הפריט מרשימת ההמתנה
                    db.WaitList.Remove(waitListItem);
                    await db.SaveChangesAsync();

                    // מציאת כל המשתמשים שעדיין ממתינים לאותו ספר ומיון לפי המיקום הנוכחי
                    var remainingUsers = await db.WaitList
                        .Where(w => w.BookID == bookId)
                        .OrderBy(w => w.WaitPosition)
                        .ToListAsync();

                    // עדכון המיקומים החדשים - התחלה מ-1
                    for (int i = 0; i < remainingUsers.Count; i++)
                    {
                        remainingUsers[i].WaitPosition = i + 1;
                        // מאפס את סטטוס ההתראה כי המיקום השתנה
                        remainingUsers[i].EmailNotificationSent = false;
                    }

                    await db.SaveChangesAsync();

                    // אם יש ספר זמין במלאי, שולח התראה למשתמש הראשון החדש
                    var book = await db.Books.FindAsync(bookId);
                    if (book != null && book.StockQuantityRent > 0)
                    {
                        await CheckAndUpdateWaitList(bookId);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"שגיאה בהסרה מרשימת המתנה לאחר השאלה: {ex.Message}");
            }
        }
                
    }

}