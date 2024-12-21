using System;
using System.Collections.Generic;  // הוספנו את זה עבור List<>
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
                        Description=viewModel.Description
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
                    return Json(new { success = true, message = "הספר עודכן בהצלחה" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "אירעה שגיאה בשמירת הספר: " + ex.Message });
                }
            }

            return Json(new { success = false, message = "נתונים לא תקינים" });
        }

        [HttpPost]
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

            var booksInGenre = await db.Books
                .Where(b => b.Genre == "ז'אנר ספציפי")  
                .ToListAsync();

            var viewModel = new GalleryViewModel
            {
                Genres = genres,
                Books = books
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

            var viewModel = new GalleryViewModel
            {
                Genres = allGenres,
                Books = books,
                SelectedGenre = genre
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
                             (r.RentalDate.Value.AddDays(14) < now ? "פג תוקף" : "פעיל"),
                    DaysOverdue = r.ReturnDate == null && r.RentalDate.Value.AddDays(14) < now
                        ? (int)(now - r.RentalDate.Value.AddDays(14)).TotalDays
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

            var rental = await db.Rentals.FindAsync(rentalId);
            if (rental != null && rental.ReturnDate == null)
            {
                rental.ReturnDate = DateTime.Now;

                var book = await db.Books.FindAsync(rental.BookID);
                if (book != null)
                {
                    book.IsAvailable = true;
                    book.StockQuantityRent += 1; // החזרת הספר למלאי ההשאלות
                }

                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "לא ניתן להחזיר השאלה זו" });
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
                            DbFunctions.AddDays(r.RentalDate, 30) >= now)
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
                        ReturnDate = rental.RentalDate?.AddDays(7) //פה לשנות ל30 אחרי הבדי
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
            return (DateTime.Now - rentalDate.Value).TotalDays >= 30;
        }
        
        //Automatically handles expired questions
        private async Task HandleExpiredRentals(int userId)
        {
            var now = DateTime.Now;
            var expiredRentals = await db.Rentals
                .Where(r => r.UserID == userId && 
                            r.ReturnDate == null && 
                            DbFunctions.AddDays(r.RentalDate, 30) < now)
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
                System.Diagnostics.Debug.WriteLine($"Starting download for user {userId}, book {bookId}, format {format}");
                
                // בדוק אם המשתמש רכש את הספר
                var purchase = await db.Purchases
                    .FirstOrDefaultAsync(p => p.BookID == bookId && p.UserID == userId);
                    
                System.Diagnostics.Debug.WriteLine($"Purchase found: {purchase != null}");
                
                if (purchase == null)
                {
                    return Json(new { success = false, message = "לא נמצאה רכישה לספר זה" });
                }

                var book = await db.Books.FindAsync(bookId);
                System.Diagnostics.Debug.WriteLine($"Book found: {book != null}");
                
                if (book == null)
                {
                    return Json(new { success = false, message = "הספר לא נמצא" });
                }

                string fileName = $"{book.Title}.{format.ToLower()}";
                string sampleFileName = $"sample_book.{format.ToLower()}";
                string filePath = System.IO.Path.Combine(Server.MapPath("~/Content/SampleBooks"), sampleFileName);
                
                System.Diagnostics.Debug.WriteLine($"Checking file path: {filePath}");
                System.Diagnostics.Debug.WriteLine($"File exists: {System.IO.File.Exists(filePath)}");
                
                if (!System.IO.File.Exists(filePath))
                {
                    return Json(new { success = false, message = $"הקובץ {sampleFileName} לא נמצא בנתיב {filePath}" });
                }

                var downloadUrl = Url.Action("DownloadFile", new { bookId, format });
                return Json(new { success = true, downloadUrl, fileName });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DownloadBook: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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
                    
                if (purchase == null)
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
                System.Diagnostics.Debug.WriteLine($"Error in DownloadFile: {ex.Message}");
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

            var waitList = await db.WaitList
                .Where(w => w.UserID == userId)
                .Join(
                    db.Books,
                    wait => wait.BookID,
                    book => book.BookID,
                    (wait, book) => new WaitListViewModel
                    {
                        WaitListID = (int)wait.WaitListID,  // הוספת המרה מפורשת
                        BookID = (int)wait.BookID,          // הוספת המרה מפורשת
                        UserID = (int)wait.UserID,          // הוספת המרה מפורשת
                        WaitPosition = (int)wait.WaitPosition, // הוספת המרה מפורשת
                        AddedDate = wait.AddedDate ?? DateTime.Now,
                        EmailNotificationSent = wait.EmailNotificationSent ?? false,
                        BookTitle = book.Title,
                        ImageSrc = book.ImageSrc,
                        IsAvailable = book.IsAvailable ?? false  // הוספת ערך ברירת מחדל
                    })
                .OrderBy(w => w.WaitPosition)
                .ToListAsync();

            return View(waitList);
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
                    db.WaitList.Remove(waitListItem);
                    
                    // עדכון מיקומים של משתמשים אחרים ברשימה
                    var otherWaitingUsers = await db.WaitList
                        .Where(w => w.BookID == waitListItem.BookID && w.WaitPosition > waitListItem.WaitPosition)
                        .ToListAsync();

                    foreach (var user in otherWaitingUsers)
                    {
                        user.WaitPosition--;
                    }

                    await db.SaveChangesAsync();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "פריט לא נמצא" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddToWaitList(int bookId, bool isRental)
        {
            try
            {
                int userId = GetCurrentUserId();
        
                // בדיקה אם המשתמש כבר ברשימת המתנה לספר זה
                var existingWaitListItem = await db.WaitList
                    .FirstOrDefaultAsync(w => w.BookID == bookId && 
                                              w.UserID == userId);

                if (existingWaitListItem != null)
                {
                    return Json(new { success = false, 
                        message = "את/ה כבר נמצא/ת ברשימת ההמתנה לספר זה" });
                }

                // מציאת המיקום הבא ברשימת ההמתנה
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
                    IsRental = isRental
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
                
    }

}