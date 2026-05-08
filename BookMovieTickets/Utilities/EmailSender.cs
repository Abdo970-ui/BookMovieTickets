using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace BookMovieTickets.Utilities
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("abdoreyad034@gmail.com", "vgxj syyo xkny lypf")
            };


            var mail = new MailMessage(from: "abdoreyad034@gmail.com", to: email, subject, htmlMessage)
            {
                IsBodyHtml = true
            };

            return client.SendMailAsync(mail);
        }
    }
}

