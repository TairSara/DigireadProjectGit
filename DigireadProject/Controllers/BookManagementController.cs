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

        public BookManagementController()
        {
            db = new libraryProject_digireadEntities();
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
                Description = book.Description
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
                // עדכן תאריך החזרה
                rental.ReturnDate = DateTime.Now;

                // החזר את הספר לזמינות
                var book = await db.Books.FindAsync(rental.BookID);
                if (book != null)
                {
                    book.IsAvailable = true;
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

            // נביא קודם את כל הרכישות
            var purchasedBookIds = await db.Purchases
                .Where(p => p.UserID == userId)
                .Select(p => p.BookID)
                .ToListAsync();

            // נביא את ההשאלות הפעילות
            var rentalsWithDates = await db.Rentals
                .Where(r => r.UserID == userId && r.ReturnDate == null)
                .Select(r => new { r.BookID, r.RentalDate })
                .ToListAsync();

            var rentedBookIds = rentalsWithDates.Select(r => r.BookID).ToList();

            // נביא את כל הספרים הרלוונטיים בשאילתה אחת
            var allRelevantBooks = await db.Books
                .Where(b => purchasedBookIds.Contains(b.BookID) || rentedBookIds.Contains(b.BookID))
                .ToListAsync();

            // נבנה את המודל עבור ספרים שנרכשו
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

            // נבנה את המודל עבור ספרים מושאלים
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
                        ReturnDate = rental.RentalDate?.AddDays(14)
                    });
                }
            }

            return View(result);
        }

        private int GetCurrentUserId()
        {
            var username = User.Identity.Name;
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            return user?.UserID ?? 0;
        }
    }

}