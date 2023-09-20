using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Schulungsportal_2.Data;
using Schulungsportal_2.Models;
using Schulungsportal_2.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Schulungsportal_2.Controllers
{
    /// <summary>
    /// Der HomeController beinhaltet alle Aktionen, die sich auf Allgemeines zum Schulungsportal beziehen
    /// </summary>
    public class HomeController : Controller
    {
        ApplicationDbContext _context;

        // logger
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn im Browser die ../Home/Index Seite aufgerufen wird. Sie gibt dem Framework die Index-View zurück, aus der dieses die HTML-Datei erzeugt, um sie dem Browser zusenden.
        /// Falls noch kein Impressum-Objekt in der Datenbank hinterlegtist, wird ein neues Objekt eingefuegt.
        /// </summary>
        /// <returns> die View für ../Home/Index </returns>
        public ActionResult Index()
        {
            if (User.IsInRole("Verwaltung"))
            {
                return RedirectToAction("Uebersicht", "Schulung"); //Angemeldeter Nutzer wird direkt auf die Schulungsübersicht geleitet
            }
            return RedirectToAction("Anmeldung", "Anmeldung");
        }
    }
}
