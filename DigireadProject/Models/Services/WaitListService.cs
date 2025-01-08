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

            var firstWaitingUser = await _db.WaitList
                .Where(w => w.BookID == bookId && !w.EmailNotificationSent.GetValueOrDefault())
                .OrderBy(w => w.WaitPosition)
                .Include(w => w.Users)
                .FirstOrDefaultAsync();

            if (firstWaitingUser?.Users?.Email != null)
            {
                await _emailService.SendBookAvailableNotificationAsync(
                    firstWaitingUser.Users.Email,
                    book.Title
                );

                firstWaitingUser.EmailNotificationSent = true;
                await _db.SaveChangesAsync();

                await Task.Delay(TimeSpan.FromMinutes(30));

                var userRented = await _db.Rentals
                    .AnyAsync(r => r.UserID == firstWaitingUser.UserID && 
                                  r.BookID == bookId && 
                                  r.ReturnDate == null);

                if (!userRented)
                {
                    _db.WaitList.Remove(firstWaitingUser);

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

                    if (book.StockQuantityRent > 0)
                    {
                        await ProcessWaitListItem(bookId);
                    }
                }
            }
        }
        
        public async Task RemoveFromWaitListAfterSuccessfulRental(int bookId, int userId)
        {
            var waitListItem = await _db.WaitList
                .FirstOrDefaultAsync(w => w.BookID == bookId && w.UserID == userId);

            if (waitListItem != null)
            {
                int oldPosition = waitListItem.WaitPosition ?? 0;

                _db.WaitList.Remove(waitListItem);

                var remainingUsers = await _db.WaitList
                    .Where(w => w.BookID == bookId && w.WaitPosition > oldPosition)
                    .ToListAsync();

                foreach (var user in remainingUsers)
                {
                    user.WaitPosition--;
                    user.EmailNotificationSent = false;
                }

                await _db.SaveChangesAsync();

                var book = await _db.Books.FindAsync(bookId);
                if (book != null && book.StockQuantityRent > 0)
                {
                    var firstWaitingUser = await _db.WaitList
                        .Where(w => w.BookID == bookId)
                        .OrderBy(w => w.WaitPosition)
                        .Include(w => w.Users)
                        .FirstOrDefaultAsync();

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