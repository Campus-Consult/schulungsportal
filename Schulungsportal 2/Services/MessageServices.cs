using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Schulungsportal_2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schulungsportal_2.Services
{

    /// <summary>
    /// Das ist die Klasse, die den IEmailSender implementiert. Diese wird benutzt um den Verwaltungszugang zu Authentifizieren.
    /// </summary>
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link http://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        // logger
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IOptionsMonitor<EMailOptions> mailOptions;

        public static string adresseSchulungsbeauftragter = "schulungen@campus-consult.org";

        public AuthMessageSender(IOptionsMonitor<EMailOptions> options)
        {
            this.mailOptions = options;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("Schulungsportal", "schulungsportal@campus-consult.org")); //Absender
            mimeMessage.To.Add(new MailboxAddress(email, email)); // Empfaenger
            mimeMessage.Subject = subject; //Betreff

            mimeMessage.Body = new TextPart("plain") //Inhalt
            {
                Text = @message
            };

            await SendEmailAsync(mimeMessage);
            //return Task.FromResult(0);
        }

        /// <summary>
        /// Diese Methode erstellt einen SMTP-Client, der eine Nachricht verschickt.
        /// </summary>
        /// <param name="nachricht">Die Nachricht, die verschickt wird mit Absender, Empfänger, Betreff, Nachricht, Anhang,... (alles, was in einer Mail ist)</param>
        public async Task SendEmailAsync(MimeMessage nachricht)
        {
            try
            {
                SmtpClient client = new SmtpClient();
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(mailOptions.CurrentValue.Mailserver, mailOptions.CurrentValue.Port, MailKit.Security.SecureSocketOptions.StartTls);

                client.AuthenticationMechanisms.Remove("XOAUTH2");

                if (mailOptions.CurrentValue.Passwort != "")
                {
                    await client.AuthenticateAsync(mailOptions.CurrentValue.Absender, mailOptions.CurrentValue.Passwort);
                }
                await client.SendAsync(nachricht);
                await client.DisconnectAsync(true);
            } catch(Exception e)
            {
                logger.Error(e);
            }
        }

        public string GetAdresseSchulungsbeauftragter()
        {
            return adresseSchulungsbeauftragter;
        }

        public string GetAbsendeAdresse()
        {
            return mailOptions.CurrentValue.Absender;
        }

        // TODO mailProperties ganz entfernen
        public static void UpdateProperties(MailProperties mailproperties)
        {
            adresseSchulungsbeauftragter = mailproperties.adresseSchulungsbeauftragter;
        }

        /// <summary>
        /// Teil des Frameworks zum Versenden von SMS, wird nicht benutzt.
        /// </summary>
        public async Task SendSmsAsync(string number, string message)
        {
            // Plug in your SMS service here to send a text message.
        }
    }
}
