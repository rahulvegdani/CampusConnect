using System.Net;
using System.Net.Mail;

namespace CampusConnect.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(
                        _config["SmtpSettings:Email"],
                        _config["SmtpSettings:Password"]
                    ),
                    EnableSsl = true
                };

                var message = new MailMessage();
                message.From = new MailAddress(_config["SmtpSettings:Email"]);
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                await smtp.SendMailAsync(message);

                Console.WriteLine("EMAIL SENT SUCCESS ✅");
            }
            catch (Exception ex)
            {
                Console.WriteLine("EMAIL FAILED ❌");
                Console.WriteLine(ex.Message);
                throw; // 🔥 IMPORTANT (so frontend knows failure)
            }
        }
    }
}