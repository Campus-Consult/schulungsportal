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
            return View();
        }


        [HttpPost]
        public ActionResult SucheTeilnehmer(TeilnehmerSucheViewModel teilnehmerSucheViewModel)
        {
            try
            {
                teilnehmerSucheViewModel.CleanNulls();
                // Hole alle Daten aus der DB, die irgendwas mit der Suche gemeinsam haben
                List<TeilnehmerSucheResultViewModel> vms = _anmeldungRepository.SearchAnmeldungenWithMatchCount(teilnehmerSucheViewModel.VornameTeilnehmer, teilnehmerSucheViewModel.NachnameTeilnehmer, teilnehmerSucheViewModel.EmailTeilnehmer, teilnehmerSucheViewModel.TelefonTeinehmer)
                    // gruppiere und Zähle die einzelnen Vorkommen für bessere Anzeige
                    .GroupBy(m => m, awmcComparer)
                    .Select(m => new TeilnehmerSucheResultViewModel
                    {
                        EmailTeilnehmer = m.Key.Email,
                        matchCount = m.Key.matchCount,
                        NachnameTeilnehmer = m.Key.Nachname,
                        VornameTeilnehmer = m.Key.Vorname,
                        number = m.Count(),
                        AnmeldungIDs = String.Join(",", m.Select(a => a.anmeldungId.ToString())),
                        TelefonTeilnehmer = m.Key.Nummer,
                    })
                    .ToList();
                // Sortieren für schönere Anzeige
                vms.Sort(delegate (TeilnehmerSucheResultViewModel x, TeilnehmerSucheResultViewModel y)
                {
                    int compare = x.matchCount.CompareTo(y.matchCount);
                    if (compare != 0) return -compare;
                    compare = x.number.CompareTo(y.number);
                    if (compare != 0) return -compare;
                    compare = x.VornameTeilnehmer.CompareTo(y.VornameTeilnehmer);
                    if (compare != 0) return compare;
                    compare = x.NachnameTeilnehmer.CompareTo(y.NachnameTeilnehmer);
                    if (compare != 0) return compare;
                    compare = x.EmailTeilnehmer.CompareTo(y.EmailTeilnehmer);
                    if (compare != 0) return compare;
                    compare = x.TelefonTeilnehmer.CompareTo(y.TelefonTeilnehmer);
                    if (compare != 0) return compare;
                    return 0;
                });
                return View("SucheTeilnehmerResult", vms);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#506";
                e = new Exception("Fehler beim Erstellen der View " + code, e);
                return View("Error", e);
            }
        }

        [HttpPost]
        public ActionResult TeilnehmerSelected(List<TeilnehmerSucheResultViewModel> models, string DeleteButton, string RecordButton)
        {
            try
            {
                if (DeleteButton != null)
                {
                    models = models.Where(m => m.Checked).ToList();
                    // https://stackoverflow.com/a/47170028
                    ModelState.Clear();
                    return View("TeilnehmerAnonymisieren", models);
                }
                if (RecordButton != null)
                {
                    IEnumerable<int> anmeldungIDs = models
                        .Where(m => m.Checked)
                        .SelectMany(m => m.AnmeldungIDs.Split(',').Select(s => int.Parse(s)));
                    SearchResultViewModel srvm = new SearchResultViewModel();
                    if (anmeldungIDs.Count() > 0)
                    {
                        srvm.schulung = _schulungRepository.GetSchulungenForAnmeldungIDs(anmeldungIDs)
                            .Where(s => s.IsGeprüft)
                            .ToList();
                        // geh mal davon aus, dass der beste Treffer mit Name übereinstimmt
                        srvm.titel = models.First().VornameTeilnehmer + " " + models.First().NachnameTeilnehmer;
                    }
                    else
                    {
                        srvm.schulung = new List<Schulung>(0);
                    }
                    return View("TeilnehmerHistorie", srvm);
                }
                return RedirectToAction("SucheTeilnehmer");
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#507";
                e = new Exception("Fehler beim Erstellen der View " + code, e);
                return View("Error", e);
            }
        }

        // Dies ist für die Bestätigung auf der vorherigen View
        [HttpPost]
        public ActionResult TeilnehmerAnonymisieren(List<TeilnehmerSucheResultViewModel> models)
        {
            try
            {
                IEnumerable<int> deleteIDs = models
                        .SelectMany(m => m.AnmeldungIDs.Split(',').Select(s => int.Parse(s)));
                logger.Warn("Deleting :" + String.Join(";", deleteIDs));
                _anmeldungRepository.BulkAnonymizeIDs(
                    deleteIDs
                );
                return RedirectToAction("SucheTeilnehmer");
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#508";
                e = new Exception("Fehler beim Erstellen der View " + code, e);
                return View("Error", e);
            }
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
