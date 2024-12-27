using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DigireadProject.Models;
using DigireadProject.Controllers;

namespace DigireadProject.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly libraryProject_digireadEntities db;

        public OrderController()
        {
            db = new libraryProject_digireadEntities();
        }

        [HttpGet]
        public ActionResult Checkout()
        {
            if (TempData["PurchaseSuccess"] != null)
            {
                return View();
            }
            return RedirectToAction("Cart", "BookManagement");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CompletePurchase()
        {
            var bookManagementController = new BookManagementController();
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var userId = GetCurrentUserId();
                    var cartItems = await db.ShoppingCart
                        .Where(c => c.UserID == userId)
                        .ToListAsync();
                    var activeRentals = await db.Rentals
                        .CountAsync(r => r.UserID == userId && 
                                         r.ReturnDate == null);
                                
                    var rentalItemsInCart = cartItems
                        .Count(i => i.IsRental == true);
                
                    if (activeRentals + rentalItemsInCart > 3)
                    {
                        return Json(new { 
                            success = false, 
                            message = "לא ניתן להשאיל יותר מ-3 ספרים במקביל" 
                        });
                    }

                    if (!cartItems.Any())
                    {
                        return RedirectToAction("Cart", "BookManagement");
                    }

                    foreach (var item in cartItems)
                    {
                        var book = await db.Books.FindAsync(item.BookID);
                        if (book == null) continue;

                        if (item.IsRental == true)
                        {
                            // בדיקה אם יש מלאי להשאלה
                            if (book.StockQuantityRent <= 0)
                            {
                                return Json(new 
                                { 
                                    success = false, 
                                    message = $"הספר {book.Title} אינו זמין להשאלה כרגע. אנא נסה שוב מאוחר יותר." 
                                });
                            }

                            var rental = new Rentals
                            {
                                UserID = userId,
                                BookID = item.BookID,
                                RentalDate = DateTime.Now,
                                ReturnDate = null
                            };
                            db.Rentals.Add(rental);

                            // עדכון מלאי ההשאלות
                            book.StockQuantityRent -= 1;
                            await bookManagementController.RemoveFromWaitListAfterRental(item.BookID.Value, userId);
                        }
                        else
                        {
                            // טיפול ברכישה רגילה
                            var purchase = new Purchases
                            {
                                UserID = userId,
                                BookID = item.BookID,
                                PurchaseDate = DateTime.Now,
                                PaymentStatus = true,
                                PaymentMethod = "Credit Card"
                            };
                            db.Purchases.Add(purchase);

                            book.StockQuantity -= item.Quantity;
                            if (book.StockQuantity <= 0)
                            {
                                book.IsAvailable = false;
                            }
                        }
                    }

                    db.ShoppingCart.RemoveRange(cartItems);
                    await db.SaveChangesAsync();
                    transaction.Commit();

                    TempData["PurchaseSuccess"] = true;
                    return RedirectToAction("Checkout");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = ex.Message });
                }
            }
        }

        private int GetCurrentUserId()
        {
            var username = User.Identity.Name;
            return db.Users.FirstOrDefault(u => u.Username == username)?.UserID ?? 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        
        [HttpGet]
        public ActionResult QuickPurchase()
        {
            var userId = GetCurrentUserId();
            var cartItems = db.ShoppingCart
                .Include(s => s.Books)
                .Where(s => s.UserID == userId)
                .ToList();

            if (!cartItems.Any())
            {
                // If the cart is empty, redirect to the main gallery
                return RedirectToAction("MainGallery", "BookManagement");
            }

            // Render the Checkout view
            return View("Checkout");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> QuickPurchase(int bookId)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var userId = GetCurrentUserId();
                    var book = await db.Books.FindAsync(bookId);

                    if (book == null)
                    {
                        return Json(new { success = false, message = "The book was not found" });
                    }

                    // Create a new purchase record
                    var purchase = new Purchases
                    {
                        UserID = userId,
                        BookID = bookId,
                        PurchaseDate = DateTime.Now,
                        PaymentStatus = true,
                        PaymentMethod = "Credit Card"
                    };

                    db.Purchases.Add(purchase);

                    // Update the book stock
                    book.StockQuantity -= 1;
                    if (book.StockQuantity <= 0)
                    {
                        book.IsAvailable = false;
                    }

                    await db.SaveChangesAsync();
                    transaction.Commit();

                    // Set a flag to indicate a successful purchase
                    TempData["PurchaseSuccess"] = true;

                    // Redirect to the Checkout page
                    return RedirectToAction("Checkout");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = ex.Message });
                }
            }
        }
    }
}