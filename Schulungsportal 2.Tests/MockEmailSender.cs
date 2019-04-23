using MimeKit;
using Schulungsportal_2.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schulungsportal_2_Tests
{
    class MockEmailSender : IEmailSender
    {
        public static List<MimeMessage> sentMessages = new List<MimeMessage>();

        public string GetAbsendeAdresse()
        {
            return "testing@campus-consult.org";
        }

        public string GetAdresseSchulungsbeauftragter()
        {
            return "schulungsbeauftragter@campus-consult.org";
        }

        /// <summary>
        /// Used by the framework so not implemented (yet)
        /// </summary>
        public Task SendEmailAsync(string email, string subject, string message)
        {
            throw new NotImplementedException();
        }

        /// instead of sending the message just store it
        public Task SendEmailAsync(MimeMessage message)
        {
            sentMessages.Add(message);
            return Task.FromResult(0);
        }
    }
}
