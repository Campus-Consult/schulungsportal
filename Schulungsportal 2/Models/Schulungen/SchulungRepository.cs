using Microsoft.EntityFrameworkCore;
using Schulungsportal_2.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace Schulungsportal_2.Models.Schulungen
{
    /// <summary>
    /// Dieses Repository verwaltet den Zugriff auf das Datenbankobjekt Schulung mit Hilfe des ApplicationDbContext des Frameworks
    /// </summary>
    public class SchulungRepository
    {
        public ApplicationDbContext context;

        /// <summary>
        /// Der Konstruktor weist sich den ApplicationDbContext des Frameworks zu.
        /// </summary>
        /// <param name="_context"> ApplicationDbContext des Frameworks </param>
        public SchulungRepository(ApplicationDbContext _context)
        {
            this.context = _context;

        }

        /// <summary>
        /// Methode um alle Schulung-Objekte des Repositories als IEnumerable-Liste zubekommen.
        /// </summary>
        /// <returns> alle Schulung-Objekte als IEnumerable-Liste </returns>
        public IEnumerable<Schulung> GetAllSortByDate()
        {
            try
            {
                return context.Schulung
                    .Include(s => s.Termine).OrderBy(x => x.Anmeldefrist).AsEnumerable();
            }
            catch(Exception e)
            {
                string code = "#001";
                e = new Exception("Fehler beim Zugriff auf die Datenbank-Schulung (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        /// <summary>
        /// Methode um alle Schulung-Objekte des Repositories als IEnumerable-Liste zubekommen, die noch nicht
        /// abgesagt und deren Anmeldefrist noch nicht abgelaufen ist
        /// </summary>
        /// <returns> alle Schulung-Objekte als IEnumerable-Liste </returns>
        public IEnumerable<Schulung> GetForRegSortByDate()
        {
            try
            {
                DateTime now = DateTime.Now;
                return context.Schulung
                    .Include(s => s.Termine)
                    .Include(s => s.Anmeldungen)
                    .Where(x => !x.IsAbgesagt && x.Anmeldefrist > now)
                    .OrderBy(x => x.Anmeldefrist).AsEnumerable();
            }
            catch (Exception e)
            {
                string code = "#002";
                e = new Exception("Fehler beim Zugriff auf die Datenbank-Schulung (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        /// <summary>
        /// Methode um die Schulung-Objekte des Repositories als IEnumerable-Liste zubekommen,
        /// die nicht Abgesagt und deren Anwesenheit noch nicht bestätigt wurde
        /// </summary>
        /// <returns> alle Schulung-Objekte als IEnumerable-Liste </returns>
        public IEnumerable<Schulung> GetForOverviewSortByDate()
        {
            try
            {
                DateTime now = DateTime.Now;
                IEnumerable<Schulung> schulungen = context.Schulung
                    .Include(s => s.Termine)
                    .Where(x => !x.IsAbgesagt && !x.IsGeprüft)
                    // TODO: select alle, die noch nicht komplett fertig sind, also wo die überprüfung noch fehlt
                    .OrderBy(x => x.Anmeldefrist).AsEnumerable();
                return schulungen;
            }
            catch (Exception e)
            {
                string code = "#003";
                e = new Exception("Fehler beim Zugriff auf die Datenbank-Schulung (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        /// <summary>
        /// Methode um die Schulung-Objekte des Repositories als IEnumerable-Liste zubekommen,
        /// die entweder abgesagt oder bereits berprüft sind
        /// </summary>
        /// <returns> alle Schulung-Objekte als IEnumerable-Liste </returns>
        public IEnumerable<Schulung> GetForArchivSortByDate()
        {
            try
            {
                DateTime now = DateTime.Now;
                IEnumerable<Schulung> schulungen = context.Schulung
                    .Include(s => s.Termine)
                    .Where(x => x.IsGeprüft || x.IsAbgesagt)
                    .OrderBy(x => x.Anmeldefrist).AsEnumerable().Reverse();
                return schulungen;
            }
            catch (Exception e)
            {
                string code = "#004";
                e = new Exception("Fehler beim Zugriff auf die Datenbank-Schulung (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        /// <summary>
        /// Methode um einem Schulung-Objekt eine GUID und dem Repository dieses Schulung-Objekt hinzuzufügen.
        /// </summary>
        /// <param name="schulung"> hinzuzufügende Schulung </param>
        public Schulung Add(Schulung schulung)
        {
            try
            {
                if (schulung.SchulungGUID == null)
                {
                    schulung.SchulungGUID = Guid.NewGuid().ToString();
                }
                if (schulung.AccessToken == null)
                {
                    schulung.AccessToken = Guid.NewGuid().ToString();
                }
                context.Schulung.Add(schulung);
                context.SaveChanges();
            }
            catch(Exception e)
            {
                Console.Error.Write(e);
                string code = "#005";
                e = new Exception("Fehler beim Anlegen der Schulung in der Datenbank-Schulung (" + e.Message + ") " + code, e);
                throw e;
            }
            return schulung;
        }

        /// <summary>
        /// Methode um das Schulungs-Objekt mit der übergebenen id zu erhalten.
        /// </summary>
        /// <param name="id"> ID des geforderten Schulungs-Objektes </param>
        /// <returns> die geforderte Schulung mit der ID oder null falls die Schulung nicht existiert </returns>
        public Schulung GetById(string id)
        {
            try
            {
                IQueryable<Schulung> schulungen = context.Schulung.Where(m => m.SchulungGUID == id)
                    .Include(s => s.Termine);
                if (schulungen.Count() != 1)
                {
                    return null;
                }
                else
                {
                    return schulungen.First();
                }
            }
            catch (Exception e)
            {
                string code = "#006";
                e = new Exception("Fehler beim Finden der Schulung in der Datenbank-Schulung (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        /// <summary>
        /// Methode um das Schulungs-Objekt mit dem übergebenen accessToken zu erhalten.
        /// </summary>
        /// <param name="accessToken"> AccessToken des geforderten Schulungs-Objektes, dies wird nur per Mail an den Dozenten gesendet </param>
        /// <returns> die geforderte Schulung mit dem accessToken oder null falls die Schulung nicht existiert </returns>
        public Schulung GetByAccessToken(string accessToken)
        {
            IQueryable<Schulung> schulungen = context.Schulung.Where(m => m.AccessToken == accessToken)
                    .Include(s => s.Termine);
            if (schulungen.Count() != 1)
            {
                return null;
            } else
            {
                return schulungen.First();
            }
        }

        public List<string> GetPreviousOrganizers()
        {
            return context.Schulung.Select(schulung => schulung.OrganisatorInstitution).Distinct().ToList();
        }

        public List<Schulung> GetSchulungenForAnmeldungIDs(IEnumerable<int> anmeldungIDs)
        {
            return context.Anmeldung
                .Where(a => anmeldungIDs.Contains(a.anmeldungId))
                .Include(a => a.Schulung)
                .Select(a => a.Schulung)
                .Include(s => s.Termine)
                .OrderBy(s => s.Termine.FirstOrDefault().Start).ToList();
        }

        /// <summary>
        /// Methode um eine bestehende Schulung mit einer überarbeiteten Version zu ersetzen.
        /// Die bestehende Version und die Überarbeitete müssen die selbe ID haben.
        /// Wenn die ID null ist passiert nichts.
        /// Wenn die Schulung zu der ID nicht existiert passiert nichts.
        /// </summary>
        /// <param name="schulung"> überarbeitete Version der Schulung </param>
        public void Update(Schulung schulung)
        {
            try
            {
                Schulung schulungAlt = context.Schulung.Include(s => s.Termine).Single(m => m.SchulungGUID == schulung.SchulungGUID);
                schulungAlt.Beschreibung = schulung.Beschreibung;
                schulungAlt.EmailDozent = schulung.EmailDozent;
                schulungAlt.Anmeldefrist = schulung.Anmeldefrist;
                schulungAlt.Check = schulung.Check;
                if (!schulungAlt.Termine.Equals(schulung.Termine))
                {
                    schulungAlt.Termine = schulung.Termine;
                }
                schulungAlt.IsAbgesagt = schulung.IsAbgesagt;
                schulungAlt.IsGeprüft = schulung.IsGeprüft;
                schulungAlt.NameDozent = schulung.NameDozent;
                schulungAlt.NummerDozent = schulung.NummerDozent;
                schulungAlt.OrganisatorInstitution = schulung.OrganisatorInstitution;
                schulungAlt.Ort = schulung.Ort;
                schulungAlt.Titel = schulung.Titel;
                // We don't wanna put the access token out there
                // so if it's not present thats ok
                if (schulung.AccessToken != null)
                {
                    schulungAlt.AccessToken = schulung.AccessToken;
                }
                context.Entry(schulungAlt).State = EntityState.Modified;
                context.SaveChanges();
            }
            catch(Exception e)
            {
                string code = "#007";
                e = new Exception("Fehler beim Update der Schulung in der Datenbank-Schulung (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        /// <summary>
        /// Methode um eine Schulung zu löschen.
        /// </summary>
        /// <param name="schulung"> ein Schulung-Objekt </param>
        public void Delete(Schulung schulung)
        {
            try
            {
                context.Schulung.Remove(schulung);
                context.SaveChanges();
            }
            catch(Exception e)
            {
                string code = "#008";
                e = new Exception("Fehler beim Löschen der Schulung in der Datenbank-Schulung (" + e.Message + ") " + code, e);
                throw e;
            }
        }
    }
}
