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
                    .Include(s => s.Dozenten)
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
                IEnumerable<Schulung> schulungen = context.Schulung
                    .Include(s => s.Termine)
                    .Include(s => s.Dozenten)
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
        public async Task<Schulung> AddAsync(Schulung schulung)
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
                await context.Schulung.AddAsync(schulung);
                await context.SaveChangesAsync();
                return schulung;
            }
            catch(Exception e)
            {
                Console.Error.Write(e);
                string code = "#005";
                e = new Exception("Fehler beim Anlegen der Schulung in der Datenbank-Schulung (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        /// <summary>
        /// Methode um das Schulungs-Objekt mit der übergebenen id zu erhalten.
        /// </summary>
        /// <param name="id"> ID des geforderten Schulungs-Objektes </param>
        /// <returns> die geforderte Schulung mit der ID oder null falls die Schulung nicht existiert </returns>
        public async Task<Schulung> GetByIdAsync(string id)
        {
            try
            {
                IQueryable<Schulung> schulungen = context.Schulung.Where(m => m.SchulungGUID == id)
                    .Include(s => s.Termine)
                    .Include(s => s.Dozenten);
                if (await schulungen.CountAsync() != 1)
                {
                    return null;
                }
                else
                {
                    return await schulungen.FirstAsync();
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
        public async Task<Schulung> GetByAccessTokenAsync(string accessToken)
        {
            IQueryable<Schulung> schulungen = context.Schulung.Where(m => m.AccessToken == accessToken)
                    .Include(s => s.Termine)
                    .Include(s => s.Dozenten);
            if (await schulungen.CountAsync() != 1)
            {
                return null;
            } else
            {
                return await schulungen.FirstAsync();
            }
        }

        public async Task<List<string>> GetPreviousOrganizersAsync()
        {
            return await context.Schulung.Select(schulung => schulung.OrganisatorInstitution).Distinct().ToListAsync();
        }

        public IEnumerable<Schulung> GetSchulungenForAnmeldungIDs(IEnumerable<int> anmeldungIDs)
        {
            return context.Anmeldung
                .Where(a => anmeldungIDs.Contains(a.anmeldungId))
                .Include(a => a.Schulung)
                .Select(a => a.Schulung)
                .Include(s => s.Termine)
                .OrderBy(s => s.Termine.FirstOrDefault().Start);
        }

        /// <summary>
        /// Gibt alle Schulungen zurück, die bereits vorbei sind, die aber nicht Abgesagt
        /// und auch noch nicht geprüft sind
        /// </summary>
        public IEnumerable<Schulung> GetUngeprüfteSchulungen() {
            var schulungsEnde = DateTime.Now.AddDays(-1);
            return context.Schulung
                .Include(s => s.Termine)
                    .Include(s => s.Dozenten)
                .Where(x => !x.IsAbgesagt && !x.IsGeprüft && x.Termine.Max(t => t.End) < schulungsEnde)
                .OrderBy(x => x.Anmeldefrist).AsEnumerable();
        }

        /// <summary>
        /// Methode um eine bestehende Schulung mit einer überarbeiteten Version zu ersetzen.
        /// Die bestehende Version und die Überarbeitete müssen die selbe ID haben.
        /// Wenn die ID null ist passiert nichts.
        /// Wenn die Schulung zu der ID nicht existiert passiert nichts.
        /// </summary>
        /// <param name="schulung"> überarbeitete Version der Schulung </param>
        public async Task UpdateAsync(Schulung schulung)
        {
            try
            {
                Schulung schulungAlt = await context.Schulung
                    .Include(s => s.Termine)
                    .Include(s => s.Dozenten)
                    .SingleAsync(m => m.SchulungGUID == schulung.SchulungGUID);
                schulungAlt.Beschreibung = schulung.Beschreibung;
                schulungAlt.Anmeldefrist = schulung.Anmeldefrist;
                schulungAlt.StartAnmeldefrist = schulung.StartAnmeldefrist;
                schulungAlt.Check = schulung.Check;
                if (!schulungAlt.Dozenten.Equals(schulung.Dozenten)) {
                    schulungAlt.Dozenten = schulung.Dozenten;
                }
                if (!schulungAlt.Termine.Equals(schulung.Termine))
                {
                    schulungAlt.Termine = schulung.Termine;
                }
                schulungAlt.GeprüftReminderSent = schulung.GeprüftReminderSent;
                schulungAlt.IsAbgesagt = schulung.IsAbgesagt;
                schulungAlt.IsGeprüft = schulung.IsGeprüft;
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
                await context.SaveChangesAsync();
            }
            catch(Exception e)
            {
                string code = "#007";
                e = new Exception("Fehler beim Update der Schulung in der Datenbank-Schulung (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        /// Setzt das Feld, dass die Mail zum Überprüfen der Anwesenheit versendet wurde
        public async Task SetGeprüftMailSent(String schulungsID, bool reminderSent) {
            Schulung schulung = await context.Schulung.FindAsync(schulungsID);
            if (schulung != null) {
                schulung.GeprüftReminderSent = reminderSent;
                context.Entry(schulung).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Methode um eine Schulung zu löschen.
        /// </summary>
        /// <param name="schulung"> ein Schulung-Objekt </param>
        public async Task DeleteAsync(Schulung schulung)
        {
            try
            {
                context.Schulung.Remove(schulung);
                await context.SaveChangesAsync();
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
