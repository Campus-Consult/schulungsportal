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

namespace Schulungsportal_2.Controllers {
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

        public static SchulungDTO toDTO(Schulung s) {
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

    public class InternalSchulungDTO : SchulungDTO {

        public String EmailDozent { get; set; }

        public String NameDozent { get; set; }

        public String NummerDozent { get; set; }

        public bool IsGeprueft { get; set; }

        public new static InternalSchulungDTO toDTO(Schulung s) {
            var termine = s.Termine.Select(t => new TerminDTO
            {
                Start = t.Start,
                End = t.End,
            });
            return new InternalSchulungDTO
                {
                Anmeldefrist = s.Anmeldefrist,
                Beschreibung = s.Beschreibung,
                IsAbgesagt = s.IsAbgesagt,
                OrganisatorInstitution = s.OrganisatorInstitution,
                Ort = s.Ort,
                SchulungGUID = s.SchulungGUID,
                Termine = termine,
                Titel = s.Titel,
                EmailDozent = s.EmailDozent,
                NummerDozent = s.NummerDozent,
                NameDozent = s.NameDozent,
                IsGeprueft = s.IsGeprüft,
            };
        }
    }

    public class SchulungDTOAnmeldungsZahl: SchulungDTO {
        public int AnmeldungsZahl { get; set; }

        public new static SchulungDTOAnmeldungsZahl toDTO(Schulung s) {
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
    }

    public class SucheRequest {
        public String vorname {get; set;}
        public String nachname {get; set;}
        public String email {get; set;}
        public String handynummer {get; set;}

        public bool IsAllNull() {
            return vorname == null && nachname == null && email == null && handynummer == null;
        }

        public void CleanNulls() {
            vorname = vorname == null ? "" : vorname;
            nachname = nachname == null ? "" : nachname;
            email = email == null ? "" : email;
            handynummer = handynummer == null ? "" : handynummer;
        }
    }

    public class AnmeldungWithMatchCountDTO {
        public int AnmeldungID {get; set;}
        public String SchulungGUID {get; set;}
        public String Vorname {get; set;}
        public String Nachname {get; set;}
        public String EMail {get; set;}
        public String Handynummer {get; set;}
        public String Status {get; set;}
        public int MatchCount {get; set;}
        public InternalSchulungDTO Schulung {get; set;}

        public static AnmeldungWithMatchCountDTO toDTO(AnmeldungRepository.AnmeldungWithMatchCount awmc) {
            return new AnmeldungWithMatchCountDTO {
                AnmeldungID = awmc.anmeldungId,
                EMail = awmc.Email,
                Handynummer = awmc.Nummer,
                MatchCount = awmc.matchCount,
                Nachname = awmc.Nachname,
                SchulungGUID = awmc.SchulungGuid,
                Status = awmc.Status,
                Vorname = awmc.Vorname,
                Schulung = InternalSchulungDTO.toDTO(awmc.Schulung),
            };
        }
    }
}