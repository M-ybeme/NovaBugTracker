using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using NovaBugTracker.Models;
using MailKit.Net.Smtp;


namespace NovaBugTracker.Services
{
    public class BTEmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;

        public BTEmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailSender = _mailSettings.Email ?? Environment.GetEnvironmentVariable("EmailAddress");
            var host = _mailSettings.Host ?? Environment.GetEnvironmentVariable("EmailHost");
            var port = _mailSettings.EmailPort != 0 ? _mailSettings.EmailPort : int.Parse(Environment.GetEnvironmentVariable("EmailPort")!);
            var password = _mailSettings.Password ?? Environment.GetEnvironmentVariable("EmailPassword");

            MimeMessage newEmail = new();

            newEmail.Sender = MailboxAddress.Parse(emailSender);

            newEmail.To.Add(MailboxAddress.Parse(email));

            newEmail.Subject = subject;

            BodyBuilder emailBody = new();
            emailBody.HtmlBody = htmlMessage;
            newEmail.Body = emailBody.ToMessageBody();

            // try to send email
            SmtpClient smtpClient = new();
            try
            {
                // connect
                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(emailSender, password);

                // send
                await smtpClient.SendAsync(newEmail);

                //disconnect
                await smtpClient.DisconnectAsync(true);
            }
            catch
            {

                throw;
            }
        }
    }
}
