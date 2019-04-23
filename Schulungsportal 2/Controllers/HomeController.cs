using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Schulungsportal_2.Data;
using Schulungsportal_2.Models;
using Schulungsportal_2.Services;
using System;
using System.Linq;

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

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn im Browser die ../Home/Impressum Seite aufgerufen wird. Sie gibt dem Framework die Impressum-View mit einem Impressum-Model zurück, aus der dieses die HTML-Datei erzeugt, um sie dem Browser zusenden.
        /// </summary>
        /// <returns> die View für ../Home/Impressum </returns>
        public ActionResult Impressum()
        {
            try
            {
                //eine neue Instanz des ImpressumViewModels wird instanziiert. Hierüber können Informationen an die View gegeben werden.
                Impressum impressum = _context.Impressum.ToList().Find(i => i != null);
                if (impressum == null)
                {
                    Impressum defaultImpressum = new Impressum()
                    {
                        Verantwortungsbereich = "Das Impressum gilt nur für die Internetpräsenz unter der Adresse: ",
                        Dienstanbieter = "Campus Consult e.V.",
                        Vorstand = "Annika Burneleit, Björn Foth, Julia Pietrucha, Yannik Knickmeier, Laura Farnschläder",
                        JournalistischeVerantwortung = "Campus Consult e.V., Technologiepark 13, 33100 Paderborn",
                        Kommunikation = "Tel.: +49 (0) 52 51 /14 80 780",
                        Anschrift = "Technologiepark 13, 33100 Paderborn",
                    };
                    _context.Impressum.Add(defaultImpressum);
                    _context.SaveChanges();
                    impressum = defaultImpressum;
                }
                //die zu verwendende View wird mit der vorher gefüllten Instanz des ImpressumViewModels zurückgegeben
                return View("Impressum", impressum);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#401";
                e = new Exception("Fehler beim erstellen der View " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn im Browser die ../Home/Datenschutz Seite aufgerufen wird. Sie gibt dem Framework die Datenschutz-View zurück, aus der dieses die HTML-Datei erzeugt, um sie dem Browser zusenden.
        /// </summary>
        /// <returns> die View für ../Home/Datenschutz </returns>
        public ActionResult Datenschutz()
        {
            //die zu verwendende View wird zurückgegeben
            return View("Datenschutz");
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn in der Verwaltungsübersicht auf "Impressum bearbeiten" geklickt wird.
        /// </summary>
        /// <returns>Gibt die View zum Impressum bearbeiten mit dem aktuellen Impressum-Objekt aus der Datenbank wieder</returns>
        [HttpGet, ActionName("ImpressumBearbeiten")]
        [Authorize(Roles = "Verwaltung")]
        [Route("Home/ImpressumBearbeiten")]
        public ActionResult ImpressumBearbeiten()
        {
            try
            {
                Impressum impressum = _context.Impressum.First();
                if (impressum == null)
                {
                    return NotFound();
                }
                return View("ImpressumBearbeiten", impressum);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#402";
                e = new Exception("Fehler beim erstellen der View " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Speichert die Änderungen am Impressum in der Datenbank und leitet zur Impressum-View weiter
        /// </summary>
        /// <param name="impressum">Das bearbeitete Impressum, dass abgespeichert wird</param>
        /// <returns>Impressum-Methode aus dem HomeController</returns>
        [HttpPost, ActionName("ImpressumBearbeiten")]
        [ValidateAntiForgeryToken]
        [Route("Home/ImpressumBearbeiten")]
        public ActionResult ImpressumBearbeitenBestaetigt(Impressum impressum)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Entry(impressum).State = EntityState.Modified;
                    _context.SaveChanges();
                    return RedirectToAction("Impressum"); //Nach Abschluss der Aktion Weiterleitung zur Impressum-View

                }
                return View("ImpressumBearbeiten", impressum);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#403";
                e = new Exception("Fehler beim verarbeiten des Inputs " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn in der Verwaltungsübersicht auf "Mailserver Einstellungen bearbeiten" geklickt wird.
        /// </summary>
        /// <returns>Gibt die View zum Mailserver Einstellungen bearbeiten mit dem aktuellen MailProperties-Objekt aus der Datenbank wieder</returns>
        [HttpGet, ActionName("MailserverBearbeiten")]
        [Authorize(Roles = "Verwaltung")]
        [Route("Home/MailserverBearbeiten")]
        public ActionResult MailPropertiesBearbeiten()
        {
            try
            {
                //MailProperties mailproperties = _context.MailProperties.ToList().Find(i => i != null);
                //if (mailproperties == null)
                //{
                //    mailproperties = new MailProperties()
                //    {
                //        mailserver = "",
                //        port = ,
                //        useSsl = ,

                //        absender = "",
                //        passwort = "",

                //        adresseSchulungsbeauftragter = ""
                //    };
                //    _context.MailProperties.Update(mailproperties);
                //    _context.SaveChanges();
                //    mailproperties = _context.MailProperties.First();
                //}

                //return View("MailserverBearbeiten", mailproperties);
                throw new Exception();
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#404";
                e = new Exception("Fehler beim erstellen der View " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Speichert die Änderungen an den Mailserver Einstellungen in der Datenbank und leitet zur Startseite weiter
        /// </summary>
        /// <param name="mailproperties">Die bearbeiteten Mailserver Einstellungen, die abgespeichert werden</param>
        /// <returns> Weiterleitung zur Startseite </returns>
        [HttpPost, ActionName("MailserverBearbeiten")]
        [ValidateAntiForgeryToken]
        public ActionResult MailPropertiesBearbeitenBestaetigt(MailProperties mailproperties)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Entry(mailproperties).State = EntityState.Modified;
                    _context.SaveChanges();

                    AuthMessageSender.UpdateProperties(mailproperties);

                    return RedirectToAction("Index"); //Nach Abschluss der Aktion Weiterleitung zur Startseite

                }
                return View("MailserverBearbeiten", mailproperties);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#405";
                e = new Exception("Fehler beim verarbeiten des Inputs " + code, e);
                return View("Error", e);
            }
        }
    }
}
