using System;
using System.Collections.Generic;  
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DigireadProject.Models.ViewModels;
using DigireadProject.Models.Services;

namespace DigireadProject.Controllers
{
    [Authorize]
    public class BookManagementController : Controller
    {
        private readonly libraryProject_digireadEntities db;
        private readonly EmailService _emailService;
        private readonly WaitListService _waitListService;

        public BookManagementController()
        {
            db = new libraryProject_digireadEntities();
            _emailService = new EmailService();
            _waitListService = new WaitListService(db, _emailService); 
            
            System.Diagnostics.Debug.WriteLine($"EmailService created: {_emailService != null}");
            System.Diagnostics.Debug.WriteLine("BookManagementController נוצר");
            System.Diagnostics.Debug.WriteLine($"EmailService status: {_emailService != null}");
        }

        private async Task CleanExpiredReservations()
        {
            try 
            {
                var expiredItems = await db.WaitList
                    .Where(w => w.IsReserved && 
                                w.ReservationExpiryTime.HasValue && 
                                w.ReservationExpiryTime.Value <= DateTime.Now)
                    .ToListAsync();

                foreach (var item in expiredItems)
                {
                    item.IsReserved = false;
                    item.ReservationExpiryTime = null;
                }

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"שגיאה בניקוי הזמנות שפג תוקפן: {ex.Message}");
            }
        }

        // GET: BookManagement/ManageBooks
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
            // בדיקת מחירים
            if (viewModel.PurchasePrice >= viewModel.OriginalPrice)
            {
                return Json(new { success = false, message = "מחיר ההנחה חייב להיות נמוך מהמחיר המקורי" });
            }

            // בדיקה אם הספר כבר קיים
            bool bookExists = await db.Books.AnyAsync(b => 
                b.Title.ToLower().Trim() == viewModel.Title.ToLower().Trim());

            if (bookExists)
            {
                return Json(new { success = false, message = "ספר זה כבר קיים במערכת" });
            }

            var book = new Books
            {
                Title = viewModel.Title.Trim(),
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

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<JsonResult> ValidateBook(string title)
{
    try
    {
        var bookExists = await db.Books
            .AnyAsync(b => b.Title.ToLower().Trim() == title.ToLower().Trim());

        return Json(new { isValid = !bookExists });
    }
    catch (Exception)
    {
        return Json(new { isValid = false });
    }
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

            // בדיקת מחירים
            if (viewModel.PurchasePrice >= viewModel.OriginalPrice)
            {
                return Json(new { success = false, message = "מחיר ההנחה חייב להיות נמוך מהמחיר המקורי" });
            }

            int oldStock = book.StockQuantityRent.GetValueOrDefault(0);
            bool wasAvailable = book.IsAvailable ?? false;
            
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

            // המשך הטיפול ברשימת המתנה ושליחת מיילים...
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
                }
            }

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
        if (book == null)
        {
            return Json(new { success = false, message = "הספר לא נמצא" });
        }

        var activeRentals = await db.Rentals
            .AnyAsync(r => r.BookID == id && r.ReturnDate == null);

