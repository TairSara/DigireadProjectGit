using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DigireadProject.Models.ViewModels;
using DigireadProject.Services;

namespace DigireadProject.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly libraryProject_digireadEntities db;
        private readonly EmailService _emailService;
        private readonly NotificationScheduler _notificationScheduler;

        public ShoppingCartController()
        {
            db = new libraryProject_digireadEntities();
            _emailService = new EmailService();
            _notificationScheduler = new NotificationScheduler(_emailService, db);
        }

        public ActionResult Cart()
        {
            int userId = GetCurrentUserId();
            var cartItems = db.ShoppingCart
                .Include(s => s.Books)
                .Where(s => s.UserID == userId)
                .ToList();
            ViewBag.PaypalClientId = System.Configuration.ConfigurationManager.AppSettings["PayPal:ClientId"];
            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Checkout()
        {
            try
            {
                int userId = GetCurrentUserId();
                System.Diagnostics.Debug.WriteLine($"התחלת תהליך Checkout עבור משתמש {userId}");

                var cartItems = await db.ShoppingCart
                    .Include(s => s.Books)
                    .Where(s => s.UserID == userId)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"נמצאו {cartItems.Count} פריטים בעגלה");

                foreach (var item in cartItems)
                {
                    var book = item.Books;
                    if (item.IsRental.GetValueOrDefault())
                    {
                        System.Diagnostics.Debug.WriteLine($"מטפל בהשאלת ספר {book.Title} (ID: {book.BookID})");

                        // בדיקת מלאי לפני ההשאלה
                        if (book.StockQuantityRent < (item.Quantity ?? 1))
                        {
                            System.Diagnostics.Debug.WriteLine($"אין מספיק מלאי להשאלה עבור ספר {book.Title}");
                            TempData["Error"] = $"הספר {book.Title} אינו זמין בכמות המבוקשת להשאלה";
                            return RedirectToAction("Cart");
                        }

                        var rental = new Rentals
                        {
                            UserID = userId,
                            BookID = item.BookID,
                            RentalDate = DateTime.Now,
                            ReturnDate = null,
                            ImageSrc = book.ImageSrc,
                            DaysOverdue = 0
                        };

                        db.Rentals.Add(rental);
                        book.StockQuantityRent -= item.Quantity ?? 1;

                        // מחיקת המשתמש מרשימת ההמתנה אם הוא נמצא בה
                        var waitListItem = await db.WaitList
                            .FirstOrDefaultAsync(w => w.BookID == item.BookID && w.UserID == userId);

                        if (waitListItem != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"נמצא פריט ברשימת ההמתנה למשתמש {userId} עבור ספר {book.Title}");
                            
                            // שמירת המיקום של הפריט שנמחק
                            int deletedPosition = waitListItem.WaitPosition ?? 0;
                            System.Diagnostics.Debug.WriteLine($"מיקום נוכחי ברשימת ההמתנה: {deletedPosition}");

                            // מחיקת הפריט מרשימת ההמתנה
                            db.WaitList.Remove(waitListItem);

                            // עדכון המיקומים של שאר המשתמשים
                            var remainingItems = await db.WaitList
                                .Where(w => w.BookID == item.BookID && w.WaitPosition > deletedPosition)
                                .ToListAsync();

                            System.Diagnostics.Debug.WriteLine($"מעדכן {remainingItems.Count} משתמשים נוספים ברשימת ההמתנה");

                            foreach (var remainingItem in remainingItems)
                            {
                                remainingItem.WaitPosition--;
                                remainingItem.EmailNotificationSent = false;
                                System.Diagnostics.Debug.WriteLine($"עדכון מיקום משתמש {remainingItem.UserID} ל-{remainingItem.WaitPosition}");
                            }

                            // בדיקה אם יש מלאי זמין למשתמשים ברשימת ההמתנה
                            if (book.StockQuantityRent > 0)
                            {
                                await HandleWaitListNotifications(book.BookID, book.Title);
                                
                                // עדכון סטטוס EmailNotificationSent עבור שלושת המשתמשים הראשונים
                                var topThreeUsers = remainingItems
                                    .OrderBy(w => w.WaitPosition)
                                    .Take(3);
                                    
                                foreach (var waitUser in topThreeUsers)
                                {
                                    waitUser.EmailNotificationSent = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"מטפל ברכישת ספר {book.Title}");

                        if (book.StockQuantity < (item.Quantity ?? 1))
                        {
                            TempData["Error"] = $"הספר {book.Title} אינו זמין בכמות המבוקשת לרכישה";
                            return RedirectToAction("Cart");
                        }

                        var purchase = new Purchases
                        {
                            UserID = userId,
                            BookID = item.BookID,
                            PurchaseDate = DateTime.Now,
                            PaymentStatus = true,
                            PaymentMethod = "כרטיס אשראי"
                        };

                        db.Purchases.Add(purchase);
                        book.StockQuantity -= item.Quantity ?? 1;
                    }

                    db.ShoppingCart.Remove(item);
                }

                await db.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("תהליך הרכישה הושלם בהצלחה");

                return RedirectToAction("Success", "Order");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"שגיאה בתהליך הרכישה: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["Error"] = "אירעה שגיאה בביצוע ההזמנה";
                return RedirectToAction("Cart");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int bookId, bool isRental)
        {
            try
            {
                int userId = GetCurrentUserId();
                
                if (isRental)
                {
                    var activeRentals = db.Rentals
                        .Count(r => r.UserID == userId && 
                                    r.ReturnDate == null && 
                                    DbFunctions.AddDays(r.RentalDate, 30) >= DateTime.Now);
                           
                    var cartRentals = db.ShoppingCart
                        .Count(s => s.UserID == userId && s.IsRental == true);
                           
                    if (activeRentals + cartRentals >= 3)
                    {
                        return Json(new { 
                            success = false, 
                            message = "שים לב! לא ניתן להשאיל יותר מ-3 ספרים במקביל",
                            isRentalLimit = true
                        });
                    }
                }
                
                var book = db.Books.Find(bookId);
                if (book == null)
                    return Json(new { success = false, message = "הספר לא נמצא" });

                int availableStock = isRental ? 
                    (book.StockQuantityRent ?? 0) : 
                    (book.StockQuantity ?? 0);

                var existingItem = db.ShoppingCart
                    .FirstOrDefault(s => s.UserID == userId && s.BookID == bookId && s.IsRental == isRental);

                if (existingItem != null)
                {
                    if ((existingItem.Quantity ?? 0) >= availableStock)
                    {
                        return Json(new { success = false, message = $"יש רק {availableStock} ספרים במלאי" });
                    }
                    existingItem.Quantity++;
                }
                else
                {
                    if (availableStock < 1)
                    {
                        return Json(new { success = false, message = "אין מלאי" });
                    }

                    var cartItem = new ShoppingCart
                    {
                        UserID = userId,
                        BookID = bookId,
                        Price = isRental ? book.RentalPrice : book.PurchasePrice,
                        DateAdded = DateTime.Now,
                        Quantity = 1,
                        IsRental = isRental,
                        ImageSrc = book.ImageSrc
                    };
                    db.ShoppingCart.Add(cartItem);
                }

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int cartId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var cartItem = db.ShoppingCart
                    .FirstOrDefault(c => c.CartID == cartId && c.UserID == userId);

                if (cartItem == null)
                    return Json(new { success = false, message = "פריט לא נמצא" });

                db.ShoppingCart.Remove(cartItem);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(int cartId, int quantity)
        {
            try
            {
                int userId = GetCurrentUserId();
                var cartItem = db.ShoppingCart
                    .Include(s => s.Books)
                    .FirstOrDefault(c => c.CartID == cartId && c.UserID == userId);

                if (cartItem == null)
                    return Json(new { success = false, message = "פריט לא נמצא" });
                
                if (cartItem.IsRental == true)
                {
                    var activeRentals = db.Rentals
                        .Count(r => r.UserID == userId && 
                                    r.ReturnDate == null);
                           
                    var otherCartRentals = db.ShoppingCart
                        .Count(s => s.UserID == userId && 
                                    s.IsRental == true && 
                                    s.CartID != cartId);
                           
                    if (activeRentals + otherCartRentals + quantity > 3)
                    {
                        return Json(new { 
                            success = false, 
                            message = "לא ניתן להשאיל יותר מ-3 ספרים במקביל" 
                        });
                    }
                }

                int availableStock = cartItem.IsRental.GetValueOrDefault() ? 
                    (cartItem.Books.StockQuantityRent ?? 0) : 
                    (cartItem.Books.StockQuantity ?? 0);

                if (quantity > availableStock)
                    return Json(new { 
                        success = false, 
                        message = $"יש רק {availableStock} ספרים במלאי. נא לעדכן את הכמות בהתאם." 
                    });

                if (quantity < 1)
                    return Json(new { success = false, message = "כמות לא תקינה" });

                cartItem.Quantity = quantity;
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var username = User.Identity.Name;
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            return user?.UserID ?? 0;
        }
        
        private async Task HandleWaitListNotifications(int bookId, string bookTitle)
        {
            var waitListUsers = await db.WaitList
                .Include(w => w.Users)
                .Where(w => w.BookID == bookId)
                .OrderBy(w => w.WaitPosition)
                .Take(3)
                .ToListAsync();

            if (waitListUsers.Any())
            {
                _notificationScheduler.ScheduleNotifications(waitListUsers, bookTitle);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
            }
            base.Dispose(disposing);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ProcessPayment()
        {
            try
            {
                await Checkout();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}