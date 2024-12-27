using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;
using DigireadProject.Models;
using DigireadProject.Models.ViewModels;

namespace DigireadProject.Services
{
    public class NotificationScheduler
    {
        private static readonly ConcurrentDictionary<string, Timer> _scheduledNotifications = new ConcurrentDictionary<string, Timer>();
        private readonly EmailService _emailService;
        private readonly libraryProject_digireadEntities _db;

        public NotificationScheduler(EmailService emailService, libraryProject_digireadEntities db)
        {
            _emailService = emailService;
            _db = db;
        }

        public void ScheduleNotifications(IEnumerable<WaitList> waitListUsers, string bookTitle)
        {
            var users = waitListUsers.Take(3).ToArray();
            
            for (int i = 0; i < users.Length; i++)
            {
                var user = users[i];
                var delayHours = i * 24; // 0 hours for first user, 24 for second, 48 for third
                
                var notificationKey = $"{user.UserID}_{bookTitle}_{DateTime.Now.Ticks}";
                var timer = new Timer(delayHours * 60 * 60 * 1000); // Convert hours to milliseconds
                
                timer.Elapsed += async (sender, e) =>
                {
                    var userEmail = await _db.Users
                        .Where(u => u.UserID == user.UserID)
                        .Select(u => u.Email)
                        .FirstOrDefaultAsync();
                        
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        await SendNotificationAsync(userEmail, bookTitle);
                    }
                    
                    timer.Dispose();
                    _scheduledNotifications.TryRemove(notificationKey, out _);
                };
                
                timer.AutoReset = false;
                _scheduledNotifications.TryAdd(notificationKey, timer);
                timer.Start();
            }
        }

        private async Task SendNotificationAsync(string email, string bookTitle)
        {
            try
            {
                await _emailService.SendBookAvailableNotificationAsync(email, bookTitle);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending notification: {ex.Message}");
            }
        }

        public void CancelScheduledNotifications(string bookTitle)
        {
            var keysToRemove = _scheduledNotifications.Keys
                .Where(k => k.Contains(bookTitle))
                .ToList();

            foreach (var key in keysToRemove)
            {
                if (_scheduledNotifications.TryRemove(key, out var timer))
                {
                    timer.Dispose();
                }
            }
        }
    }
}