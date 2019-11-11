using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Schulungsportal_2.Data;

namespace Schulungsportal_2
{
    public class Util
    {
        // logger
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        ///
        /// Gibt die Grund URL für den Request an, z.B. https://localhost:123 oder https://schulungen.test.de
        ///
        public static String getRootUrl(HttpRequest request) {
            if (request != null) {
                return string.Format("https://{0}", request.Host);
            } else {
                logger.Warn("Couldn't get current request, returning default!");
                return "http://localhost";
            }
        }

        public static String getVorstand(ApplicationDbContext context) {
            var impressum = context.Impressum.FirstOrDefault();
            if (impressum == null)
            {
                return "";
            }
            else
            {
                return impressum.Vorstand;
            }
        }
    }
}