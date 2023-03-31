using ContactPro.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ContactPro.Services
{
    public class EmailService : IEmailSender
    {
        private readonly MailSettings _mailsettings;
        public EmailService(IOptions<MailSettings> mailsettings)
        {
            _mailsettings = mailsettings.Value;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailSender =_mailsettings.Email ?? Environment.GetEnvironmentVariable("Email");

            MimeMessage newEmail = new();

            newEmail.Sender = MailboxAddress.Parse(emailSender);

            foreach(var emailAddress in email.Split(","))
            {
                newEmail.To.Add(MailboxAddress.Parse(emailAddress));
            }
            newEmail.Subject = subject;
            BodyBuilder emailBody = new();
            emailBody.HtmlBody = htmlMessage;
            newEmail.Body = emailBody.ToMessageBody();

            //login to smtp
            using SmtpClient smtpClient = new();
            try {
                var host = _mailsettings.Host ?? Environment.GetEnvironmentVariable("Host");
                var port = _mailsettings.Port !=0 ? _mailsettings.Port : int.Parse(Environment.GetEnvironmentVariable("Port")!);
                var password = _mailsettings.Password ?? Environment.GetEnvironmentVariable("Password");

                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(emailSender, password);
                await smtpClient.SendAsync(newEmail);
                await smtpClient.DisconnectAsync(true);
                 
            }
            catch(Exception ex) {
                var error = ex.Message;
                throw;
            }

        }
    }
}
