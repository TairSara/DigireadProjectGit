using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DigireadProject.Models.ViewModels;  

namespace DigireadProject.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private libraryProject_digireadEntities db = new libraryProject_digireadEntities();

        public ActionResult Cart()
        {
            int userId = GetCurrentUserId();
            var cartItems = db.ShoppingCart
                .Include(s => s.Books)
                .Where(s => s.UserID == userId)
                .ToList();
            return View(cartItems);
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
                        .Count(r => r.UserID == userId && r.ReturnDate == null);
                           
                    var cartRentals = db.ShoppingCart
                        .Count(s => s.UserID == userId && s.IsRental == true);
                           
                    if (activeRentals + cartRentals >= 3)
                    {
                        return Json(new { 
                            success = false, 
                            message = "שים לב! לא ניתן להשאיל יותר מ-3 ספרים במקביל",
                            isRentalLimit = true  // דגל מיוחד לזיהוי שזו שגיאת מגבלת השאלות
                        });
                    }
                }
                
                var book = db.Books.Find(bookId);
                if (book == null)
                    return Json(new { success = false, message = "הספר לא נמצא" });

                // המרה בטוחה של ערכי null
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
                
                // אם זו השאלה, נבדוק את המגבלה
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

                // בדיקת כמות המלאי בהתאם לסוג הפעולה
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
            }
            base.Dispose(disposing);
        }
        
        [HttpGet]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout()
        {
            try
            {
                int userId = GetCurrentUserId();
                var cartItems = db.ShoppingCart
                    .Include(s => s.Books)
                    .Where(s => s.UserID == userId)
                    .ToList();

                foreach (var item in cartItems)
                {
                    var book = item.Books;
                    if (item.IsRental.GetValueOrDefault())
                    {
                        // בדיקת מלאי לפני ההשאלה
                        if (book.StockQuantityRent < (item.Quantity ?? 1))
                        {
                            TempData["Error"] = $"הספר {book.Title} אינו זמין בכמות המבוקשת להשאלה";
                            return RedirectToAction("Cart");
                        }

                        var rental = new Rentals
                        {
                            UserID = userId,
                            BookID = item.BookID,
                            RentalDate = DateTime.Now,
                            ReturnDate = DateTime.Now.AddDays(30),
                            ImageSrc = item.Books.ImageSrc,
                            DaysOverdue = 0
                        };

                        db.Rentals.Add(rental);
                        book.StockQuantityRent -= item.Quantity ?? 1;  // עדכון מלאי השאלה
                    }
                    else
                    {
                        // בדיקת מלאי לפני הרכישה
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
                        book.StockQuantity -= item.Quantity ?? 1;  // עדכון מלאי רכישה
                    }

                    db.ShoppingCart.Remove(item);
                }

                db.SaveChanges();
                return RedirectToAction("Success", "Order");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "אירעה שגיאה בביצוע ההזמנה";
                return RedirectToAction("Cart");
            }
        }
        
    }
    
}