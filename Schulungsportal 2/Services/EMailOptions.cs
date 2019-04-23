using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schulungsportal_2.Services
{
    public class EMailOptions
    {
        public string Mailserver { get; set; }
        public int Port { get; set; }
        public string Absender { get; set; }
        public string Passwort { get; set; }
    }
}
