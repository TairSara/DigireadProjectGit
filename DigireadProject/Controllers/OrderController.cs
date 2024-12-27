using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DigireadProject.Models.ViewModels; 

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
        public ActionResult CompletePurchase()
        {
            return RedirectToAction("PaymentForm");
        }

        [HttpGet]
        public ActionResult PaymentForm()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                var cartItems = db.ShoppingCart
                    .Where(c => c.UserID == userId)
                    .ToList();

                if (!cartItems.Any())
                {
                    return RedirectToAction("Cart", "ShoppingCart");
                }

                var totalAmount = cartItems.Sum(x => x.Price * x.Quantity);
                ViewBag.TotalAmount = totalAmount;

                System.Diagnostics.Debug.WriteLine($"Loading PaymentForm for user {userId} with total amount {totalAmount}");

                return View(new PaymentViewModel());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PaymentForm: {ex.Message}");
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ProcessPayment(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.TotalAmount = model.Price;
                return View("PaymentForm", model);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var userId = GetCurrentUserId();
                    var book = await db.Books.FindAsync(model.BookId);
                    
                    if (book == null)
                    {
                        ModelState.AddModelError("", "הספר לא נמצא");
                        return View("PaymentForm", model);
                    }

                    if (model.IsRental)
                    {
                        if (book.StockQuantityRent <= 0)
                        {
                            ModelState.AddModelError("", "הספר אינו זמין להשאלה כרגע");
                            return View("PaymentForm", model);
                        }

                        var rental = new Rentals
                        {
                            UserID = userId,
                            BookID = model.BookId,
                            RentalDate = DateTime.Now,
                            ReturnDate = null
                        };
                        db.Rentals.Add(rental);
                        book.StockQuantityRent -= 1;
                    }
                    else
                    {
                        if (book.StockQuantity <= 0)
                        {
                            ModelState.AddModelError("", "הספר אינו זמין לרכישה כרגע");
                            return View("PaymentForm", model);
                        }

                        var purchase = new Purchases
                        {
                            UserID = userId,
                            BookID = model.BookId,
                            PurchaseDate = DateTime.Now,
                            PaymentStatus = true,
                            PaymentMethod = "Credit Card"
                        };
                        db.Purchases.Add(purchase);
                        book.StockQuantity -= 1;

                        if (book.StockQuantity <= 0)
                        {
                            book.IsAvailable = false;
                        }
                    }

                    await db.SaveChangesAsync();
                    transaction.Commit();

                    TempData["PurchaseSuccess"] = true;
                    return RedirectToAction("Checkout");
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "אירעה שגיאה בביצוע התשלום");
                    return View("PaymentForm", model);
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
        public async Task<ActionResult> DirectPurchase(int bookId, bool isRental = false)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                var book = await db.Books.FindAsync(bookId);
                if (book == null)
                {
                    return HttpNotFound();
                }

                if (isRental)
                {
                    // Check rental availability
                    if (book.IsForRent== false || book.StockQuantityRent <= 0)
                    {
                        return RedirectToAction("BookDetails", "BookManagement", new { id = bookId });
                    }

                    // Check rental limit
                    var activeRentals = await db.Rentals
                        .CountAsync(r => r.UserID == userId && r.ReturnDate == null);
                    if (activeRentals >= 3)
                    {
                        TempData["ErrorMessage"] = "לא ניתן להשאיל יותר מ-3 ספרים במקביל";
                        return RedirectToAction("BookDetails", "BookManagement", new { id = bookId });
                    }
                }
                else
                {
                    // Check purchase availability
                    if (book == null || book.IsAvailable == false || book.StockQuantity <= 0)
                    {
                        return RedirectToAction("BookDetails", "BookManagement", new { id = bookId });
                    }
                }

                var viewModel = new PaymentViewModel
                {
                    BookId = book.BookID,
                    BookTitle = book.Title,
                    BookImageSrc = book.ImageSrc,
                    Price = isRental ? book.RentalPrice ?? 0 : book.PurchasePrice ?? 0,
                    IsRental = isRental
                };

                ViewBag.TotalAmount = viewModel.Price;

                return View("PaymentForm", viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DirectPurchase: {ex.Message}");
                return RedirectToAction("BookDetails", "BookManagement", new { id = bookId });
            }
        }
    }
}

