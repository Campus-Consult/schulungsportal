using Microsoft.EntityFrameworkCore;
using Schulungsportal_2.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace Schulungsportal_2.Models
{
    /// <summary>
    /// Dieses Repository verwaltet den Zugriff auf das Datenbankobjekt Schulung mit Hilfe des ApplicationDbContext des Frameworks
    /// </summary>
    public class InviteRepository
    {
        private ApplicationDbContext context;

        /// <summary>
        /// Der Konstruktor weist sich den ApplicationDbContext des Frameworks zu.
        /// </summary>
        /// <param name="_context"> ApplicationDbContext des Frameworks </param>
        public InviteRepository(ApplicationDbContext _context)
        {
            this.context = _context;

        }

        public async Task<Invite> GetById(string id) {
            return await context.Invites.FindAsync(id);
        }

        public async Task<Invite> CreateForMail(string eMail, DateTime expirationTime) {
            Invite invite = new Invite {
                EMailAdress = eMail,
                InviteGUID = Guid.NewGuid().ToString(),
                ExpirationTime = expirationTime,
            };
            await context.Invites.AddAsync(invite);
            await context.SaveChangesAsync();
            return invite;
        }

        public async Task Remove(string guid) {
            context.Invites.Remove(await context.Invites.FindAsync(guid));
            await context.SaveChangesAsync();
        }

        /**
         * Entfernt alle abgelaufenen Invites
         */
        public async Task PurgeExpired() {
            DateTime now = DateTime.Now;
            context.Invites.RemoveRange(context.Invites.Where(i => i.ExpirationTime < now));
            await context.SaveChangesAsync();
        }
    }
}
