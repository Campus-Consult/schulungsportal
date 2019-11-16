using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schulungsportal_2.Data;
using Schulungsportal_2.Models;
using Schulungsportal_2.Models.Anmeldungen;
using Schulungsportal_2.Models.Schulungen;
using Schulungsportal_2.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Schulungsportal.Controllers
{
    /// <summary>
    /// Der SucheController beinhaltet alle Aktionen zum Durchsuchen des Archivs
    /// </summary>
    public class SucheController : Controller
    {
        // TODO: alles auf vue umstellen
        private SchulungRepository _schulungRepository;
        private AnmeldungRepository _anmeldungRepository;

        // logger
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// SucheController Konstruktor legt Repositories für Datenzugriff an.
        /// </summary>
        public SucheController(ApplicationDbContext context, IMapper mapper)
        {
            _schulungRepository = new SchulungRepository(context);
            _anmeldungRepository = new AnmeldungRepository(context, mapper);
        }

        /// <summary>
        /// Diese Methode gibt die Suche-View zurueck, um das Archiv nach Schulungen zu durchsuchen.
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Verwaltung")]
        public ActionResult Suche()
        {
            try
            {
            return View("Suche");
        }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#501";
                e = new Exception("Fehler beim erstellen der View " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird aufgerufen, wenn in der Suche-View nach dem Namen einer Schulung gesucht wird.
        /// </summary>
        /// <param name="sucheViewModel">Das sucheViewModel, das den string enthält, nachdem gesucht wird</param>
        /// <returns>Gibt eine Liste aller Schulungen wieder, die zur Suchanfrage passen</returns>
        public ActionResult SucheTitel(SucheViewModel sucheViewModel)
        {
            try
            {
                if (sucheViewModel.Titel == null) return RedirectToAction("Suche");

                List<Schulung> Schulungen = _schulungRepository.GetForArchivSortByDate()
                    .Where(s => s.Titel.ToLower().Contains(sucheViewModel.Titel.ToLower()))
                    .ToList();

                SearchResultViewModel srvw = new SearchResultViewModel
                {
                    schulung = Schulungen,
                    suchAnfrage = "Titel: " + sucheViewModel.Titel.ToLower(),
                    titel = "Titel"
                };

                return View("SuchErgebnis", srvw);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#502";
                e = new Exception("Fehler beim verarbeiten des Inputs " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird aufgerufen, wenn in der Suche-View nach dem Namen eines Dozenten gesucht wird.
        /// </summary>
        /// <param name="sucheViewModel">Das sucheViewModel, das den string enthält, nachdem gesucht wird</param>
        /// <returns>Gibt eine Liste aller Schulungen wieder, die zur Suchanfrage passen</returns>
        public ActionResult SucheDozent(SucheViewModel sucheViewModel)
        {
            try
            {
                if (sucheViewModel.VornameDozent == null && sucheViewModel.NachnameDozent == null) return RedirectToAction("Suche");
                if (sucheViewModel.VornameDozent == null) sucheViewModel.VornameDozent = "";
                if (sucheViewModel.NachnameDozent == null) sucheViewModel.NachnameDozent = "";

                List<Schulung> Schulungen = _schulungRepository.GetForArchivSortByDate()
                    .Where(s => s.NameDozent.ToLower().Contains(sucheViewModel.VornameDozent.ToLower()) && s.NameDozent.ToLower().Contains(sucheViewModel.NachnameDozent.ToLower()))
                    .ToList();
                SearchResultViewModel srvw = new SearchResultViewModel
                {
                    schulung = Schulungen,
                    suchAnfrage = "Dozent: " + sucheViewModel.VornameDozent + " " + sucheViewModel.NachnameDozent,
                    titel = "Dozent"
                };

                return View("SuchErgebnis", srvw);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#503";
                e = new Exception("Fehler beim verarbeiten des Inputs " + code, e);
                return View("Error", e);
            }
        }
        

        /// <summary>
        /// Diese Methode wird aufgerufen, wenn in der Suche-View nach einem Jahr gesucht wird.
        /// </summary>
        /// <param name="sucheViewModel">Das sucheViewModel, das den string enthält, nachdem gesucht wird</param>
        /// <returns>Gibt eine Liste aller Schulungen wieder, die zur Suchanfrage passen</returns>
        public ActionResult SucheJahr(SucheViewModel sucheViewModel)
        {
            try
            {
                if (sucheViewModel.Jahr == null) return RedirectToAction("Suche");

                //TODO: beim jahr nur auf den ersten Start-Termin achten?
                List<Schulung> Schulungen = _schulungRepository.GetForArchivSortByDate()
                    .Where(s => s.Termine.First().Start.Year.ToString().Equals(sucheViewModel.Jahr))
                    .ToList();

                SearchResultViewModel srvw = new SearchResultViewModel
                {
                    schulung = Schulungen,
                    suchAnfrage = "Jahr: " + sucheViewModel.Jahr,
                    titel = "Jahr"
                };

                return View("SuchErgebnis", srvw);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#505";
                e = new Exception("Fehler beim verarbeiten des Inputs " + code, e);
                return View("Error", e);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Verwaltung")]
        public ActionResult SucheTeilnehmer()
        {
            // uses vue frontend
            return View();
        }

        // seperate Klasse zum Vergleichen, da IDs nicht beachtet werden
        private AWMCComparer awmcComparer = new AWMCComparer();

        private class AWMCComparer : IEqualityComparer<AnmeldungRepository.AnmeldungWithMatchCount>
        {
            public bool Equals(AnmeldungRepository.AnmeldungWithMatchCount a, AnmeldungRepository.AnmeldungWithMatchCount b)
            {
                return (a.Email == b.Email)
                        && (a.matchCount == b.matchCount)
                        && (a.Nachname == b.Nachname)
                        && (a.Nummer == b.Nummer)
                        && (a.Status == b.Status)
                        && (a.Vorname == b.Vorname);
            }

            public int GetHashCode(AnmeldungRepository.AnmeldungWithMatchCount obj)
            {
                unchecked
                {
                    int result = 17;
                    result = (result * 23) + obj.Email.GetHashCode();
                    result = (result * 23) + obj.matchCount.GetHashCode();
                    result = (result * 23) + obj.Nachname.GetHashCode();
                    result = (result * 23) + obj.Nummer.GetHashCode();
                    result = (result * 23) + obj.Status.GetHashCode();
                    result = (result * 23) + obj.Vorname.GetHashCode();
                    return result;
                }
            }
        }
    }
}
