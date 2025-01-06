using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DigireadProject.Models.Services
{

    public class WaitListService
    {
        private readonly libraryProject_digireadEntities _db;
        private readonly EmailService _emailService;

        public WaitListService(libraryProject_digireadEntities db, EmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public async Task ProcessWaitListItem(int bookId)
        {
            var book = await _db.Books.FindAsync(bookId);
            if (book == null || book.StockQuantityRent <= 0) return;

            // מצא את המשתמש הראשון ברשימת ההמתנה שעדיין לא קיבל התראה
            var firstWaitingUser = await _db.WaitList
                .Where(w => w.BookID == bookId && !w.EmailNotificationSent.GetValueOrDefault())
                .OrderBy(w => w.WaitPosition)
                .Include(w => w.Users)
                .FirstOrDefaultAsync();

            if (firstWaitingUser?.Users?.Email != null)
            {
                // שלח התראה למשתמש
                await _emailService.SendBookAvailableNotificationAsync(
                    firstWaitingUser.Users.Email,
                    book.Title
                );

                // סמן שנשלחה התראה
                firstWaitingUser.EmailNotificationSent = true;
                await _db.SaveChangesAsync();

                // תזמן מחיקה אוטומטית אחרי 30 דקות
                await Task.Delay(TimeSpan.FromMinutes(30));

                // בדוק אם המשתמש עדיין לא השאיל את הספר
                var userRented = await _db.Rentals
                    .AnyAsync(r => r.UserID == firstWaitingUser.UserID && 
                                  r.BookID == bookId && 
                                  r.ReturnDate == null);

                if (!userRented)
                {
                    // מחק את המשתמש מרשימת ההמתנה
                    _db.WaitList.Remove(firstWaitingUser);

                    // עדכן את המיקומים של שאר המשתמשים
                    var remainingUsers = await _db.WaitList
                        .Where(w => w.BookID == bookId)
                        .OrderBy(w => w.WaitPosition)
                        .ToListAsync();

                    for (int i = 0; i < remainingUsers.Count; i++)
                    {
                        remainingUsers[i].WaitPosition = i + 1;
                        remainingUsers[i].EmailNotificationSent = false;
                    }

                    await _db.SaveChangesAsync();

                    // אם יש עדיין מלאי, התחל את התהליך מחדש עם המשתמש הבא
                    if (book.StockQuantityRent > 0)
                    {
                        await ProcessWaitListItem(bookId);
                    }
                }
            }
        }
        
        public async Task RemoveFromWaitListAfterSuccessfulRental(int bookId, int userId)
        {
            // מחפש את הפריט ברשימת ההמתנה
            var waitListItem = await _db.WaitList
                .FirstOrDefaultAsync(w => w.BookID == bookId && w.UserID == userId);

            if (waitListItem != null)
            {
                // שומר את המיקום שהיה למשתמש
                int oldPosition = waitListItem.WaitPosition ?? 0;

                // מוחק את המשתמש מרשימת ההמתנה
                _db.WaitList.Remove(waitListItem);

                // מעדכן את המיקומים של שאר המשתמשים
                var remainingUsers = await _db.WaitList
                    .Where(w => w.BookID == bookId && w.WaitPosition > oldPosition)
                    .ToListAsync();

                foreach (var user in remainingUsers)
                {
                    user.WaitPosition--;
                    user.EmailNotificationSent = false;
                }

                await _db.SaveChangesAsync();

                // בדיקה אם יש עדיין ספר במלאי
                var book = await _db.Books.FindAsync(bookId);
                if (book != null && book.StockQuantityRent > 0)
                {
                    // מציאת המשתמש הראשון ברשימת ההמתנה
                    var firstWaitingUser = await _db.WaitList
                        .Where(w => w.BookID == bookId)
                        .OrderBy(w => w.WaitPosition)
                        .Include(w => w.Users)
                        .FirstOrDefaultAsync();

                    // אם יש משתמש ראשון ויש לו אימייל, שולח לו התראה
                    if (firstWaitingUser?.Users?.Email != null)
                    {
                        await _emailService.SendBookAvailableNotificationAsync(
                            firstWaitingUser.Users.Email,
                            book.Title
                        );
                        firstWaitingUser.EmailNotificationSent = true;
                        await _db.SaveChangesAsync();
                    }
                }
            }
        }    }
}