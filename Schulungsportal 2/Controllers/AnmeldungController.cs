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
    /// Der AnmeldungController beinhaltet alle Aktionen zur Verwaltung von Anmeldungen
    /// </summary>
    public class AnmeldungController: Controller
    {
        private ApplicationDbContext _context;
        private SchulungRepository _schulungRepository;
        private AnmeldungRepository _anmeldungRepository;

        private ISchulungsportalEmailSender emailSender;
        
        // logger
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // <summary>
        /// AnmeldungController Konstruktor legt die Repositories an.
        /// </summary>
        public AnmeldungController(ApplicationDbContext context, ISchulungsportalEmailSender emailSender, IMapper mapper)
        {
            _schulungRepository = new SchulungRepository(context);
            _anmeldungRepository = new AnmeldungRepository(context, mapper);
            _context = context;

            this.emailSender = emailSender;
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn im Browser die ../Anmeldung/Anmeldung Seite aufgerufen wird. Sie gibt dem Framework die Anmeldung-View und ein AnmeldungViewModel zurück, welches eine Liste von CC-Stati für ein Dropdown-Menü und eine Liste von Checkboxen zur Schulungsauswahl enthält.
        /// </summary>
        /// <returns> die View für ../Anmeldung/Anmeldung </returns>
        public ActionResult Anmeldung()
        { 
            try
            {
                // alle Schulungen aus der Datebank abrufen
                List<Schulung> Schulungen = _schulungRepository.GetForRegSortByDate().ToList();
                
                // neues Viewmodel erstellen   
                AnmeldungViewModel anmeldungViewModel = new AnmeldungViewModel(Schulungen);

                //die zu verwendende View wird zurückgegeben mit dem gefüllten anmeldungViewModel
                return View("Anmeldung", anmeldungViewModel);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#301";
                e = new Exception("Fehler beim Erstellen der View " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn die Anmeldung/Anmeldung-Eingabemaske ausgefüllt zurückgesendet wird. Sollte die Anmeldung/Anmeldung-Eingabemaske nicht richtig ausgefüllt worden sein, wird die Anmeldung-View zurückgegeben, ansonsten werden für alle ausgewählten Checkboxen eine neue Anmeldung zu einer Schulung auf Basis des übertragenen Inhalts angelegt und eine Bestätigungsseite zurückgegeben.
        /// </summary>
        /// <param name="newSchulung"> Inhalt einer ausgefüllten Anmeldung/Anmeldung-Eingabemaske in Form eines AnmeldungViewModel-Objekts </param>
        /// <returns> Bestaetigungs-View oder erneut die View für ../Anmeldung/Anmeldung </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Anmeldung(AnmeldungViewModel newAnmeldung)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // failsafe für den Status, wenn absichtlich falsch übergeben
                    if (!AnmeldungViewModel.AllStati.Contains(newAnmeldung.Status))
                    {
                        return StatusCode(400, "invalid status");
                    }
                    DateTime now = DateTime.Now;
                    List<Schulung> angemeldeteSchulungen = new List<Schulung>();
                    foreach (SchulungsCheckBox checkbox in newAnmeldung.SchulungsCheckboxen)
                    {
                        if (checkbox.Checked)
                        {
                            Anmeldung anmeldung = newAnmeldung.ToAnmeldung();
                            anmeldung.SchulungGuid = checkbox.Guid;
                            Schulung schulung = _schulungRepository.GetById(anmeldung.SchulungGuid);
                            // nur erreicht, wenn absichtlich invalide Id übergeben
                            if (schulung == null)
                            {
                                return StatusCode(404, "Schulung existiert nicht");
                            }
                            // Termine aus Datenbank laden
                            if (schulung.Termine.Count == 0) { }
                            // Check ob die Schulung immer noch offen ist, sonst ignorieren
                            if (!schulung.IsAbgesagt && schulung.Anmeldefrist > now) {
                                if (_anmeldungRepository.AnmeldungAlreadyExist(anmeldung))
                                {
                                    schulung.Check = true;
                                    angemeldeteSchulungen.Add(schulung);
                                }
                                else
                                {
                                    angemeldeteSchulungen.Add(schulung);
                                    _anmeldungRepository.Add(anmeldung);
                                    MailingHelper.GenerateAndSendBestätigungsMail(newAnmeldung.ToAnmeldung(), schulung, getVorstand(), emailSender);
                                }
                            }
                        }
                    }

                    return View("Bestätigung", angemeldeteSchulungen); //Weiterleitung auf Bestaetigungs-View
                }
                AnmeldungViewModel anmeldungViewModel = new AnmeldungViewModel((List<Schulung>)_schulungRepository.GetForRegSortByDate());
                newAnmeldung.Schulungen = anmeldungViewModel.Schulungen;
                newAnmeldung.Stati = anmeldungViewModel.Stati;

                return View("Anmeldung", newAnmeldung);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#302";
                e = new Exception("Fehler beim Verarbeiten des Inputs " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn eine Anmeldung geloescht werden soll. Der Nutzer wird anschließend zu einer Bestaetigungsseite weitergeleitet, auf der das Loeschen bestaetigt werden muss.
        /// </summary>
        /// <param name="anmeldungId"> Die anmeldungId der zu löschenden Anmeldung</param>
        /// <returns>Weiterleitung auf .../Anmeldung/Loeschen/{anmeldungId} </returns>
        [HttpGet, ActionName("Loeschen")]
        [Authorize(Roles = "Verwaltung")]
        [Route("Anmeldung/Loeschen/{anmeldungId}")]
        public ActionResult Loeschen(int anmeldungId)
        {
            try
            {
                if (anmeldungId == 0)
                {
                    return StatusCode(400);
                }
                Anmeldung anmeldung = _anmeldungRepository.GetById(anmeldungId);
                if (anmeldung == null)
                {
                    return NotFound();
                }
                return View("Loeschen2", anmeldung);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#303";
                e = new Exception("Fehler beim Erstellen der View " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird ausgeführt, wenn das Löschen bestätigt wurde. Die Anmeldung wird aus der Datenbank gelöscht und man wird zu der Teilnehmerliste weitergeleitet.
        /// </summary>
        /// <param name="anmeldung">Die zu löschende Anmeldung</param>
        /// <returns>Weiterleitung zu .../Anmeldung/Teilnehmerliste/{schulungGUID} </returns>
        [HttpPost, ActionName("Loeschen")]
        [Route("Anmeldung/Loeschen/{anmeldungId}")]
        [ValidateAntiForgeryToken]
        public ActionResult LoeschenBestaetigt(int anmeldungId)
        {
            try
            {
                Anmeldung anmeldung = _anmeldungRepository.GetById(anmeldungId);
                string guid = anmeldung.SchulungGuid;
                _anmeldungRepository.Delete(anmeldung);
                return Redirect("/Schulung/Teilnehmerliste/" + guid);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#304";
                e = new Exception("Fehler beim Verarbeiten des Inputs " + code, e);
                return View("Error", e);
            }
        }


        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn im Browser die ../Anmeldung/Nachtragen/{schulungGuid} Seite aufgerufen wird. Sie gibt dem Framework die Nachtragen-View und ein AnmeldungNachtragenViewModel zurück, welches die Einagbemaske mit einer Liste von CC-Stati für ein Dropdown-Menü enthält.
        /// </summary>
        /// <param name="schulungGuid"> die schulungGuid der Schulung zu der die Anmeldung gamacht wird</param>
        /// <returns> Nachtragen-View </returns>
        [HttpGet, ActionName("Nachtragen")]
        [Authorize(Roles = "Verwaltung")]
        [Route("Anmeldung/Nachtragen/{schulungGuid}")]
        public ActionResult Nachtragen(string schulungGuid)
        {
            try
            {
                if (schulungGuid == null)
                {
                    return StatusCode(400);
                }
                Schulung schulung = _schulungRepository.GetById(schulungGuid);
                if (schulung == null)
                {
                    return StatusCode(404, "Schulung mit angegebener ID nicht gefunden!");
                }
                AnmeldungNachtragenViewModel anmeldung = new AnmeldungNachtragenViewModel();
                anmeldung.SchulungGuid = schulungGuid;
                anmeldung.SchulungTitel = schulung.Titel;

                return View("Nachtragen", anmeldung);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#305";
                e = new Exception("Fehler beim Erstellen der View " + code, e);
                return View("Error", e);
            }
        }

        /// <summary>
        /// Diese Methode wird vom Framework aufgerufen, wenn die Anmeldung/Nachtragen-Eingabemaske ausgefüllt zurückgesendet wird. Sollte die Anmeldung/Nachtragen-Eingabemaske nicht richtig ausgefüllt worden sein, wird die Nachtragen-View zurückgegeben, ansonsten wird die neue Anmeldung angelegt und auf ../Schulung/Teilnehmerliste/{schulungGuid} umgeleitet.
        /// </summary>
        /// <param name="anmeldungViewModel"> Die ausgefüllte Eingabemaske </param>
        /// <returns> Weiterleitung zu ../Schulung/Teilnehmerliste/{schulungGuid} </returns>
        [HttpPost, ActionName("Nachtragen")]
        [ValidateAntiForgeryToken]
        [Route("Anmeldung/Nachtragen/{schulungGuid}")]
        public ActionResult Nachtragen(AnmeldungNachtragenViewModel anmeldungViewModel, String schulungGuid)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Anmeldung anmeldung = anmeldungViewModel.ToAnmeldung();
                    if (anmeldung.SchulungGuid != schulungGuid)
                    {
                        throw new Exception("Guid missmatch");
                    }
                    Schulung schulung = _schulungRepository.GetById(schulungGuid);
                    if (schulung == null)
                    {
                        return StatusCode(404, "Schulung mit angegebener ID nicht gefunden!");
                    }
                    _anmeldungRepository.Add(anmeldung);
                    // sende Mail falls mindestens ein Start Termin nach jetzt is, also noch was von der Schulung
                    // in Zukunft kommt
                    if (DateTime.Now < schulung.Termine.Min(x => x.Start))
                    {
                        MailingHelper.GenerateAndSendBestätigungsMail(anmeldung, _schulungRepository.GetById(anmeldung.SchulungGuid), getVorstand(), emailSender);
                    }
                    return Redirect("/Schulung/Teilnehmerliste/" + anmeldung.SchulungGuid);
                }
                return View("Nachtragen", anmeldungViewModel);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#306";
                e = new Exception("Fehler beim Verarbeiten des Inputs " + code, e);
                return View("Error", e);
            }
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
            }
            else
            {
                return impressum.Vorstand;
            }
        }
    }
}
