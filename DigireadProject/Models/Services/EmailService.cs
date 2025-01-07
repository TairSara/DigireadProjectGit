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
        private const string EmailTemplate = @"
            <!DOCTYPE html>
            <html dir='rtl' lang='he'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <link href='https://fonts.googleapis.com/css2?family=Heebo:wght@400;500;700&display=swap' rel='stylesheet'>
                <style>
                    :root {
                        --primary-color: #2563eb;
                        --secondary-color: #1e40af;
                        --background-color: #f8fafc;
                        --text-color: #1e293b;
                        --border-color: #e2e8f0;
                    }
                    
                    body {
                        font-family: 'Heebo', Arial, sans-serif;
                        line-height: 1.6;
                        margin: 0;
                        padding: 0;
                        background-color: var(--background-color);
                        color: var(--text-color);
                    }

                    .container {
                        max-width: 600px;
                        margin: 20px auto;
                        background: #fff;
                        border-radius: 16px;
                        box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
                        overflow: hidden;
                    }

                    .header {
                        background: linear-gradient(135deg, var(--primary-color), var(--secondary-color));
                        color: white;
                        padding: 2rem;
                        text-align: center;
                        position: relative;
                    }

                    .header::after {
                        content: '';
                        position: absolute;
                        bottom: -20px;
                        left: 0;
                        right: 0;
                        height: 40px;
                        background: #fff;
                        clip-path: polygon(0 0, 100% 0, 100% 100%, 0 0);
                    }

                    .content {
                        padding: 2rem;
                    }

                    .card {
                        background: var(--background-color);
                        border-radius: 12px;
                        padding: 1.5rem;
                        margin: 1.5rem 0;
                        border: 1px solid var(--border-color);
                        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
                    }

                    .button {
                        display: inline-block;
                        padding: 12px 24px;
                        background: linear-gradient(135deg, var(--primary-color), var(--secondary-color));
                        color: white;
                        text-decoration: none;
                        border-radius: 8px;
                        font-weight: 500;
                        margin: 1rem 0;
                        text-align: center;
                        transition: transform 0.2s;
                        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                    }

                    .button:hover {
                        transform: translateY(-2px);
                        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.15);
                    }

                    table {
                        width: 100%;
                        border-collapse: separate;
                        border-spacing: 0;
                        margin: 1rem 0;
                    }

                    th {
                        background: var(--primary-color);
                        color: white;
                        padding: 12px;
                        text-align: right;
                        font-weight: 500;
                    }

                    th:first-child {
                        border-radius: 8px 8px 0 0;
                    }

                    td {
                        padding: 12px;
                        border-bottom: 1px solid var(--border-color);
                    }

                    tr:last-child td {
                        border-bottom: none;
                    }

                    .highlight {
                        color: var(--primary-color);
                        font-weight: 500;
                    }

                    .footer {
                        text-align: center;
                        padding: 2rem;
                        background: var(--background-color);
                        color: #64748b;
                        font-size: 0.875rem;
                        border-top: 1px solid var(--border-color);
                    }

                    .alert {
                        border-right: 4px solid var(--primary-color);
                        background-color: #eff6ff;
                        padding: 1rem;
                        margin: 1rem 0;
                        border-radius: 8px;
                    }

                    @media (max-width: 600px) {
                        .container {
                            margin: 10px;
                            border-radius: 8px;
                        }
                        
                        .content {
                            padding: 1rem;
                        }
                    }
                </style>
            </head>
            <body>
                <div class='container'>
                    {0}
                    <div class='footer'>
                        <p>© DigiRead 2025 | כל הזכויות שמורות</p>
                        <p>צור קשר | מדיניות פרטיות | תנאי שימוש</p>
                    </div>
                </div>
            </body>
            </html>";

        public EmailService()
        {
            smtpClient = new SmtpClient();
        }

        public async Task SendOrderConfirmationAsync(string email, List<CartItemViewModel> items, decimal totalAmount)
        {
            string orderContent = $@"
                <div class='header'>
                    <h2>תודה על הזמנתך ב-DigiRead!</h2>
                    <p>הזמנתך התקבלה ואושרה בהצלחה.</p>
                </div>
                <div class='content'>
                    <div class='card'>
                        <h3>פרטי ההזמנה:</h3>
                        <table>
                            <thead>
                                <tr>
                                    <th>שם הספר</th>
                                    <th>סוג</th>
                                    <th>כמות</th>
                                    <th>מחיר</th>
                                </tr>
                            </thead>
                            <tbody>
                                {string.Join("", items.Select(item => $@"
                                    <tr>
                                        <td>{item.BookTitle}</td>
                                        <td>{(item.IsRental ? "השאלה" : "רכישה")}</td>
                                        <td>{item.Quantity}</td>
                                        <td>₪{item.Price:F2}</td>
                                    </tr>
                                "))}
                                <tr>
                                    <td colspan='3' style='text-align: left; font-weight: bold;'>סה״כ לתשלום:</td>
                                    <td style='font-weight: bold;'>₪{totalAmount:F2}</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                    <div style='text-align: center;'>
                        <p>תודה שבחרת ב-DigiRead!</p>
                        <p>נשמח לראותך שוב בקרוב.</p>
                    </div>
                </div>";

            var mailMessage = new MailMessage
            {
                From = new MailAddress("tairsto@ac.sce.ac.il"),
                Subject = "אישור הזמנה - DigiRead",
                IsBodyHtml = true,
                Body = string.Format(EmailTemplate, orderContent)
            };
            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            string resetContent = $@"
                <div class='header'>
                    <h2>בקשה לאיפוס סיסמה</h2>
                </div>
                <div class='content'>
                    <div class='alert'>
                        <p>קיבלנו בקשה לאיפוס הסיסמה שלך.</p>
                    </div>
                    <div class='card' style='text-align: center;'>
                        <p>כדי לאפס את הסיסמה, לחץ על הכפתור הבא:</p>
                        <a href='{resetLink}' class='button'>איפוס סיסמה</a>
                        <p style='color: #64748b; font-size: 0.875rem; margin-top: 1rem;'>
                            אם לא ביקשת לאפס את הסיסמה, אנא התעלם מהודעה זו.
                        </p>
                    </div>
                </div>";

            var mailMessage = new MailMessage
            {
                From = new MailAddress("tairsto@ac.sce.ac.il"),
                Subject = "איפוס סיסמה - DigiRead",
                IsBodyHtml = true,
                Body = string.Format(EmailTemplate, resetContent)
            };
            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendRentalExpirationAlertAsync(string email, string bookTitle, int daysLeft)
        {
            string alertContent = $@"
                <div class='header'>
                    <h2>התראה על סיום תקופת השאלה</h2>
                </div>
                <div class='content'>
                    <p>שלום,</p>
                    <div class='alert'>
                        <p>ברצוננו להודיע לך שנותרו <strong class='highlight'>{daysLeft} ימים</strong> להשאלת הספר:</p>
                        <h3 class='highlight'>{bookTitle}</h3>
                    </div>
                    <div class='card'>
                        <p>אנא שים לב שבתום תקופת ההשאלה הספר יוסר אוטומטית מספריית הספרים שלך.</p>
                    </div>
                    <div style='text-align: center;'>
                        <p>תודה על שימושך בשירותי DigiRead!</p>
                    </div>
                </div>";

            var mailMessage = new MailMessage
            {
                From = new MailAddress("tairsto@ac.sce.ac.il"),
                Subject = "התראה על סיום תקופת השאלה - DigiRead",
                IsBodyHtml = true,
                Body = string.Format(EmailTemplate, alertContent)
            };
            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendBookAvailableNotificationAsync(string email, string bookTitle)
        {
            string notificationContent = $@"
                <div class='header'>
                    <h2>הספר שביקשת זמין עכשיו!</h2>
                </div>
                <div class='content'>
                    <p>שלום,</p>
                    <div class='card'>
                        <p>שמחים לבשר לך שהספר שביקשת זמין כעת:</p>
                        <h3 class='highlight'>{bookTitle}</h3>
                        <p>מהר לרכוש או להשאיל את הספר לפני שמישהו אחר יקדים אותך!</p>
                    </div>
                    <div style='text-align: center;'>
                        <p>תודה על שימושך בשירותי DigiRead!</p>
                    </div>
                </div>";

            var mailMessage = new MailMessage
            {
                From = new MailAddress("tairsto@ac.sce.ac.il"),
                Subject = "הספר שביקשת זמין! - DigiRead",
                IsBodyHtml = true,
                Body = string.Format(EmailTemplate, notificationContent)
            };
            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}