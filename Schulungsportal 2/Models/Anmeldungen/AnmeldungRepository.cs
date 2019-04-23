﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Schulungsportal_2.Models.Schulungen;
using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
using Schulungsportal_2.Data;
using AutoMapper;

namespace Schulungsportal_2.Models.Anmeldungen
{
    /// <summary>
    /// Dieses Repository verwaltet die Anmeldung zu Schulungen mit Hilfe des ApplicationDbContext des Frameworks
    /// </summary>
    public class AnmeldungRepository
    {
        private ApplicationDbContext context;
        private IMapper mapper;

        /// <summary>
        /// Der Konstruktor weist sich den ApplicationDbContext des Frameworks zu.
        /// </summary>
        /// <param name="_context"> ApplicationDbContext des Frameworks </param>
        public AnmeldungRepository(ApplicationDbContext _context, IMapper mapper)
        {
            this.context = _context;
            this.mapper = mapper;
        }

        /// <summary>
        /// Methode um alle Anmeldungen zu erhalten
        /// </summary>
        /// <returns> Liste aller Anmeldungen </returns>
        public IEnumerable<Anmeldung> GetAll()
        {
            try
            {
                return context.Anmeldung.AsEnumerable();
            }
            catch(Exception e)
            {
                string code = "#101";
                e = new Exception("Fehler beim Zugriff auf die Datenbank-Anmeldung (" + e.Message + ") " + code, e);
                throw e;
            }            
        }

        /// <summary>
        /// Methode um alle Anmeldungen zu einer Schulung zu erhalten
        /// </summary>
        /// <param name="guid"> die SchulungGuid der Schulung </param>
        /// <returns> Liste aller Anmeldungen zu einer Schulung </returns>
        public IEnumerable<Anmeldung> GetBySchulungGuid(string guid)
        {
            try
            {
                return context.Anmeldung
                    .Where(x => x.SchulungGuid == guid).AsEnumerable();
            }
            catch (Exception e)
            {
                string code = "#102";
                e = new Exception("Fehler beim Finden der Anmeldungen in Datenbank-Anmeldung (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        /// <summary>
        /// Methode um eine neue Anmeldung hinzuzufügen.
        /// </summary>
        /// <param name="anmeldung"> ein Anmeldung-Objekt </param>
        public void Add(Anmeldung anmeldung)
        {
            try
            {
                context.Anmeldung.Add(anmeldung);
                context.SaveChanges();
            }
            catch (Exception e)
            {
                string code = "#103";
                e = new Exception("Fehler beim Erstellen der Anmeldung in Datenbank-Anmeldung (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        /// <summary>
        /// Gibt die zu einer anmeldungId zugehörige Anmeldung aus der Datenbank wieder. Wenn nicht vorhanden, wird eine Exception geworfen.
        /// </summary>
        /// <param name="id">anmeldungId der Anmeldung, nach der man sucht</param>
        /// <returns>Die Anmledung zu der ID</returns>
        public Anmeldung GetById(int id)
        {
            try
            {
                Anmeldung anmeldung = context.Anmeldung.Single(m => m.anmeldungId == id);
                return anmeldung;
            }
            catch (Exception e)
            {
                string code = "#104";
                e = new Exception("Fehler beim Finden der Anmeldung in Datenbank-Anmeldung (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        /// <summary>
        /// Methode um eine Anmeldung zu löschen.
        /// </summary>
        /// <param name="anmeldung"> ein Anmeldung-Objekt </param>
        public void Delete(Anmeldung anmeldung)
        {
            try
            {
                context.Anmeldung.Remove(anmeldung);
                context.SaveChanges();
            }
            catch (Exception e)
            {
                string code = "#105";
                e = new Exception("Fehler beim Löschen der Anmeldung in Datenbank-Anmeldung (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        /// <summary>
        /// Löscht die übergebenen IDs
        /// </summary>
        /// <param name="ids"></param>
        public void BulkAnonymizeIDs(IEnumerable<int> ids)
        {
            context.Anmeldung.Where(a => ids.Contains(a.anmeldungId))
                .ToList().ForEach(a =>
                {
                    a.Email = "-----";
                    a.Nachname = "-----";
                    a.Nummer = "-----";
                    a.Vorname = "-----";
                });
            context.SaveChanges();
        }

        /// <summary>
        /// überprüft ob die übergegebene Anmeldung bereits existiert
        /// </summary>
        /// <param name="anmeldung"> die zu prüfende Anmeldung </param>
        /// <returns> true wenn die Anmeldung bereits existiert sonst false</returns>
        public bool AnmeldungAlreadyExist(Anmeldung anmeldung)
        {
            try
            {
                if (context.Anmeldung.Where(x => x.SchulungGuid == anmeldung.SchulungGuid && x.Email == anmeldung.Email).Count() != 0)
                {
                    return true;
                } else
                {
                    return false;
                }
            }
            catch(Exception e)
            {
                string code = "#106";
                e = new Exception("Fehler bei Überprüfung der Anmeldung mit Datenbank-Anmeldung (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        /// <summary>
        /// Methode zum Suchen von Anmeldungen in der Datenbank, es kann nach Vor-, Nachname, Email und Handynummer
        /// gesucht werden, als Ergebnis wird die Anmeldung zurückgegeben sowie die Anzahl der passenden Suchparameter
        /// </summary>
        /// <param name="vorname"></param>
        /// <param name="nachname"></param>
        /// <param name="email"></param>
        /// <param name="handynummer"></param>
        /// <returns></returns>
        public IEnumerable<AnmeldungWithMatchCount> SearchAnmeldungenWithMatchCount(string vorname, string nachname, string email, string handynummer)
        {
            return context.Anmeldung.AsEnumerable().Select((anmeldung) => new {
                anmeldung = anmeldung,
                matchC = (String.Equals(anmeldung.Vorname.Trim(), vorname.Trim(), StringComparison.OrdinalIgnoreCase) ? 1 : 0) +
                         (String.Equals(anmeldung.Nachname.Trim(), nachname.Trim(), StringComparison.OrdinalIgnoreCase) ? 1 : 0) +
                         (String.Equals(anmeldung.Email.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase) ? 1 : 0) +
                         (String.Equals(anmeldung.Nummer.Trim(), handynummer.Trim(), StringComparison.OrdinalIgnoreCase) ? 1 : 0)
             }).Where((anm)=>anm.matchC>0).Select((anm)=> {
                 AnmeldungWithMatchCount amwc = mapper.Map<AnmeldungWithMatchCount>(anm.anmeldung);
                 amwc.matchCount = anm.matchC;
                return amwc;
             });
        }

        /// <summary>
        /// Wird für das Suchen nach Teilnehmern verwendet, damit diese gelöscht werden können, erweitert die normale Anmeldung um einen
        /// "MatchCount" der angibt, wieviele der Suchparameter übereinstimmten
        /// </summary>
        [NotMapped]
        public class AnmeldungWithMatchCount : Anmeldung
        {
            public int matchCount { get; set; }
        }
    }
}
