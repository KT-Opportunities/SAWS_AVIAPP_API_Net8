using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;

using SAWSCore8API.Models;
using SAWSCore8API.Interfaces;
using MailKit.Security;

namespace SAWSCore8API.Services
{
    public class EmailService : IEmailService
    {

        private readonly SmptSetting _appSettings;
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            var settings = new SmptSetting();

            if (settings != null)
            {

                settings.from = configuration["SmptSettings:from"];
                settings.userName = configuration["SmptSettings:userName"];
                settings.Password = configuration["SmptSettings:Password"];
                settings.regUrl = configuration["AppURL"] + "#/register";
                settings.enableSsl = bool.Parse(configuration["SmptSettings:EnableSsl"]);
                settings.Port = int.Parse(configuration["SmptSettings:Port"]);
                settings.host = configuration["SmptSettings:host"];
                settings.applicationUrl = configuration["SmptSettings:applicationUrl"];
            }

            _appSettings = settings;
            _configuration = configuration;
        }

        // public EmailService(IOptions<SmptSetting> appSettings, IConfiguration configuration)
        // {
        //     var settings = appSettings.Value;

        //     if (settings.userName == null)
        //     {

        //         settings.from = "notifications@weathersa.co.za";
        //         settings.userName = "notifications@weathersa.co.za";
        //         // settings.Password = "M@nagem3nt";
        //         settings.regUrl = "https://www.weathersa.co.za/#/register";
        //         settings.enableSsl = true;
        //         settings.Port = 25;

        //         settings.host = "smtp.weathersa.co.za";
        //     }

        //     _appSettings = settings;
        //     _configuration = configuration;

        // }

        public void Send(string to, string orgname, string fname, string lname)
        { 
        }

        public void SendPasswordResetEmail(string to, string htmlBody)
        {
            if (_appSettings == null)
            {
                throw new Exception("Error reading email settings");
            }

            try
            {
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress("South African Weather Service", _appSettings.from));
                mailMessage.To.Add(new MailboxAddress(to, to));
                mailMessage.Subject = "South African Weather Service forgot/reset password request";

                mailMessage.Body = new TextPart("html")
                {
                    Text = htmlBody
                };

                using (var smtpClient = new SmtpClient())
                {
                    //smtpClient.Connect(_appSettings.host, _appSettings.Port, SecureSocketOptions.StartTls
                    smtpClient.Connect(_appSettings.host, _appSettings.Port, _appSettings.enableSsl);
                    smtpClient.Authenticate(_appSettings.userName, _appSettings.Password);
                    smtpClient.Send(mailMessage);
                    smtpClient.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while sending password reset details.", ex);
            }
        }


        public void SendLogInCredentialsEmail(string to, string htmlBody)
        {
            if (_appSettings == null)
            {
                throw new Exception("Error reading email settings");
            }

            try
            {
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress("South African Weather Service", _appSettings.from));
                mailMessage.To.Add(new MailboxAddress(to, to));
                mailMessage.Subject = "South African Weather Service login credentials";

                mailMessage.Body = new TextPart("html")
                {
                    Text = htmlBody
                };

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect(_appSettings.host, _appSettings.Port, _appSettings.enableSsl);
                    smtpClient.Authenticate(_appSettings.userName, _appSettings.Password);
                    smtpClient.Send(mailMessage);
                    smtpClient.Disconnect(true);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error with sending login credentials", ex);

            }

        }
    }
}
