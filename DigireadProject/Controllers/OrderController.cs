using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DigireadProject.Models;

namespace DigireadProject.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly libraryProject_digireadEntities _db;

        public OrderController()
        {
            _db = new libraryProject_digireadEntities();
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
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var userId = GetCurrentUserId();
                    var cartItems = await _db.ShoppingCart
                        .Where(c => c.UserID == userId)
                        .ToListAsync();

                    if (!cartItems.Any())
                    {
                        return RedirectToAction("Cart", "BookManagement");
                    }

                    foreach (var item in cartItems)
                    {
                        if (item.IsRental == true)
                        {
                            var rental = new Rentals
                            {
                                UserID = userId,
                                BookID = item.BookID,
                                RentalDate = DateTime.Now,
                                ReturnDate = null
                            };
                            _db.Rentals.Add(rental);
                        }
                        else
                        {
                            var purchase = new Purchases
                            {
                                UserID = userId,
                                BookID = item.BookID,
                                PurchaseDate = DateTime.Now,
                                PaymentStatus = true,
                                PaymentMethod = "Credit Card"
                            };
                            _db.Purchases.Add(purchase);
                        }

                        var book = await _db.Books.FindAsync(item.BookID);
                        if (book != null)
                        {
                            book.StockQuantity -= item.Quantity;
                            if (book.StockQuantity <= 0)
                            {
                                book.IsAvailable = false;
                            }
                        }
                    }

                    _db.ShoppingCart.RemoveRange(cartItems);
                    await _db.SaveChangesAsync();
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
            return _db.Users.FirstOrDefault(u => u.Username == username)?.UserID ?? 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}