        if (activeRentals)
        {
            book.IsAvailable = false;
            book.IsForRent = false;
            await db.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = "לספר יש השאלות פעילות. הספר סומן כלא זמין",
                statusChanged = true 
            });
        }

        var hasHistory = await db.Purchases.AnyAsync(p => p.BookID == id) ||
                         await db.Rentals.AnyAsync(r => r.BookID == id);

        if (hasHistory)
        {
            book.IsAvailable = false;
            book.IsForRent = false;
            await db.SaveChangesAsync();
            return Json(new { 
                success = true, 
                message = "הספר סומן כלא זמין מכיוון שיש לו היסטוריית רכישות או השאלות",
                statusChanged = true
            });
        }
        else
        {
            db.Books.Remove(book);
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "הספר נמחק בהצלחה" });
        }
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
            if (disposing)
            {
                if (db != null)
                {
                    db.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        [AllowAnonymous]
        public async Task<ActionResult> Gallery()
        {
            var books = await db.Books
                .Select(b => new
                {
                    Book = b,
                    AverageRating = b.Reviews
                        .Where(r => r.RatingBook.HasValue)
                        .Select(r => (decimal)r.RatingBook.Value)
                        .DefaultIfEmpty(0)
                        .Average(),
                    ReviewCount = b.Reviews.Count(r => r.RatingBook.HasValue)
                })
                .OrderByDescending(b => b.AverageRating)
                .ThenByDescending(b => b.ReviewCount)
                .Select(b => b.Book)
                .ToListAsync();

            var genres = books.Select(b => b.Genre).Distinct().ToList();
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
                UserWishlist = userWishlist
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
                UserWishlist = userWishlist 
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
                             (r.RentalDate.Value.AddDays(31) < now ? "פג תוקף" : "פעיל"),
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
                if (rental.ReturnDate == null)
                {
                    var book = await db.Books.FindAsync(rental.BookID);
                    if (book != null)
                    {
                        book.IsAvailable = true;
                    }

                    db.Rentals.Remove(rental);
                    await db.SaveChangesAsync();

                    return Json(new { success = true });
                }
            }
            return Json(new { success = false, message = "לא ניתן לבטל השאלה זו" });
        }

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
                        book.StockQuantityRent += 1;
                        await db.SaveChangesAsync(); 

                        if (book.StockQuantityRent > 0)
                        {
                            var firstWaitingUser = await db.WaitList
                                .Where(w => w.BookID == book.BookID)
                                .OrderBy(w => w.WaitPosition)
                                .Include(w => w.Users)
                                .FirstOrDefaultAsync();

                            if (firstWaitingUser != null && firstWaitingUser.Users?.Email != null)
                            {
                                await _waitListService.ProcessWaitListItem(book.BookID);
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
                        ReturnDate = rental.RentalDate?.AddDays(31) 
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
            return (DateTime.Now - rentalDate.Value).TotalDays >= 31; 
        }
        
        //Automatically handles expired questions
        private async Task HandleExpiredRentals(int userId)
        {
            var now = DateTime.Now;
            var expiredRentals = await db.Rentals
                .Where(r => r.UserID == userId && 
                            r.ReturnDate == null && 
                            DbFunctions.AddDays(r.RentalDate, 31) < now)
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
                        book.StockQuantityRent += 1; 
                        await _waitListService.ProcessWaitListItem(book.BookID);
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

            var expiringRentals = await db.Rentals
                .Where(r => r.ReturnDate == null 
                            && r.RentalDate.HasValue
                            && DbFunctions.AddDays(r.RentalDate.Value, 25) <= now 
                            && DbFunctions.AddDays(r.RentalDate.Value, 26) > now) 
                .Include(r => r.Books) 
                .Include(r => r.Users) 
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

            if (rental == null)
            {
                return Json(new { success = false, message = "ההשאלה לא נמצאה במערכת" });
            }

            var book = await db.Books
                .FirstOrDefaultAsync(b => b.BookID == bookId);

            if (book == null)
            {
                return Json(new { success = false, message = "הספר לא נמצא במערכת" });
            }

            int currentStock = book.StockQuantityRent ?? 0;
            book.StockQuantityRent = currentStock + 1;
            
            System.Diagnostics.Debug.WriteLine($"עדכון מלאי: מ-{currentStock} ל-{book.StockQuantityRent}");

            db.Rentals.Remove(rental);
            
            await db.SaveChangesAsync();

            if (book.StockQuantityRent > 0)
            {
                var firstWaitingUser = await db.WaitList
                    .Where(w => w.BookID == bookId)
                    .OrderBy(w => w.WaitPosition)
                    .Include(w => w.Users)
                    .FirstOrDefaultAsync();

                if (firstWaitingUser?.Users?.Email != null)
                {
                    try
                    {
                        await _waitListService.ProcessWaitListItem(bookId);
                        System.Diagnostics.Debug.WriteLine($"נשלח מייל למשתמש ברשימת המתנה");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"שגיאה בשליחת מייל: {ex.Message}");
                    }
                }
            }
        }
        else if (type == "רכישה")
        {
            var purchase = await db.Purchases
                .FirstOrDefaultAsync(p => p.BookID == bookId && p.UserID == userId);

            if (purchase == null)
            {
                return Json(new { success = false, message = "הרכישה לא נמצאה במערכת" });
            }

            db.Purchases.Remove(purchase);
            await db.SaveChangesAsync();
        }

        return Json(new { success = true, message = "הפריט הוסר בהצלחה מהספרייה שלך" });
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"שגיאה כללית: {ex.Message}");
        System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
        return Json(new { success = false, message = $"אירעה שגיאה בהסרת הפריט: {ex.Message}" });
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

                if (format.ToUpper() == "FB2")
                {
                    format = "PDF";
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
                var hasAccess = db.Purchases.Any(p => p.BookID == bookId && p.UserID == userId) ||
                                db.Rentals.Any(r => r.BookID == bookId && r.UserID == userId && r.ReturnDate == null);
        
                if (!hasAccess)
                {
                    return HttpNotFound();
                }

                if (format.ToUpper() == "FB2")
                {
                    format = "PDF";
                }

                string sampleFileName = $"sample_book.{format.ToLower()}";
                string filePath = Server.MapPath($"~/Content/SampleBooks/{sampleFileName}");
        
                if (!System.IO.File.Exists(filePath))
                {
                    System.Diagnostics.Debug.WriteLine($"קובץ לא נמצא בנתיב: {filePath}");
                    return HttpNotFound();
                }

                var book = db.Books.Find(bookId);
                string fileName = $"{book.Title}.{format.ToLower()}";

                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"שגיאה בהורדת קובץ: {ex.Message}");
                return HttpNotFound();
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
                    return "application/x-fictionbook+xml";
                default:
                    return "application/octet-stream";
            }
        }
        
       [Authorize]
