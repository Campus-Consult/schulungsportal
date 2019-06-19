using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schulungsportal_2.Data;
using Schulungsportal_2.Models;
using Schulungsportal_2.Models.Anmeldungen;
using Schulungsportal_2.Models.Schulungen;
using Schulungsportal_2.Services;
using Schulungsportal_2.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Schulungsportal_2.Controllers
{
    /// <summary>
    /// Der SchulungController beinhaltet alle Aktionen zur  Verwaltung und Anzeige von Schulungen oder deren Inhalten
    /// </summary>
    public class SchulungController : Controller
    {
        private SchulungRepository _schulungRepository;
        private AnmeldungRepository _anmeldungRepository;
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private IEmailSender emailSender;

        // logger
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// SchulungController Konstruktor legt Repositories für Datenzugriff an.
        /// </summary>
        public SchulungController(ApplicationDbContext context, IEmailSender emailSender, IMapper mapper)
        {
            _schulungRepository = new SchulungRepository(context);
            _anmeldungRepository = new AnmeldungRepository(context, mapper);
            _context = context;
            _mapper = mapper;
            this.emailSender = emailSender;
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn im Browser die ../Schulung/Uebersicht Seite aufgerufen wird. Sie gibt dem Framework die Uebersicht-View mit der Liste aller Schulungen aus der Datenbank zurück, aus der dieses die HTML-Datei erzeugt, um sie dem Browser zusenden.
        /// </summary>
        /// <returns> die View für ../Schulungen/Uebersicht </returns>
        [Authorize(Roles = "Verwaltung")]
        public ActionResult Uebersicht()
        {
            try
            {
                return View("Uebersicht", _schulungRepository.GetForOverviewSortByDate().ToList());
            }
            catch(Exception e)
            {
                logger.Error(e);
                string code = "#201";
                e = new Exception("Fehler beim Erstellen der View " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn im Browser die ../Schulung/Anlegen Seite aufgerufen wird. Sie gibt dem Framework die Anlegen-View zurück, aus der dieses die HTML-Datei erzeugt, um sie dem Browser zusenden.
        /// </summary>
        /// <returns> die View für ../Schulungen/Anlegen </returns>
        [Authorize(Roles = "Verwaltung")]
        public ActionResult Anlegen()
        {
            try
            {
                List<string> orgs = _schulungRepository.GetPreviousOrganizers();
                ViewBag.OrganisatorenDatalist = orgs;
                return View("Anlegen", new SchulungCreateViewModel());
            }
            catch(Exception e)
            {
                logger.Error(e);
                string code = "#202";
                e = new Exception("Fehler beim Erstellen der View " + code, e);
                return View("Error", e);
            }
        }
        
        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn die Schulungen/Anlegen-Eingabemaske ausgefüllt zurückgesendet wird. Sollte die Schulungen/Anlegen-Eingabemaske nicht richtig ausgefüllt worden sein, wird die Anlegen-View zurückgegeben, ansonsten wird eine neue Schulung auf Basis des übergebenen Inhalts angelegt und auf ../Schulung/Uebersicht umgeleitet.
        /// </summary>
        /// <param name="newSchulung"> Inhalt einer ausgefüllten Schulungen/Anlegen-Eingabemaske in Form eines SchulungViewModel-Objekts </param>
        /// <returns> Umleitung auf ../Schulung/Uebersicht oder erneut die View für ../Schulungen/Anlegen </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Anlegen(SchulungCreateViewModel newSchulung)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Schulung schulung = _schulungRepository.Add(newSchulung.ToSchulung());
                    // stellt für alle Termine sicher, dass die Reihenfolge Anmeldefrist<Start<Ende eingehalten ist
                    if (schulung.Termine.Count > 0 && schulung.Termine.All(x => x.Start > schulung.Anmeldefrist && x.End > x.Start))
                    {
                        var rootUrl = string.Format("https://{0}", Request.Host);
                        MailingHelper.GenerateAndSendAnlegeMail(schulung, rootUrl, getVorstand(), emailSender);
                        return RedirectToAction("Uebersicht");
                    }
                    if (schulung.Termine.Count > 0)
                    {
                        ViewBag.errorMessage = "Anmeldefrist vor Starttermin vor Endtermin bitte!";
                    }
                    else
                    {
                        ViewBag.errorMessage = "Mindestens ein Termin muss vorhanden sein!";
                    }
                }
                ViewBag.OrganisatorenDatalist = _schulungRepository.GetPreviousOrganizers();
                return View("Anlegen", newSchulung);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#203";
                e = new Exception("Fehler beim Verarbeiten des Inputs " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn im Browser die ../Schulung/Teilnehmerliste/{schulungGuid} Seite aufgerufen wird. Sie gibt dem Framework die Teilnehmerliste-View und ein TeilnehmerlisteViewModel mit der zur schulungGuid gehörenden Schulung und deren Anmeldungen zurück, aus der dieses die HTML-Datei erzeugt, um sie dem Browser zusenden.
        /// </summary>
        /// <param name="schulungGuid"> die schulungGuid der Schulung, zu der die Teilnehmerliste angezeigt werden soll </param>
        /// <returns> die View für ../Schulung/Teilnehmerliste/{schulungGuid} </returns>
        [Authorize(Roles = "Verwaltung")]
        [Route("Schulung/Teilnehmerliste/{schulungGuid}")]
        public ActionResult Teilnehmerliste(string schulungGuid)
        {
            try
            {
                if (schulungGuid == null)
                {
                    return new StatusCodeResult(400);
                }
                Schulung schulung = _schulungRepository.GetById(schulungGuid);
                if (schulung == null)
                {
                    return NotFound("Schulung mit angegebener ID nicht gefunden!");
                }
                TeilnehmerlisteViewModel tl = new TeilnehmerlisteViewModel();
                tl.Schulung = schulung;
                tl.Anmeldungen = _anmeldungRepository.GetBySchulungGuid(schulungGuid);
                tl.RundmailLink = ";%20" + tl.Schulung.EmailDozent + "&subject=Informationen%20zur%20" + tl.Schulung.Titel + "&body=Hallo%20Teilnehmer%20der%20" + tl.Schulung.Titel + ",%0D%0A%0D%0Ahier%20steht%20die%20Nachricht.";
                tl.RundmailLink = String.Join(";%20", tl.Anmeldungen
                    // filtert anonymisierte raus
                    .Where(a => a.Email.Contains("@"))
                    .Select(a => a.Email)) + tl.RundmailLink;
                tl.RundmailLink = tl.RundmailLink.Insert(0, "mailto:?bcc=");

                return View("Teilnehmerliste", tl);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#204";
                e = new Exception("Fehler beim Erstellen der View " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn im Browser die ../Schulung/Teilnehmer/{accessToken} Seite aufgerufen wird. Sie gibt dem Framework die Teilnehmerliste-View und ein TeilnehmerlisteViewModel mit der zur schulungGuid gehörenden Schulung und deren Anmeldungen zurück, aus der dieses die HTML-Datei erzeugt, um sie dem Browser zusenden.
        /// </summary>
        /// <param name="accessToken"> die schulungGuid der Schulung, zu der die Teilnehmerliste angezeigt werden soll </param>
        /// <returns> die View für ../Schulung/Teilnehmerliste/{accessToken} </returns>
        [Route("Schulung/Teilnehmer/{accessToken}")]
        public ActionResult Teilnehmer(string accessToken)
        {
            try
            {
                if (accessToken == null)
                {
                    return new StatusCodeResult(400);
                }
                TeilnehmerlisteViewModel tl = new TeilnehmerlisteViewModel();
                tl.Schulung = _schulungRepository.GetByAccessToken(accessToken);
                if (tl.Schulung == null)
                {
                    return NotFound("Schulung nicht gefunden!");
                }
                tl.Anmeldungen = _anmeldungRepository.GetBySchulungGuid(tl.Schulung.SchulungGUID);
                tl.RundmailLink = ";%20" + tl.Schulung.EmailDozent + "&subject=Informationen%20zur%20" + tl.Schulung.Titel + "&body=Hallo%20Teilnehmer%20der%20" + tl.Schulung.Titel + ",%0D%0A%0D%0Ahier%20steht%20die%20Nachricht.";
                tl.RundmailLink = String.Join(";%20", tl.Anmeldungen
                    // filtert anonymisierte raus
                    .Where(a => a.Email.Contains("@"))
                    .Select(a => a.Email)) + tl.RundmailLink;
                tl.RundmailLink = tl.RundmailLink.Insert(0, "mailto:?bcc=");

                return View("Teilnehmer", tl);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#204";
                e = new Exception("Fehler beim Erstellen der View " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn im Browser die ../Schulung/Details/{schulungGuid} Seite aufgerufen wird. Sie gibt dem Framework die Details-View und ein Schulung-Model zurück, aus der dieses die HTML-Datei erzeugt, um sie dem Browser zusenden.
        /// </summary>
        /// <param name="schulungGuid"> die schulungGuid der Schulung, zu der die Details angezeigt werden sollen </param>
        /// <returns> die View für ../Schulung/Details/{schulungGuid} </returns>
        [Authorize(Roles = "Verwaltung")]
        [Route("Schulung/Details/{schulungGuid}")]
        public ActionResult Details(string schulungGuid)
        {
            try
            {
                if (schulungGuid == null)
                {
                    return new StatusCodeResult(400);
                }
                Schulung schulung = _schulungRepository.GetById(schulungGuid);
                if (schulung == null)
                {
                    return NotFound("Schulung mit angegebener ID nicht gefunden!");
                } else
                {
                    return View("Details", schulung);
                }
            }
            catch(Exception e)
            {
                logger.Error(e);
                string code = "#205";
                e = new Exception("Fehler beim Erstellen der View " + code, e);
                return View("Error", e);
            }
            
        }


        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn im Browser die ../Schulung/Loeschen/{schulungGuid} Seite aufgerufen wird. Sie gibt dem Framework die Loeschen-View und ein Schulung-Model zurück, aus der dieses die HTML-Datei erzeugt, um sie dem Browser zusenden.
        /// </summary>
        /// <param name="schulungGuid"> die schulungGuid der Schulung, die gelöscht werden soll </param>
        /// <returns> die View für ../Schulung/Loeschen/{schulungGuid} </returns>
        [HttpGet, ActionName("Loeschen")]
        [Authorize(Roles = "Verwaltung")]
        [Route("Schulung/Loeschen/{schulungGuid}")]
        public ActionResult Loeschen(string schulungGuid)
        {
            try
            {
                if (schulungGuid == null)
                {
                    return new StatusCodeResult(400);
                }
                Schulung schulung = _schulungRepository.GetById(schulungGuid);
                if (schulung == null)
                {
                    return NotFound("Schulung mit angegebener ID nicht gefunden!");
                }
                if (!schulung.IsAbgesagt) return RedirectToAction("Uebersicht");
                return View("Loeschen", schulung);
            }
            catch(Exception e)
            {
                logger.Error(e);
                string code = "#206";
                e = new Exception("Fehler beim Erstellen der View " + code, e);
                return View("Error", e);
            }            
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn das Löschen auf der ../Schulung/Loeschen/{schulungGuid} Seite bestätigt wird. Ihr wird die Schulung übergeben, die gelöscht werden soll. Anschließend wird zur Schulungsübersicht weitergeleitet.
        /// </summary>
        /// <param name="schulung"> Schulungs-Objekt mit der SchulungGuid der Schulung, die gelöscht werden soll</param>
        /// <returns> Weiterleitung zu ../Schulung/Uebersicht </returns>
        [HttpPost, ActionName("Loeschen")]
        [ValidateAntiForgeryToken]
        [Route("Schulung/Loeschen/{schulungGuid}")]
        public ActionResult LoeschenBestaetigt(Schulung schulung, string schulungGuid)
        {
            try
            {
                schulung = _schulungRepository.GetById(schulung.SchulungGUID);
                if (schulung == null)
                {
                    return NotFound("Schulung mit angegebener ID nicht gefunden!");
                }
                _schulungRepository.Delete(schulung);

                return RedirectToAction("Uebersicht");
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#207";
                e = new Exception("Fehler beim Verarbeiten des Inputs " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn der Schulungsbeauftragte auf "Archiv" klickt. Dadurch wird man zum Archiv weitergeleitet.
        /// </summary>
        /// <returns>Weiterleitung zu .../Schulung/Archiv </returns>
        [Authorize(Roles = "Verwaltung")]
        public ActionResult Archiv()
        {
            try {
                ArchivViewModel archivViewModel = new ArchivViewModel();
                archivViewModel.schulung = _schulungRepository.GetForArchivSortByDate();
                return View("Archiv", archivViewModel);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#208";
                e = new Exception("Fehler beim Erstellen der View " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn im Browser die ../Schulung/Absagen/{schulungGuid} Seite aufgerufen wird. Sie gibt dem Framework die Absagen-View und ein Schulung-Model zurück, aus der dieses die HTML-Datei erzeugt, um sie dem Browser zusenden.
        /// </summary>
        /// <param name="schulungGuid"> die schulungGuid der Schulung, die abgesagt werden soll </param>
        /// <returns> die View für ../Schulung/Absagen/{schulungGuid} </returns>
        [HttpGet, ActionName("Absagen")]
        [Authorize(Roles = "Verwaltung")]
        [Route("Schulung/Absagen/{schulungGuid}")]
        public ActionResult Absagen(string schulungGuid)
        {
            try
            {
                if (schulungGuid == null)
                {
                    return new StatusCodeResult(400);
                }
                Schulung schulung = _schulungRepository.GetById(schulungGuid);
                if (schulung == null)
                {
                    return NotFound("Schulung mit angegebener ID nicht gefunden!");
                }
                if (schulung.IsAbgesagt || schulung.IsGeprüft) return RedirectToAction("Uebersicht");
                return View("Absagen", schulung);
            }
            catch(Exception e)
            {
                logger.Error(e);
                string code = "#209";
                e = new Exception("Fehler beim Erstellen der View " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn das Absagen auf der ../Schulung/Absagen/{schulungGuid} Seite bestätigt wird. Ihr wird die Schulung übergeben, die abgesagt werden soll. Es werden an alle Teilnehmer eine Mail gesendet. Anschließend wird zur Schulungsübersicht weitergeleitet.
        /// </summary>
        /// <param name="schulung"> Schulungs-Objekt mit der SchulungGuid der Schulung, die abgesagt werden soll</param>
        /// <returns> Weiterleitung zu ../Schulung/Uebersicht </returns>
        [HttpPost, ActionName("Absagen")]
        [ValidateAntiForgeryToken]
        [Route("Schulung/Absagen/{schulungGuid}")]
        public ActionResult AbsagenBestaetigt(Schulung schulung, string schulungGuid)
        {
            try
            {
                schulung = _schulungRepository.GetById(schulung.SchulungGUID);
                if (schulung == null)
                {
                    return NotFound("Schulung mit angegebener ID nicht gefunden!");
                }
                schulung.IsAbgesagt = true;
                _schulungRepository.Update(schulung);

                IEnumerable<Anmeldung> Anmeldungen = _anmeldungRepository.GetBySchulungGuid(schulung.SchulungGUID);

                string vorstand = getVorstand();

                foreach(Anmeldung anmeldung in Anmeldungen)
                {
                    MailingHelper.GenerateAndSendAbsageMail(anmeldung, schulung, vorstand, emailSender);
                }

                return RedirectToAction("Uebersicht");
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#210";
                e = new Exception("Fehler beim Verarbeiten des Inputs " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn im Browser die ../Schulung/Bearbeiten/{SchulungGUID} Seite aufgerufen wird. Sie gibt dem Framework die Bearbeiten-View mit dem SchulungViewModel auf Basis der zu bearbeitenden Schulung zurück, aus der dieses die HTML-Datei erzeugt, um sie dem Browser zusenden.
        /// </summary>
        /// <returns> die View für ../Schulungen/Bearbeiten/{SchulungGUID} </returns>
        [HttpGet, ActionName("Bearbeiten")]
        [Authorize(Roles = "Verwaltung")]
        [Route("Schulung/Bearbeiten/{SchulungGUID}")]
        public ActionResult Bearbeiten(string SchulungGuid)
        {
            try
            {
                if (SchulungGuid == null)
                {
                    return new StatusCodeResult(400);
                }
                SchulungCreateViewModel schulungVM = _schulungRepository.GetById(SchulungGuid).ToSchulungCreateViewModel(_mapper);
                if (schulungVM == null)
                {
                    return NotFound("Schulung mit angegebener ID nicht gefunden!");
                }
                // Add suggestions for organizers
                List<string> orgs = _schulungRepository.GetPreviousOrganizers();
                ViewBag.OrganisatorenDatalist = orgs;
                return View("Bearbeiten", schulungVM);
            }
            catch(Exception e)
            {
                logger.Error(e);
                string code = "#211";
                e = new Exception("Fehler beim Erstellen der View " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn die Schulungen/Bearbeiten-Eingabemaske ausgefüllt zurückgesendet wird. Sollte die Schulungen/Bearbeiten-Eingabemaske nicht richtig ausgefüllt worden sein, wird die Bearbeiten-View zurückgegeben, ansonsten wird die änderung zu der exisiterenden Schulung auf Basis des übergebenen Inhalts geändert und auf ../Schulung/Uebersicht umgeleitet.
        /// </summary>
        /// <param name="newSchulung"> Inhalt einer ausgefüllten Schulungen/Bearbeiten-Eingabemaske in Form eines SchulungViewModel-Objekts </param>
        /// <returns> Umleitung auf ../Schulung/Uebersicht oder erneut die View für ../Schulungen/Bearbeiten </returns>
        [HttpPost, ActionName("Bearbeiten")]
        [ValidateAntiForgeryToken]
        [Route("Schulung/Bearbeiten/{SchulungGuid}")]
        public ActionResult BearbeitenBestaetigt(SchulungCreateViewModel schulungVM, string SchulungGuid)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Schulung schulung = schulungVM.ToSchulung();
                    // check ob zeiten passen
                    if (schulung.Termine.Count > 0 && schulung.Termine.All(x => x.Start > schulung.Anmeldefrist && x.End > x.Start))
                    {
                        _schulungRepository.Update(schulung);
                        return RedirectToAction("Uebersicht"); //Nach Abschluss der Aktion Weiterleitung zur Ubersicht-View
                    }
                    if (schulung.Termine.Count > 0)
                    {
                        ViewBag.errorMessage = "Anmeldefrist vor Starttermin vor Endtermin bitte!";
                    }
                    else
                    {
                        ViewBag.errorMessage = "Mindestens ein Termin muss vorhanden sein!";
                    }
                }
                // Fehlermeldung und view bag wieder aufbereiten
                List<string> orgs = _schulungRepository.GetPreviousOrganizers();
                ViewBag.OrganisatorenDatalist = orgs;
                return View("Bearbeiten", schulungVM);
            }
            catch(Exception e)
            {
                logger.Error(e);
                string code = "#212";
                e = new Exception("Fehler beim Verarbeiten des Inputs " + code, e);
                return View("Error", e);
            }
        }



        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn im Browser die ../Schulung/Geprüft/{schulungGuid} Seite aufgerufen wird. Sie gibt dem Framework die Geprueft-View und ein Schulung-Model zurück, aus der dieses die HTML-Datei erzeugt, um sie dem Browser zusenden.
        /// </summary>
        /// <param name="schulungGuid"> die schulungGuid der Schulung, die "geprüft" gesetzt werden soll </param>
        /// <returns> die View für ../Schulung/Geprueft/{schulungGuid} </returns>
        [HttpGet, ActionName("Geprueft")]
        [Authorize(Roles = "Verwaltung")]
        [Route("Schulung/Geprueft/{schulungGuid}")]
        public ActionResult Geprueft(string schulungGuid)
        {
            try
            {
            if (schulungGuid == null)
            {
                return new StatusCodeResult(400);
            }
            Schulung schulung = _schulungRepository.GetById(schulungGuid);
            if (schulung == null)
                {
                    return NotFound("Schulung mit angegebener ID nicht gefunden!");
                }
            if (schulung.IsAbgesagt) return RedirectToAction("Uebersicht");
            if (schulung.IsGeprüft) return RedirectToAction("Uebersicht");
            return View("Geprueft", schulung);
        }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#213";
                e = new Exception("Fehler beim Erstellen der View " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn die Abfrage zu Prüfung auf der ../Schulung/Geprueft/{schulungGuid} Seite bestätigt wird. Ihr wird die Schulung übergeben, die geprüft wurde. Anschließend wird zur Schulungsübersicht weitergeleitet.
        /// </summary>
        /// <param name="schulung"> Schulungs-Objekt mit der SchulungGuid der Schulung, die geprüft wurde</param>
        /// <returns> Weiterleitung zu ../Schulung/Uebersicht </returns>
        [HttpPost, ActionName("Geprueft")]
        [ValidateAntiForgeryToken]
        [Route("Schulung/Geprueft/{schulungGuid}")]
        public ActionResult GeprueftBestaetigt(Schulung schulung, string schulungGuid)
        {
            try
            {
                schulung = _schulungRepository.GetById(schulung.SchulungGUID);
                if (schulung == null)
                {
                    return NotFound("Schulung mit angegebener ID nicht gefunden!");
                }
                schulung.IsGeprüft = true;
                _schulungRepository.Update(schulung);

                return RedirectToAction("Uebersicht");
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#214";
                e = new Exception("Fehler beim Verarbeiten des Inputs " + code, e);
                return View("Error", e);
            }
        }
        
        /// <summary>
        /// Methode, um das senden des Newsletters als Cronjob zu implementieren
        /// </summary>
        /// <param name="key">Der geheime Schlüssel, sonst wird die Mail nicht gesendet</param>
        /// <returns></returns>
        public ActionResult SendNewsletter(string key)
        {
            if (key != "PbpsNDeKx9cFkoJP5fptQ6jczLqAwXe5")
            {
                return new StatusCodeResult(403);
            }
            // hole alle nächsten Schulungen aus der Datenbank und sende newsletter
            // erstmal deaktiviert
            // MailingHelper.GenerateAndSendSchulungsNewsletter(_schulungRepository.GetForRegSortByDate().ToList(), getVorstand(), emailSender);
            return new StatusCodeResult(200);
        }
        
        /// <summary>
        /// Gibt den aktuellen gespeicherten Vorstand zurück,
        /// </summary>
        /// <returns></returns>
        public string getVorstand()
        {
            var impressum = _context.Impressum.FirstOrDefault();
            if (impressum == null)
            {
                return "";
            } else
            {
                return impressum.Vorstand;
            }
        }

    }
}