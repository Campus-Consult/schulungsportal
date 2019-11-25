﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Schulungsportal_2.Data;

namespace Schulungsportal_2.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20191125172550_ueberpruefungReminder")]
    partial class ueberpruefungReminder
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128);

                    b.Property<string>("Name")
                        .HasMaxLength(128);

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("Schulungsportal_2.Models.Anmeldungen.Anmeldung", b =>
                {
                    b.Property<int>("anmeldungId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccessToken")
                        .IsRequired()
                        .HasMaxLength(40);

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<string>("Nachname")
                        .IsRequired();

                    b.Property<string>("Nummer")
                        .IsRequired();

                    b.Property<string>("SchulungGuid");

                    b.Property<string>("Status")
                        .IsRequired();

                    b.Property<string>("Vorname")
                        .IsRequired();

                    b.HasKey("anmeldungId");

                    b.HasIndex("SchulungGuid");

                    b.ToTable("Anmeldung");
                });

            modelBuilder.Entity("Schulungsportal_2.Models.Impressum", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Anschrift")
                        .IsRequired();

                    b.Property<string>("Dienstanbieter")
                        .IsRequired();

                    b.Property<string>("JournalistischeVerantwortung")
                        .IsRequired();

                    b.Property<string>("Kommunikation")
                        .IsRequired();

                    b.Property<string>("KontaktEMail")
                        .IsRequired();

                    b.Property<string>("Verantwortungsbereich")
                        .IsRequired();

                    b.Property<string>("Vorstand")
                        .IsRequired();

                    b.HasKey("id");

                    b.ToTable("Impressum");
                });

            modelBuilder.Entity("Schulungsportal_2.Models.Invite", b =>
                {
                    b.Property<string>("InviteGUID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("EMailAdress")
                        .IsRequired();

                    b.Property<DateTime>("ExpirationTime");

                    b.HasKey("InviteGUID");

                    b.ToTable("Invites");
                });

            modelBuilder.Entity("Schulungsportal_2.Models.MailProperties", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("absender")
                        .IsRequired();

                    b.Property<string>("adresseSchulungsbeauftragter")
                        .IsRequired();

                    b.Property<string>("mailserver")
                        .IsRequired();

                    b.Property<string>("passwort")
                        .IsRequired();

                    b.Property<int>("port");

                    b.Property<bool>("useSsl");

                    b.HasKey("id");

                    b.ToTable("MailProperties");
                });

            modelBuilder.Entity("Schulungsportal_2.Models.Schulungen.Schulung", b =>
                {
                    b.Property<string>("SchulungGUID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccessToken")
                        .IsRequired()
                        .HasMaxLength(40);

                    b.Property<DateTime>("Anmeldefrist");

                    b.Property<string>("Beschreibung")
                        .IsRequired();

                    b.Property<string>("EmailDozent")
                        .IsRequired();

                    b.Property<bool>("GeprüftReminderSent");

                    b.Property<bool>("IsAbgesagt");

                    b.Property<bool>("IsGeprüft");

                    b.Property<string>("NameDozent")
                        .IsRequired();

                    b.Property<string>("NummerDozent")
                        .IsRequired();

                    b.Property<string>("OrganisatorInstitution")
                        .IsRequired();

                    b.Property<string>("Ort")
                        .IsRequired();

                    b.Property<string>("Titel")
                        .IsRequired();

                    b.HasKey("SchulungGUID");

                    b.HasIndex("AccessToken")
                        .IsUnique();

                    b.ToTable("Schulung");
                });

            modelBuilder.Entity("Schulungsportal_2.Models.Schulungen.Termin", b =>
                {
                    b.Property<string>("SchulungGUID");

                    b.Property<DateTime>("Start");

                    b.Property<DateTime>("End");

                    b.HasKey("SchulungGUID", "Start", "End");

                    b.ToTable("Termin");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Schulungsportal_2.Models.Anmeldungen.Anmeldung", b =>
                {
                    b.HasOne("Schulungsportal_2.Models.Schulungen.Schulung", "Schulung")
                        .WithMany("Anmeldungen")
                        .HasForeignKey("SchulungGuid")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Schulungsportal_2.Models.Schulungen.Termin", b =>
                {
                    b.HasOne("Schulungsportal_2.Models.Schulungen.Schulung", "Schulung")
                        .WithMany("Termine")
                        .HasForeignKey("SchulungGUID")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
