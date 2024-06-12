using System.Net.Mail;
using System.Net;

namespace TodoApp.Services.Concretes
{
    public class SmtpService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly MailAddress _fromAddress;

        public SmtpService(string smtpHost, int smtpPort, string smtpUser, string smtpPass, string fromEmail)
        {
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
            _smtpUser = smtpUser;
            _smtpPass = smtpPass;
            _fromAddress = new MailAddress(fromEmail);
        }

        public async Task<bool> SendMail(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                var toAddress = new MailAddress(toEmail);
                using (var email = new MailMessage(_fromAddress, toAddress))
                {
                    email.IsBodyHtml = isHtml;
                    email.Subject = subject;
                    email.Body = body;

                    using (var smtp = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                        smtp.EnableSsl = true;

                        await smtp.SendMailAsync(email);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here as needed
                return false;
            }
        }
    }
}
