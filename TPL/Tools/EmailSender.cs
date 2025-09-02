using System.Net;
using System.Net.Mail;

namespace TPLWeb.Tools
{
    public interface IEmailSender
    {
        Task SendEmailAsync(EmailModel email);
        Task SendPasswordResetEmailAsync(string email, string resetLink, string userName);
        Task SendEmailConfirmationAsync(string email, string confirmationLink, string userName);
    }

    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(EmailModel email)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings").Get<List<EmailSettings>>()?.FirstOrDefault();
                if (emailSettings == null)
                {
                    throw new InvalidOperationException("Email settings not configured");
                }

                using var message = new MailMessage()
                {
                    From = new MailAddress(emailSettings.FromEmail!, emailSettings.FromName),
                    Subject = email.Subject,
                    Body = email.Body,
                    IsBodyHtml = true
                };

                message.To.Add(email.To);

                using var smtpClient = new SmtpClient(emailSettings.SmtpHost, emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(emailSettings.SmtpUsername, emailSettings.SmtpPassword),
                    EnableSsl = emailSettings.EnableSsl
                };

                await smtpClient.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {Email}", email.To);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email.To);
                throw new InvalidOperationException("Failed to send email", ex);
            }
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink, string userName)
        {
            var subject = "بازیابی رمز عبور - سامانه مدیریت مکاتبات";
            var body = GetPasswordResetEmailTemplate(resetLink, userName);
            
            var emailModel = new EmailModel(email, subject, body);
            await SendEmailAsync(emailModel);
        }

        public async Task SendEmailConfirmationAsync(string email, string confirmationLink, string userName)
        {
            var subject = "تایید ایمیل - سامانه مدیریت مکاتبات";
            var body = GetEmailConfirmationTemplate(confirmationLink, userName);
            
            var emailModel = new EmailModel(email, subject, body);
            await SendEmailAsync(emailModel);
        }

        private string GetPasswordResetEmailTemplate(string resetLink, string userName)
        {
            return $@"
<!DOCTYPE html>
<html dir='rtl' lang='fa'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: 'Tahoma', sans-serif; direction: rtl; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; overflow: hidden; box-shadow: 0 0 20px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; }}
        .button {{ display: inline-block; background: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; color: #6c757d; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>بازیابی رمز عبور</h2>
            <p>سامانه مدیریت مکاتبات</p>
        </div>
        <div class='content'>
            <p>سلام {userName}</p>
            <p>درخواست بازیابی رمز عبور برای حساب کاربری شما دریافت شد.</p>
            <p>برای تنظیم رمز عبور جدید، روی دکمه زیر کلیک کنید:</p>
            <div style='text-align: center;'>
                <a href='{resetLink}' class='button'>بازیابی رمز عبور</a>
            </div>
            <p><strong>توجه:</strong> این لینک تا ۲۴ ساعت معتبر است.</p>
            <p>اگر شما این درخواست را نداده‌اید، این ایمیل را نادیده بگیرید.</p>
        </div>
        <div class='footer'>
            <p>این ایمیل به صورت خودکار ارسال شده است. لطفا پاسخ ندهید.</p>
            <p>© {DateTime.Now.Year} سامانه مدیریت مکاتبات</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetEmailConfirmationTemplate(string confirmationLink, string userName)
        {
            return $@"
<!DOCTYPE html>
<html dir='rtl' lang='fa'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: 'Tahoma', sans-serif; direction: rtl; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; overflow: hidden; box-shadow: 0 0 20px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; }}
        .button {{ display: inline-block; background: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; color: #6c757d; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>تایید ایمیل</h2>
            <p>سامانه مدیریت مکاتبات</p>
        </div>
        <div class='content'>
            <p>سلام {userName}</p>
            <p>خوش آمدید! برای تکمیل فرآیند ثبت نام، لطفا ایمیل خود را تایید کنید.</p>
            <div style='text-align: center;'>
                <a href='{confirmationLink}' class='button'>تایید ایمیل</a>
            </div>
            <p>اگر شما در سایت ما ثبت نام نکرده‌اید، این ایمیل را نادیده بگیرید.</p>
        </div>
        <div class='footer'>
            <p>این ایمیل به صورت خودکار ارسال شده است. لطفا پاسخ ندهید.</p>
            <p>© {DateTime.Now.Year} سامانه مدیریت مکاتبات</p>
        </div>
    </div>
</body>
</html>";
        }
    }

    public class EmailModel
    {
        public EmailModel(string to, string subject, string body)
        {
            To = to;
            Subject = subject;
            Body = body;
        }

        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class EmailSettings
    {
        public string? SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public string? FromEmail { get; set; }
        public string? FromName { get; set; }
        public bool EnableSsl { get; set; }
    }
}