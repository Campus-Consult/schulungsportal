using System;
using System.Linq;
using System.Collections.Generic;
using Schulungsportal_2.Data;
using Schulungsportal_2.Models.Schulungen;
using Schulungsportal_2.Models.Anmeldungen;
using Schulungsportal_2.Controllers;
using AutoMapper;
using Schulungsportal_2.Models;
using Microsoft.AspNetCore.Mvc;
using Schulungsportal_2.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Schulungsportal_2_Tests
{
    public class Utils
    {

        public static void CreateTestData(ApplicationDbContext context, DateTime now)
        {
            context.Schulung.RemoveRange(context.Schulung);
            context.Anmeldung.RemoveRange(context.Anmeldung);
            context.SaveChanges();
            context.Add(new Schulung {
                Titel = "Schulung 1",
                Beschreibung = "Schulung 1",
                EmailDozent = "test@test.test",
                Termine = CreateSingletonTermine(now.AddDays(2), now.AddDays(3)),
                IsAbgesagt = false,
                IsGeprüft = false,
                NameDozent = "Test Dozent",
                NummerDozent = "123",
                OrganisatorInstitution = "CC",
                Ort = "Büro",
                SchulungGUID = "00000000-0000-0000-0000-000000000000",
                Anmeldefrist = now.AddDays(1),
            });
            context.Add(new Schulung
            {
                Titel = "Schulung 2",
                Beschreibung = "Schulung 2",
                EmailDozent = "test@test.test",
                Termine = CreateSingletonTermine(now.AddDays(2), now.AddDays(3)),
                IsAbgesagt = false,
                IsGeprüft = false,
                NameDozent = "Test Dozent",
                NummerDozent = "123",
                OrganisatorInstitution = "CC",
                Ort = "Büro",
                SchulungGUID = "00000000-0000-0000-0000-000000000001",
                Anmeldefrist = now.AddDays(-1),
            });
            context.Add(new Anmeldung
            {
                Email = "test@test.test",
                Nachname = "Nach",
                Vorname = "Vor",
                Nummer = "123",
                SchulungGuid = "00000000-0000-0000-0000-000000000000",
                Status = "Mitglied"
            });
            context.Add(new Anmeldung
            {
                Email = "test@test.test",
                Nachname = "asdf",
                Vorname = "VorVor",
                Nummer = "123",
                SchulungGuid = "00000000-0000-0000-0000-000000000000",
                Status = "Mitglied"
            });
            context.Add(new Anmeldung
            {
                Email = "test@test.test",
                Nachname = "ASdf",
                Vorname = "test",
                Nummer = "123",
                SchulungGuid = "00000000-0000-0000-0000-000000000000",
                Status = "Mitglied"
            });
            context.Add(new Anmeldung
            {
                Email = "test@test.test",
                Nachname = "something",
                Vorname = "completely",
                Nummer = "different",
                SchulungGuid = "00000000-0000-0000-0000-000000000000",
                Status = "Mitglied"
            });
            context.SaveChanges();
        }

        public static void CreateTestDataForAnmeldung(ApplicationDbContext context, DateTime now)
        {
            context.Anmeldung.RemoveRange(context.Anmeldung);
            context.Schulung.RemoveRange(context.Schulung);
            context.SaveChanges();
            // Kann sich anmelden
            context.Add(HelpCreateSchulung("Test 0", "00000000-0000-0000-0000-000000000000", now.AddDays(1)));
            // Kann sich nicht anmelden, da schon angemeldet
            context.Add(HelpCreateSchulung("Test 1", "00000000-0000-0000-0000-000000000001", now.AddDays(1)));
            context.Add(new Anmeldung
            {
                Email = "test@test.test",
                Nachname = "something",
                Vorname = "completely",
                Nummer = "different",
                SchulungGuid = "00000000-0000-0000-0000-000000000001",
                Status = "Mitglied"
            });
            // nicht angezeigt, kann nicht anmelden da anmeldefrist abgelaufen
            context.Add(HelpCreateSchulung("Test 2", "00000000-0000-0000-0000-000000000002", now.AddDays(-1)));
            // nicht angezeigt, kann nicht anmelden, da abgesagt
            Schulung abgesagt = HelpCreateSchulung("Test 3", "00000000-0000-0000-0000-000000000003", now.AddDays(1));
            abgesagt.IsAbgesagt = true;
            context.Add(abgesagt);
            // Kann sich anmelden
            context.Add(HelpCreateSchulung("Test 4", "00000000-0000-0000-0000-000000000004", now.AddDays(1)));
            // Kann sich anmelden
            context.Add(HelpCreateSchulung("Test 5", "00000000-0000-0000-0000-000000000005", now.AddDays(1)));
            context.SaveChanges();
        }

        public static void CreateTestDataForSearch(ApplicationDbContext context, DateTime now) {
            context.Anmeldung.RemoveRange(context.Anmeldung);
            context.Schulung.RemoveRange(context.Schulung);
            context.SaveChanges();
            context.Add(HelpCreateSchulung("Test 0", "00000000-0000-0000-0000-000000000000", now.AddDays(-100), geprüft: true));
            context.Add(HelpCreateSchulung("Test 1", "00000000-0000-0000-0000-000000000001", now.AddDays(-99), geprüft: true));
            context.Add(HelpCreateSchulung("Test 2", "00000000-0000-0000-0000-000000000002", now.AddDays(-98), geprüft: true));
            context.Add(HelpCreateSchulung("Test 3", "00000000-0000-0000-0000-000000000003", now.AddDays(-97), geprüft: true));
            context.Add(HelpCreateSchulung("Test 4", "00000000-0000-0000-0000-000000000004", now.AddDays(-96), geprüft: true));
            var vorname = "Test";
            var nachname = "User";
            var email = "test@test.test";
            var handynummer = "12345";
            for (int i = 0; i < 5; i++)
            {
                context.Add(new Anmeldung
                {
                    Email = (i%3==0)?email:email+i,
                    Nachname = (i%2==0)?nachname:nachname+i,
                    Vorname = (i%5==0)?vorname:vorname+i,
                    Nummer = (i%7==0)?handynummer:handynummer+i,
                    SchulungGuid = "00000000-0000-0000-0000-00000000000"+(i%5),
                    Status = "Mitglied"
                });
            }
            context.SaveChanges();
        }

        public static List<Termin> CreateSingletonTermine(DateTime start, DateTime end)
        {
            List<Termin> testTermin = new List<Termin>(1);
            testTermin.Add(new Termin { Start = start, End = end});
            return testTermin;
        }

        public static Schulung HelpCreateSchulung(string name, string id, DateTime anmeldefrist, bool geprüft=false)
        {
            return new Schulung
            {
                AccessToken = id + "0",
                Anmeldefrist = anmeldefrist,
                Beschreibung = name,
                EmailDozent = "test@test.test",
                IsAbgesagt = false,
                IsGeprüft = geprüft,
                NameDozent = "test",
                NummerDozent = "12345",
                OrganisatorInstitution = "CC",
                Ort = "hier",
                SchulungGUID = id,
                Termine = CreateSingletonTermine(anmeldefrist.AddDays(1), anmeldefrist.AddDays(2)),
                Titel = name,
            };
        }
    }
}