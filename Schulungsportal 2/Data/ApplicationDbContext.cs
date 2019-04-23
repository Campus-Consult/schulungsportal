using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Schulungsportal_2.Models;
using Schulungsportal_2.Models.Anmeldungen;
using Schulungsportal_2.Models.Schulungen;

namespace Schulungsportal_2.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public ApplicationDbContext()
            : base()
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Add uniqueness Index on Schulung
            builder.Entity<Schulung>()
                .HasIndex(s => s.AccessToken)
                .IsUnique();

            builder.Entity<Termin>()
                .HasKey(t => new { t.SchulungGUID, t.Start, t.End });

            builder.Entity<Termin>()
                .HasOne(t => t.Schulung)
                .WithMany(s => s.Termine)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Anmeldung>()
                .HasOne(a => a.Schulung)
                .WithMany(s => s.Anmeldungen)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // Objekte zum Zugriff auf die Datenbank
        public DbSet<Schulung> Schulung { get; set; }
        public DbSet<Anmeldung> Anmeldung { get; set; }
        public DbSet<Impressum> Impressum { get; set; }
        public DbSet<MailProperties> MailProperties { get; set; }


        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
