using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using DigireadProject.Models.ViewModels;

namespace DigireadProject.Models.Services
{
    public class EmailService
    {
        private readonly SmtpClient smtpClient;

        public EmailService()
        {
            smtpClient = new SmtpClient();
        }
        public async Task SendOrderConfirmationAsync(string email, List<CartItemViewModel> items, decimal totalAmount)
{
    var mailMessage = new MailMessage
    {
        From = new MailAddress("tairsto@ac.sce.ac.il"),
        Subject = "אישור הזמנה - DigiRead",
        IsBodyHtml = true,
        Body = $@"
        <div style='direction: rtl; text-align: right; font-family: Arial, sans-serif;'>
            <h2 style='color: #007bff;'>תודה על הזמנתך ב-DigiRead!</h2>
            <p>הזמנתך התקבלה ואושרה בהצלחה.</p>
            
            <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                <h3>פרטי ההזמנה:</h3>
                <table style='width: 100%; border-collapse: collapse;'>
                    <thead>
                        <tr style='background-color: #007bff; color: white;'>
                            <th style='padding: 10px; text-align: right;'>שם הספר</th>
                            <th style='padding: 10px; text-align: right;'>סוג</th>
                            <th style='padding: 10px; text-align: right;'>כמות</th>
                            <th style='padding: 10px; text-align: right;'>מחיר</th>
                        </tr>
                    </thead>
                    <tbody>
                        {string.Join("", items.Select(item => $@"
                            <tr>
                                <td style='padding: 10px; border-bottom: 1px solid #ddd;'>{item.BookTitle}</td>
                                <td style='padding: 10px; border-bottom: 1px solid #ddd;'>{(item.IsRental ? "השאלה" : "רכישה")}</td>
                                <td style='padding: 10px; border-bottom: 1px solid #ddd;'>{item.Quantity}</td>
                                <td style='padding: 10px; border-bottom: 1px solid #ddd;'>₪{item.Price:F2}</td>
                            </tr>
                        "))}
                        <tr>
                            <td colspan='3' style='padding: 10px; text-align: left; font-weight: bold;'>סה״כ לתשלום:</td>
                            <td style='padding: 10px; font-weight: bold;'>₪{totalAmount:F2}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
            
            <p>תודה שבחרת ב-DigiRead!</p>
            <p>נשמח לראותך שוב בקרוב.</p>
        </div>"
    };
    mailMessage.To.Add(email);

    await smtpClient.SendMailAsync(mailMessage);
}
        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("tairsto@ac.sce.ac.il"),
                Subject = "איפוס סיסמה - DigiRead",
                IsBodyHtml = true,
                Body = $@"
                <div style='direction: rtl; text-align: right;'>
                    <h2>בקשה לאיפוס סיסמה</h2>
                    <p>קיבלנו בקשה לאיפוס הסיסמה שלך.</p>
                    <p>לחץ על הקישור הבא לאיפוס הסיסמה:</p>
                    <a href='{resetLink}' style='background-color: #007bff; color: white; padding: 10px 15px; text-decoration: none; border-radius: 5px;'>איפוס סיסמה</a>
                    <p>אם לא ביקשת לאפס את הסיסמה, אנא התעלם מהודעה זו.</p>
                </div>"
            };
            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendRentalExpirationAlertAsync(string email, string bookTitle, int daysLeft)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("tairsto@ac.sce.ac.il"),
                Subject = "התראה על סיום תקופת השאלה - DigiRead",
                IsBodyHtml = true,
                Body = $@"
                <div style='direction: rtl; text-align: right;'>
                    <h2>התראה על סיום תקופת השאלה</h2>
                    <p>שלום,</p>
                    <p>ברצוננו להודיע לך שנותרו <strong>{daysLeft} ימים</strong> להשאלת הספר:</p>
                    <h3 style='color: #007bff;'>{bookTitle}</h3>
                    <p>אנא שים לב שבתום תקופת ההשאלה הספר יוסר אוטומטית מספריית הספרים שלך.</p>
                    <p>תודה על שימושך בשירותי DigiRead!</p>
                </div>"
            };
            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }
    
        public async Task SendBookAvailableNotificationAsync(string email, string bookTitle)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("tairsto@ac.sce.ac.il"),
                Subject = "הספר שביקשת זמין! - DigiRead",
                IsBodyHtml = true,
                Body = $@"
            <div style='direction: rtl; text-align: right;'>
                <h2>הספר שביקשת זמין עכשיו!</h2>
                <p>שלום,</p>
                <p>שמחים לבשר לך שהספר שביקשת זמין כעת:</p>
                <h3 style='color: #007bff;'>{bookTitle}</h3>
                <p>ספר זמין עבורך ל4 שעות בלבד!</p>
                <p>מהר לרכוש או להשאיל את הספר לפני שמישהו אחר יקדים אותך!</p>
                <p>תודה על שימושך בשירותי DigiRead!</p>
            </div>"
            };
            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }
        
        
    }
    
}