public async Task<ActionResult> MyWaitList()
{
    int userId = GetCurrentUserId();
    await CleanExpiredReservations(); 

    var waitListItems = await db.WaitList
        .Where(w => w.UserID == userId)
        .Include(w => w.Books)
        .OrderBy(w => w.WaitPosition)
        .ToListAsync();

    foreach (var item in waitListItems)
    {
        if (item.Books?.StockQuantityRent > 0 && item.WaitPosition == 1)
        {
            var hasActiveReservation = await db.WaitList
                .AnyAsync(w => w.BookID == item.BookID && 
                              w.IsReserved && 
                              w.ReservationExpiryTime > DateTime.Now);

            if (!hasActiveReservation)
            {
                item.IsReserved = true;
                item.ReservationExpiryTime = DateTime.Now.AddHours(4);
                item.EmailNotificationSent = true;
                
                if (item.Users?.Email != null)
                {
                    await _emailService.SendBookAvailableNotificationAsync(
                        item.Users.Email,
                        item.Books.Title
                    );
                }
            }
        }
    }

    await db.SaveChangesAsync();

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
        StockQuantityRent = w.Books?.StockQuantityRent ?? 0,
        IsReserved = w.IsReserved,
        ReservationExpiryTime = w.ReservationExpiryTime
    }).ToList();

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

                    db.WaitList.Remove(waitListItem);
                    await db.SaveChangesAsync();

                    var remainingUsers = await db.WaitList
                        .Where(w => w.BookID == bookId)
                        .OrderBy(w => w.WaitPosition)
                        .ToListAsync();

                    for (int i = 0; i < remainingUsers.Count; i++)
                    {
                        remainingUsers[i].WaitPosition = i + 1;
                        remainingUsers[i].EmailNotificationSent = false;
                    }

                    await db.SaveChangesAsync();

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
    await CleanExpiredReservations();             
    try            
    {                
        if (!isRental)                
        {                    
            return Json(new { success = false, message = "לא ניתן להצטרף לרשימת המתנה עבור רכישת ספר" });                
        }                 

        int userId = GetCurrentUserId();                 

        var existingWaitListItem = await db.WaitList                    
            .FirstOrDefaultAsync(w => w.BookID == bookId && w.UserID == userId);                 

        if (existingWaitListItem != null)                
        {                    
            return Json(new { success = false, message = "הנך כבר נמצא ברשימת ההמתנה לספר זה" });                
        }                 

        var book = await db.Books.FindAsync(bookId);                
        if (book == null)                
        {                    
            return Json(new { success = false, message = "הספר לא נמצא" });                
        }                 

        if (!book.IsForRent == true)                
        {                    
            return Json(new { success = false, message = "הספר אינו מיועד להשאלה" });                
        }                 

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
            IsRental = true,                    
            IsReserved = false,                    
            ReservationExpiryTime = null                
        };                 

        db.WaitList.Add(waitListItem);                
        await db.SaveChangesAsync();                 

        return Json(new {                     
            success = true,                    
            message = "נוספת בהצלחה לרשימת ההמתנה",                    
            position = waitListItem.WaitPosition                
        });            
    }            
    catch (Exception ex)            
    {                
        return Json(new { success = false, message = $"אירעה שגיאה בהוספה לרשימת ההמתנה: {ex.Message}" });            
    }        
}
 private async Task CheckAndUpdateWaitList(int bookId)
 
        {
            await CleanExpiredReservations();

            var book = await db.Books.FindAsync(bookId);
            
            if (book != null && book.StockQuantityRent > 0)
            {
                var hasActiveReservation = await db.WaitList
                    .AnyAsync(w => w.BookID == bookId && 
                                 w.IsReserved && 
                                 w.ReservationExpiryTime > DateTime.Now);

                if (!hasActiveReservation)
                {
                    var firstWaitingUser = await db.WaitList
                        .Where(w => w.BookID == bookId)
                        .OrderBy(w => w.WaitPosition)
                        .Include(w => w.Users)
                        .FirstOrDefaultAsync();

                    if (firstWaitingUser != null)
                    {
                        firstWaitingUser.IsReserved = true;
                        firstWaitingUser.ReservationExpiryTime = DateTime.Now.AddHours(4);

                        var otherWaitListItems = await db.WaitList
                            .Where(w => w.BookID == bookId && w.WaitListID != firstWaitingUser.WaitListID)
                            .ToListAsync();

                        foreach (var item in otherWaitListItems)
                        {
                            item.IsReserved = false;
                            item.ReservationExpiryTime = null;
                        }

                        await db.SaveChangesAsync();

                        if (firstWaitingUser.Users?.Email != null)
                        {
                            await _emailService.SendBookAvailableNotificationAsync(
                                firstWaitingUser.Users.Email,
                                book.Title
                            );
                            firstWaitingUser.EmailNotificationSent = true;
                            await db.SaveChangesAsync();
                        }
                    }
                }
            }
        }
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<JsonResult> CheckRentalEligibility(int bookId)
{
    await CleanExpiredReservations();

    try 
    {
        int userId = GetCurrentUserId();
        var book = await db.Books.FindAsync(bookId);

        if (book == null)
        {
            return Json(new { 
                success = false, 
                actionType = "error",
                message = "הספר לא נמצא"
            });
        }

        var currentRentalsCount = await db.Rentals
            .CountAsync(r => r.UserID == userId && r.ReturnDate == null);

        if (currentRentalsCount >= 3)
        {
            return Json(new { 
                success = false, 
                actionType = "error",
                message = "לא ניתן להשאיל יותר משלושה ספרים בו-זמנית" 
            });
        }

        if (book.IsForRent == true && book.StockQuantityRent > 0)
        {
            var firstWaitingUser = await db.WaitList
                .Where(w => w.BookID == bookId && !w.IsReserved)  
                .OrderBy(w => w.WaitPosition)
                .FirstOrDefaultAsync();

            var userActiveReservation = await db.WaitList
                .FirstOrDefaultAsync(w => w.BookID == bookId && 
                                        w.UserID == userId && 
                                        w.IsReserved && 
                                        w.ReservationExpiryTime > DateTime.Now);

            if (userActiveReservation != null)
            {
                return Json(new { 
                    success = true, 
                    actionType = "rent",
                    message = "הספר שמור עבורך להשכרה"
                });
            }

            if (firstWaitingUser != null)
            {
                if (firstWaitingUser.UserID == userId)
                {
                    firstWaitingUser.IsReserved = true;
                    firstWaitingUser.ReservationExpiryTime = DateTime.Now.AddHours(4);
                    
                    book.StockQuantityRent -= 1;
                    
                    db.WaitList.Remove(firstWaitingUser);
                    
                    await db.SaveChangesAsync();

                    var remainingUsers = await db.WaitList
                        .Where(w => w.BookID == bookId)
                        .OrderBy(w => w.WaitPosition)
                        .ToListAsync();

                    for (int i = 0; i < remainingUsers.Count; i++)
                    {
                        remainingUsers[i].WaitPosition = i + 1;
                    }

                    await db.SaveChangesAsync();

                    return Json(new { 
                        success = true, 
                        actionType = "rent",
                        message = "הספר זמין עבורך להשכרה"
                    });
                }
                else
                {
                    var userWaitingEntry = await db.WaitList
                        .FirstOrDefaultAsync(w => w.BookID == bookId && w.UserID == userId);

                    if (userWaitingEntry != null)
                    {
                        return Json(new {
                            success = true,
                            actionType = "waitlist_info",
                            message = $"מיקומך ברשימת ההמתנה: {userWaitingEntry.WaitPosition}"
                        });
                    }
                    else
                    {
                        var lastPosition = await db.WaitList
                            .Where(w => w.BookID == bookId)
                            .Select(w => (int?)w.WaitPosition)
                            .DefaultIfEmpty(0)
                            .MaxAsync();

                        var newWaitListItem = new WaitList
                        {
                            BookID = bookId,
                            UserID = userId,
                            WaitPosition = lastPosition + 1,
                            AddedDate = DateTime.Now,
                            EmailNotificationSent = false,
                            IsRental = true,
                            IsReserved = false
                        };

                        db.WaitList.Add(newWaitListItem);
                        await db.SaveChangesAsync();

                        return Json(new { 
                            success = true, 
                            actionType = "waitlist_signup",
                            message = $"הוספת לרשימת ההמתנה במקום {newWaitListItem.WaitPosition}"
                        });
                    }
                }
            }

            return Json(new { 
                success = true, 
                actionType = "rent",
                message = "הספר זמין להשכרה"
            });
        }

        return Json(new { 
            success = true, 
            actionType = "waitlist_signup",
            message = "הספר אינו זמין כרגע להשכרה"
        });
    }
    catch (Exception ex)
    {
        return Json(new { 
            success = false, 
            actionType = "error",
            message = $"אירעה שגיאה: {ex.Message}"
        });
    }
}        
public async Task RemoveFromWaitListAfterRental(int bookId, int userId)
        {
            try
            {
                var waitListItem = await db.WaitList
                    .FirstOrDefaultAsync(w => w.BookID == bookId && w.UserID == userId);

                if (waitListItem != null)
                {
                    db.WaitList.Remove(waitListItem);
                    await db.SaveChangesAsync();

                    var remainingUsers = await db.WaitList
                        .Where(w => w.BookID == bookId)
                        .OrderBy(w => w.WaitPosition)
                        .ToListAsync();

                    for (int i = 0; i < remainingUsers.Count; i++)
                    {
                        remainingUsers[i].WaitPosition = i + 1;
                        remainingUsers[i].EmailNotificationSent = false;
                    }

                    await db.SaveChangesAsync();

                    var book = await db.Books.FindAsync(bookId);
                    if (book != null && book.StockQuantityRent > 0)
                    {
                        await CheckAndUpdateWaitList(bookId);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"שגיאה בהסרה מרשימת המתנה: {ex.Message}");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetPopularBooks(int page = 1, int pageSize = 12)
        {
            var booksQuery = db.Books
                .Where(b => b.Reviews.Any(r => r.RatingBook.HasValue))
                .Select(b => new
                {
                    b.BookID,
                    b.Title,
                    b.MainAuthor,
                    b.ImageSrc,
                    ReviewStats = new
                    {
                        AverageRating = Math.Round(
                            b.Reviews
                                .Where(r => r.RatingBook.HasValue)
                                .Select(r => (decimal)r.RatingBook.Value)
                                .DefaultIfEmpty(0m)
                                .Average()
                            , 1), 
                        ReviewCount = b.Reviews.Count(r => r.RatingBook.HasValue),
                        Reviews = b.Reviews
                            .Where(r => r.RatingBook.HasValue)
                            .OrderByDescending(r => r.ReviewDateBook)
                            .Select(r => new 
                            {
                                Rating = r.RatingBook ?? 0,
                                Comment = r.ReviewTextBook,
                                ReviewDate = r.ReviewDateBook ?? DateTime.Now
                            })
                            .Take(5)
                    }
                })
                .OrderByDescending(x => x.ReviewStats.AverageRating) 
                .ThenByDescending(x => x.ReviewStats.ReviewCount)  
                .ThenBy(x => x.Title);                        

            var books = await booksQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var hasMore = await booksQuery
                .Skip(page * pageSize)
                .AnyAsync();

            return Json(new { books, hasMore }, JsonRequestBehavior.AllowGet);
        }
    }
    
}