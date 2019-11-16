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
using System.Threading.Tasks;

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

                    var rootUrl = Util.getRootUrl(Request);
                    var vorstand = Util.getVorstand(_context);

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
                                    //logger.Info(anmeldung.AccessToken);
                                    MailingHelper.GenerateAndSendBestätigungsMail(anmeldung, schulung, vorstand, rootUrl, emailSender);
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
            // TODO: test, valid id, invalid id
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
            // TODO: test, valid id, invalid id
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
            // TODO: test, valid id, invalid id
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

        [Route("Anmeldung/Selbstmanagement/{accessToken}")]
        public ActionResult Selbstmanagement(String accessToken) {
            // TODO: test, invalid token, already canceled, already started, valid token
            Anmeldung anmeldung = _anmeldungRepository.GetByAccessTokenWithSchulung(accessToken);
            if (anmeldung == null) {
                return NotFound("Die Anmeldung existiert nicht, vielleicht bereits abgemeldet?");
            }
            var cantCancelReason = getCantCancelReason(anmeldung);
            if (cantCancelReason != null) {
                ViewBag.cantCancelReason = cantCancelReason;
                return View("SelbstmanagementError");
            }
            return View("Selbstmanagement", anmeldung);
        }

        [Route("Anmeldung/Selbstabmeldung/{accessToken}")]
        public ActionResult Selbstabmeldung(String accessToken) {
            // TODO: test, valid token, invalid token (other cases already handled above)
            Anmeldung anmeldung = _anmeldungRepository.GetByAccessTokenWithSchulung(accessToken);
            if (anmeldung == null) {
                return NotFound("Die Anmeldung existiert nicht, vielleicht bereits abgemeldet?");
            }
            var cantCancelReason = getCantCancelReason(anmeldung);
            if (cantCancelReason != null) {
                ViewBag.cantCancelReason = cantCancelReason;
                return View("SelbstmanagementError");
            }
            AbmeldungViewModel abmeldung = new AbmeldungViewModel {
                Nachricht = "",
                Schulung = anmeldung.Schulung,
                anmeldungId = anmeldung.anmeldungId,
            };
            return View("Selbstabmeldung", abmeldung);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Anmeldung/Selbstabmeldung/{accessToken}")]
        public ActionResult Selbstabmeldung(AbmeldungViewModel abmeldung, String accessToken) {
            // TODO: test
            Anmeldung anmeldungByToken = _anmeldungRepository.GetByAccessTokenWithSchulung(accessToken);
            if (anmeldungByToken == null) {
                return NotFound("Die Anmeldung existiert nicht, vielleicht bereits abgemeldet?");
            }
            var cantCancelReason = getCantCancelReason(anmeldungByToken);
            if (cantCancelReason != null) {
                ViewBag.cantCancelReason = cantCancelReason;
                return View("SelbstmanagementError");
            }
            // Just a safety check
            if (anmeldungByToken.anmeldungId != abmeldung.anmeldungId) {
                return BadRequest();
            }
            _anmeldungRepository.Delete(anmeldungByToken);
            if (abmeldung.Nachricht == null || abmeldung.Nachricht.Trim() == "") {
                abmeldung.Nachricht = null;
            }
            var vorstand = Util.getVorstand(_context);
            MailingHelper.GenerateAndSendAbmeldungMail(anmeldungByToken, anmeldungByToken.Schulung, vorstand, emailSender);

            // Sende Mail an Dozenten falls Nachricht existiert oder wenn es nach Anmeldefrist ist
            if (abmeldung.Nachricht != null || anmeldungByToken.Schulung.Anmeldefrist < DateTime.Now) {
                MailingHelper.GenerateAndSendAbsageAnSchulungsdozentMail(anmeldungByToken, abmeldung.Nachricht, vorstand, emailSender);
            }
            return View("AbmeldungErfolgreich");
        }

        [Route("Anmeldung/Bearbeiten/{accessToken}")]
        public ActionResult Bearbeiten(String accessToken) {
            // TODO: test
            Anmeldung anmeldungByToken = _anmeldungRepository.GetByAccessTokenWithSchulung(accessToken);
            if (anmeldungByToken == null) {
                return NotFound("Die Anmeldung existiert nicht, vielleicht bereits abgemeldet?");
            }
            var cantCancelReason = getCantCancelReason(anmeldungByToken);
            if (cantCancelReason != null) {
                ViewBag.cantCancelReason = cantCancelReason;
                return View("SelbstmanagementError");
            }
            return View("Bearbeiten", anmeldungByToken);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Anmeldung/Bearbeiten/{accessToken}")]
        public ActionResult Bearbeiten(Anmeldung anmeldung, String accessToken) {
            // TODO: test
            Anmeldung anmeldungByToken = _anmeldungRepository.GetByAccessTokenWithSchulung(accessToken);
            if (anmeldungByToken == null) {
                return NotFound("Die Anmeldung existiert nicht, vielleicht bereits abgemeldet?");
            }
            var cantCancelReason = getCantCancelReason(anmeldungByToken);
            if (cantCancelReason != null) {
                ViewBag.cantCancelReason = cantCancelReason;
                return View("SelbstmanagementError");
            }
            bool shouldSendNewMail = anmeldungByToken.Email != anmeldung.Email;
            // update model
            anmeldungByToken.Vorname = anmeldung.Vorname;
            anmeldungByToken.Nachname = anmeldung.Nachname;
            anmeldungByToken.Email = anmeldung.Email;
            anmeldungByToken.Nummer = anmeldung.Nummer;
            _anmeldungRepository.Update(anmeldungByToken);
            // if the Email changed send the appointment to the new Mail
            if (shouldSendNewMail) {
                MailingHelper.GenerateAndSendBestätigungsMail(anmeldungByToken, anmeldungByToken.Schulung, Util.getVorstand(_context), Util.getRootUrl(Request), emailSender);
            }
            return Redirect("/Anmeldung/Selbstmanagement/"+accessToken);
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
            // TODO: test
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
                        MailingHelper.GenerateAndSendBestätigungsMail(anmeldung, _schulungRepository.GetById(anmeldung.SchulungGuid), Util.getVorstand(_context), Util.getRootUrl(Request), emailSender);
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

        /// Get the reason why it can't be cancelled, if it can be cancelled null is returned
        public String getCantCancelReason(Anmeldung anmeldung) {
            var earliest = anmeldung.Schulung.Termine.OrderBy(x => x.Start).FirstOrDefault();
            if (DateTime.Now > earliest.Start) {
                return "Schulung hat bereits begonnen!";
            }
            if (anmeldung.Schulung.IsAbgesagt) {
                return "Schulung ist bereits abgesagt!";
            }
            return null;
        }
    }
}
