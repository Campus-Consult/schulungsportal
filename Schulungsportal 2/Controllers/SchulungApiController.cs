using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [Route("api/schulungen")]
    /// <summary>
    /// Der SchulungController beinhaltet alle Aktionen zur  Verwaltung und Anzeige von Schulungen oder deren Inhalten
    /// </summary>
    public class SchulungApiController : Controller
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
        public SchulungApiController(ApplicationDbContext context, IEmailSender emailSender, IMapper mapper)
        {
            _schulungRepository = new SchulungRepository(context);
            _anmeldungRepository = new AnmeldungRepository(context, mapper);
            _context = context;
            _mapper = mapper;
            this.emailSender = emailSender;
        }

        [Route("")]
        public ActionResult<IEnumerable<SchulungDTO>> GetAll(int offset = 0, int max = 100) {
            // don't allow negative offsets and limit max to 200
            if (offset < 0 || max < 0 || max > 200) {
                return StatusCode(400);
            }
            return Json(_context.Schulung
                .Include(s => s.Anmeldungen)
                .Include(s => s.Termine)
                .OrderBy(s => s.Anmeldefrist)
                .Skip(offset)
                .Take(max)
                .Select(toSchulungDTOAnmeldungsZahl)
                .AsEnumerable());
        }

        [Route("upcoming")]
        public ActionResult<IEnumerable<SchulungDTO>> GetUpcoming(int offset = 0, int max = 100) {
            // don't allow negative offsets and limit max to 200
            if (offset < 0 || max < 0 || max > 200) {
                return StatusCode(400);
            }
            return Json(_context.Schulung
                .Where(s => s.Anmeldefrist > DateTime.Now)
                .Where(s => !s.IsAbgesagt)
                .Include(s => s.Anmeldungen)
                .Include(s => s.Termine)
                .OrderBy(s => s.Anmeldefrist)
                .Skip(offset)
                .Take(max)
                .Select(toSchulungDTOAnmeldungsZahl)
                .AsEnumerable());
        }

        [Authorize(Roles="Verwaltung")]
        [Route("foranmeldungen")]
        public ActionResult<IEnumerable<SchulungDTO>> GetForAnmeldungen(String ids) {
            try {
                var anmeldungen = ids.Split(",").Select(id => int.Parse(id));
                return Json(_schulungRepository.GetSchulungenForAnmeldungIDs(anmeldungen)
                    .Where(s => s.IsGeprüft)
                    .Select(toSchulungDTO)
                    .AsEnumerable());
            } catch (FormatException) {
                return BadRequest();
            }
        }

        public class TerminDTO {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }

        public class SchulungDTO {
            public string SchulungGUID { get; set; }

            public String Titel { get; set; }

            public String OrganisatorInstitution { get; set; }

            public String Beschreibung { get; set; }

            public String Ort { get; set; }
            
            public DateTime Anmeldefrist { get; set; }

            public IEnumerable<TerminDTO> Termine { get; set; }

            public Boolean IsAbgesagt { get; set; }
        }

        public class SchulungDTOAnmeldungsZahl: SchulungDTO {
            public int AnmeldungsZahl { get; set; }
        }

        private SchulungDTOAnmeldungsZahl toSchulungDTOAnmeldungsZahl(Schulung s) {
            var termine = s.Termine.Select(t => new TerminDTO
            {
                Start = t.Start,
                End = t.End,
            });
            return new SchulungDTOAnmeldungsZahl
                {
                Anmeldefrist = s.Anmeldefrist,
                Beschreibung = s.Beschreibung,
                AnmeldungsZahl = s.Anmeldungen.Count,
                IsAbgesagt = s.IsAbgesagt,
                OrganisatorInstitution = s.OrganisatorInstitution,
                Ort = s.Ort,
                SchulungGUID = s.SchulungGUID,
                Termine = termine,
                Titel = s.Titel,
            };
        }

        private SchulungDTO toSchulungDTO(Schulung s) {
            var termine = s.Termine.Select(t => new TerminDTO
            {
                Start = t.Start,
                End = t.End,
            });
            return new SchulungDTO
                {
                Anmeldefrist = s.Anmeldefrist,
                Beschreibung = s.Beschreibung,
                IsAbgesagt = s.IsAbgesagt,
                OrganisatorInstitution = s.OrganisatorInstitution,
                Ort = s.Ort,
                SchulungGUID = s.SchulungGUID,
                Termine = termine,
                Titel = s.Titel,
            };
        }
    }
}