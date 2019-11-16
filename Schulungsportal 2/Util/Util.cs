using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Schulungsportal_2.Data;

namespace Schulungsportal_2
{
    ///
    /// Sammlung nützlicher Methoden, die an verschienen Stellen verwendet werden
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
                // eigentlich sollte dies nicht auftreten, allerdings scheint dies in den Tests doch der Fall zu sein
                // daher diese Ausnahme
                logger.Warn("Couldn't get current request, returning default!");
                return "http://localhost";
            }
        }

        ///
        /// Liest den aktuellen Vorstand aus der Datenbank und gibt diesen zurück
        ///
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