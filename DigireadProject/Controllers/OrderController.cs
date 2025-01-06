using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DigireadProject.Models.ViewModels; 
using System.Data.Entity;
using DigireadProject.Models.Services;


namespace DigireadProject.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly libraryProject_digireadEntities db = new libraryProject_digireadEntities();
        private readonly EmailService _emailService;
        private readonly WaitListService _waitListService;

        public OrderController()
        {
            db = new libraryProject_digireadEntities();
            _emailService = new EmailService();
            _waitListService = new WaitListService(db, _emailService); 
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
        public ActionResult CompletePurchase(List<CartItemViewModel> Items)
        {
            if (Items == null || !Items.Any())
            {
                return RedirectToAction("Cart", "ShoppingCart");
            }

            var userId = GetCurrentUserId();
            TempData["CartItems"] = Items;
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

                var userCart = db.ShoppingCart
                    .Include(c => c.Books)
                    .Where(c => c.UserID == userId)
                    .Select(c => new CartItemViewModel
                    {
                        BookId = c.Books.BookID,
                        BookTitle = c.Books.Title,
                        BookImageSrc = c.Books.ImageSrc,  // הוספת שדה התמונה
                        Price = c.Price ?? 0,
                        Quantity = c.Quantity ?? 1,
                        IsRental = c.IsRental ?? false
                    })
                    .ToList();

                if (!userCart.Any())
                {
                    return RedirectToAction("Cart", "ShoppingCart");
                }

                decimal totalAmount = userCart.Sum(x => x.Price * x.Quantity);

                var viewModel = new PaymentViewModel
                {
                    BookId = userCart.First().BookId,
                    BookTitle = userCart.First().BookTitle,
                    BookImageSrc = db.Books.Find(userCart.First().BookId)?.ImageSrc,
                    Price = totalAmount,
                    IsRental = userCart.First().IsRental,
                    CartItems = userCart
                };

                return View(viewModel);
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
            if (model == null || !model.CartItems.Any())
            {
                ModelState.AddModelError("", "נתוני הספרים חסרים");
                return View("PaymentForm", model);
            }

            if (!ModelState.IsValid)
            {
                return View("PaymentForm", model);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var userId = GetCurrentUserId();
                    
                    foreach (var cartItem in model.CartItems)
                    {
                        var book = await db.Books.FindAsync(cartItem.BookId);
                        
                        if (book == null)
                        {
                            ModelState.AddModelError("", $"הספר {cartItem.BookTitle} לא נמצא");
                            return View("PaymentForm", model);
                        }

                        if (cartItem.IsRental)
                        {
                            if (book.StockQuantityRent < cartItem.Quantity)
                            {
                                ModelState.AddModelError("", $"הספר {cartItem.BookTitle} אינו זמין להשאלה בכמות המבוקשת");
                                return View("PaymentForm", model);
                            }

                            var rental = new Rentals
                            {
                                UserID = userId,
                                BookID = cartItem.BookId,
                                RentalDate = DateTime.Now,
                                ReturnDate = null
                            };
                            db.Rentals.Add(rental);
                            book.StockQuantityRent -= cartItem.Quantity;
                            var waitListService = new WaitListService(db, new EmailService());
                            await waitListService.RemoveFromWaitListAfterSuccessfulRental(cartItem.BookId, userId);

                        }
                        else
                        {
                            if (book.StockQuantity < cartItem.Quantity)
                            {
                                ModelState.AddModelError("", $"הספר {cartItem.BookTitle} אינו זמין לרכישה בכמות המבוקשת");
                                return View("PaymentForm", model);
                            }

                            var purchase = new Purchases
                            {
                                UserID = userId,
                                BookID = cartItem.BookId,
                                PurchaseDate = DateTime.Now,
                                PaymentStatus = true,
                                PaymentMethod = "Credit Card"
                            };
                            db.Purchases.Add(purchase);
                            book.StockQuantity -= cartItem.Quantity;

                            if (book.StockQuantity <= 0)
                            {
                                book.IsAvailable = false;
                            }
                        }

                        // מחיקת הפריט מעגלת הקניות
                        var cartItemToRemove = await db.ShoppingCart
                            .FirstOrDefaultAsync(sc => sc.UserID == userId && sc.BookID == cartItem.BookId);
                        if (cartItemToRemove != null)
                        {
                            db.ShoppingCart.Remove(cartItemToRemove);
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
                    await _waitListService.RemoveFromWaitListAfterSuccessfulRental(bookId, userId);
                }
                else
                {
                    // Check purchase availability
                    if (book == null || book.IsAvailable == false || book.StockQuantity <= 0)
                    {
                        return RedirectToAction("BookDetails", "BookManagement", new { id = bookId });
                    }
                }
                
                var cartItem = new CartItemViewModel
                {
                    BookId = book.BookID,
                    BookTitle = book.Title,
                    BookImageSrc = book.ImageSrc,
                    Price = isRental ? book.RentalPrice ?? 0 : book.PurchasePrice ?? 0,
                    Quantity = 1,
                    IsRental = isRental
                };

                var viewModel = new PaymentViewModel
                {
                    BookId = book.BookID,
                    BookTitle = book.Title,
                    BookImageSrc = book.ImageSrc,
                    Price = isRental ? book.RentalPrice ?? 0 : book.PurchasePrice ?? 0,
                    IsRental = isRental,
                    CartItems = new List<CartItemViewModel> { cartItem }  
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

