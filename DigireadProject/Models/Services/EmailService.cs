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
}