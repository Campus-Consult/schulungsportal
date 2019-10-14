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
    [Route("api/suche")]
    /// <summary>
    /// Der SchulungController beinhaltet alle Aktionen zur  Verwaltung und Anzeige von Schulungen oder deren Inhalten
    /// </summary>
    public class SucheApiController : Controller
    {
        private SchulungRepository _schulungRepository;
        private AnmeldungRepository _anmeldungRepository;
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private ISchulungsportalEmailSender emailSender;

        // logger
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// SchulungController Konstruktor legt Repositories für Datenzugriff an.
        /// </summary>
        public SucheApiController(ApplicationDbContext context, ISchulungsportalEmailSender emailSender, IMapper mapper)
        {
            _schulungRepository = new SchulungRepository(context);
            _anmeldungRepository = new AnmeldungRepository(context, mapper);
            _context = context;
            _mapper = mapper;
            this.emailSender = emailSender;
        }

        /// Sucht Teilnehmer nach dem übergebenen request
        [Route("teilnehmer")]
        [HttpPost]
        [Authorize(Roles = "Verwaltung")]
        public ActionResult<IEnumerable<AnmeldungWithMatchCountDTO>> SucheTeilnehmer([FromBody] SucheRequest sucheRequest) {
            if (sucheRequest == null || sucheRequest.IsAllNull()) {
                return BadRequest();
            }
            sucheRequest.CleanNulls();
            return Json(_anmeldungRepository.SearchAnmeldungenWithMatchCount(sucheRequest.vorname, sucheRequest.nachname, sucheRequest.email, sucheRequest.handynummer)
               .Select(AnmeldungWithMatchCountDTO.toDTO));
        }

        private String CleanNull(String maybeNull) {
            return maybeNull == null ? "" : maybeNull;
        }
    }
}