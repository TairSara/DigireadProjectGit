using System.Net.Mail;
using System.Threading.Tasks;

public class EmailService
{
    private readonly SmtpClient _smtpClient;

    public EmailService()
    {
        _smtpClient = new SmtpClient();
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

        await _smtpClient.SendMailAsync(mailMessage);
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

        await _smtpClient.SendMailAsync(mailMessage);
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
                <p>מהר לרכוש או להשאיל את הספר לפני שמישהו אחר יקדים אותך!</p>
                <p>תודה על שימושך בשירותי DigiRead!</p>
            </div>"
        };
        mailMessage.To.Add(email);

        await _smtpClient.SendMailAsync(mailMessage);
    }
}