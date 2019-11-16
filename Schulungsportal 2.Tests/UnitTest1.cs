using System;
using Xunit;
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
    public class UnitTest1
    {
        ApplicationDbContext context;
        SchulungRepository sr;
        AnmeldungRepository ar;
        AnmeldungController ac;
        SchulungApiController sac;
        MockEmailSender emailSender = new MockEmailSender();

        public UnitTest1()
        {
            context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("test").Options);
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new MapperProfile())).CreateMapper();
            sr = new SchulungRepository(context);
            ar = new AnmeldungRepository(context, mapper);
            ac = new AnmeldungController(context, emailSender, mapper);
            sac = new SchulungApiController(context, emailSender, mapper);
        }

        [Fact]
        public void TestAnmeldungWithMatchCount()
        {
            // save current datetime so it isn't affected by programm execution and can be checked later
            DateTime now = DateTime.Now;
            Utils.CreateTestData(context,now);
            // test with one match
            IEnumerable<AnmeldungRepository.AnmeldungWithMatchCount> results = ar.SearchAnmeldungenWithMatchCount("Vor", "Nach", "irrelevant", "");
            Assert.NotNull(results);
            Assert.Single(results);
            AnmeldungRepository.AnmeldungWithMatchCount awmc = results.First();
            Assert.Equal(2, awmc.matchCount);

            // test with phone number, more matches with different match count
            results = ar.SearchAnmeldungenWithMatchCount("test", "", "", "123");
            Assert.NotNull(results);
            Assert.Equal(3, results.Count());
            Assert.Equal(1, results.ElementAt(0).matchCount);
            Assert.Equal(1, results.ElementAt(1).matchCount);
            Assert.Equal(2, results.ElementAt(2).matchCount);

            // test match all, case insensitive
            results = ar.SearchAnmeldungenWithMatchCount("Vor", "ASDF", "test@test.test", "123");
            Assert.NotNull(results);
            Assert.Equal(4, results.Count());
            Assert.Equal(3, results.ElementAt(0).matchCount);
            Assert.Equal(3, results.ElementAt(1).matchCount);
            Assert.Equal(3, results.ElementAt(2).matchCount);
            Assert.Equal(1, results.ElementAt(3).matchCount);
        }

        [Fact]
        public void TestAnmeldung()
        {
            // save current datetime so it isn't affected by programm execution and can be checked later
            DateTime now = DateTime.Now;
            Utils.CreateTestData(context,now);
            MockEmailSender.sentMessages.Clear();

            // check types
            ActionResult result = ac.Anmeldung();
            Assert.IsType<ViewResult>(result);
            ViewResult vRes = (ViewResult)result;
            Assert.IsType<AnmeldungViewModel>(vRes.Model);
            AnmeldungViewModel avm = (AnmeldungViewModel)vRes.Model;

            // 1 schulung
            Assert.Single(avm.Schulungen);

            Assert.Equal("00000000-0000-0000-0000-000000000000", avm.SchulungsCheckboxen.ElementAt(0).Guid);

            // Für die Schulung anmelden:
            avm.Nachname = "testN";
            avm.Vorname = "testV";
            avm.Nummer = "123";
            avm.Status = "Mitglied";
            avm.Email = "test@test.de";
            avm.SchulungsCheckboxen.ElementAt(0).Checked = true;

            result = ac.Anmeldung(avm);

            // sleep cause sending the mail is async
            System.Threading.Thread.Sleep(1000);

            // assert there was a mail sent:
            Assert.Single(MockEmailSender.sentMessages);
        }

        [Fact]
        public void TestAnmeldung2() // mehr tests fürs anmelden
        {
            // save current datetime so it isn't affected by programm execution and can be checked later
            DateTime now = DateTime.Now;
            Utils.CreateTestDataForAnmeldung(context,now);
            MockEmailSender.sentMessages.Clear();

            // check types
            ActionResult result = ac.Anmeldung();
            Assert.IsType<ViewResult>(result);
            ViewResult vRes = (ViewResult)result;
            Assert.IsType<AnmeldungViewModel>(vRes.Model);
            AnmeldungViewModel avm = (AnmeldungViewModel)vRes.Model;

            Assert.Equal(4, avm.Schulungen.Count);
            Assert.Equal(4, avm.SchulungsCheckboxen.Count);

            // check titel of checkboxes
            string schulungTime = now.AddDays(2).ToString("dd.MM.yyyy HH:mm");
            Assert.Equal("Test 0 am " + schulungTime + " Uhr", avm.SchulungsCheckboxen.ElementAt(0).Titel);
            Assert.Equal("Test 1 am " + schulungTime + " Uhr", avm.SchulungsCheckboxen.ElementAt(1).Titel);
            Assert.Equal("Test 4 am " + schulungTime + " Uhr", avm.SchulungsCheckboxen.ElementAt(2).Titel);
            Assert.Equal("Test 5 am " + schulungTime + " Uhr", avm.SchulungsCheckboxen.ElementAt(3).Titel);

            // mark 0,1,4
            avm.SchulungsCheckboxen.ElementAt(0).Checked = true;
            avm.SchulungsCheckboxen.ElementAt(1).Checked = true;
            avm.SchulungsCheckboxen.ElementAt(2).Checked = true;

            // enter data
            avm.Nachname = "testN";
            avm.Vorname = "testV";
            avm.Nummer = "123";
            avm.Status = "Mitglied";
            avm.Email = "test@test.test";

            // add more checkboxes with invalid courses, 2 and 3
            avm.SchulungsCheckboxen.Add(new SchulungsCheckBox(true, "asdf", "00000000-0000-0000-0000-000000000002"));
            avm.SchulungsCheckboxen.Add(new SchulungsCheckBox(true, "asdf", "00000000-0000-0000-0000-000000000003"));

            // submit model back
            result = ac.Anmeldung(avm);
            Assert.IsType<ViewResult>(result);
            vRes = (ViewResult)result;
            Assert.IsType<List<Schulung>>(vRes.Model);
            List<Schulung> schulungListRes = (List<Schulung>)vRes.Model;

            // 2 invalid are ignored, same as the one we didn't sign up for
            Assert.Equal(3, schulungListRes.Count);

            Assert.Equal("Test 0", schulungListRes.ElementAt(0).Titel);
            Assert.False(schulungListRes.ElementAt(0).Check);
            // this one is checked cause there already was a signup with this mail
            Assert.Equal("Test 1", schulungListRes.ElementAt(1).Titel);
            Assert.True(schulungListRes.ElementAt(1).Check);
            Assert.Equal("Test 4", schulungListRes.ElementAt(2).Titel);
            Assert.False(schulungListRes.ElementAt(2).Check);

            // sleep cause sending the mail is async
            System.Threading.Thread.Sleep(1000);

            // only mails for successful signups should have been sent
            Assert.Equal(2, MockEmailSender.sentMessages.Count);
        }

        [Fact]
        public void TestAnmeldungWithoutNummer() // you can still sign up if you don't supply a mobile number
        {
            // save current datetime so it isn't affected by programm execution and can be checked later
            DateTime now = DateTime.Now;
            Utils.CreateTestDataForAnmeldung(context,now);
            MockEmailSender.sentMessages.Clear();

            // check types
            ActionResult result = ac.Anmeldung();
            Assert.IsType<ViewResult>(result);
            ViewResult vRes = (ViewResult)result;
            Assert.IsType<AnmeldungViewModel>(vRes.Model);
            AnmeldungViewModel avm = (AnmeldungViewModel)vRes.Model;

            Assert.Equal(4, avm.Schulungen.Count);
            Assert.Equal(4, avm.SchulungsCheckboxen.Count);

            // check titel of checkboxes
            string schulungTime = now.AddDays(2).ToString("dd.MM.yyyy HH:mm");
            Assert.Equal("Test 0 am " + schulungTime + " Uhr", avm.SchulungsCheckboxen.ElementAt(0).Titel);
            Assert.Equal("Test 1 am " + schulungTime + " Uhr", avm.SchulungsCheckboxen.ElementAt(1).Titel);
            Assert.Equal("Test 4 am " + schulungTime + " Uhr", avm.SchulungsCheckboxen.ElementAt(2).Titel);
            Assert.Equal("Test 5 am " + schulungTime + " Uhr", avm.SchulungsCheckboxen.ElementAt(3).Titel);

            // mark 0,1,4
            avm.SchulungsCheckboxen.ElementAt(0).Checked = true;
            avm.SchulungsCheckboxen.ElementAt(1).Checked = true;
            avm.SchulungsCheckboxen.ElementAt(2).Checked = true;

            // enter data
            avm.Nachname = "testN";
            avm.Vorname = "testV";
            // specifically don't set this
            // avm.Nummer = "123";
            avm.Status = "Mitglied";
            avm.Email = "test@test.test";

            // submit model back
            result = ac.Anmeldung(avm);
            Assert.IsType<ViewResult>(result);
            vRes = (ViewResult)result;
            Assert.IsType<List<Schulung>>(vRes.Model);
            List<Schulung> schulungListRes = (List<Schulung>)vRes.Model;

            // 2 invalid are ignored, same as the one we didn't sign up for
            Assert.Equal(3, schulungListRes.Count);

            Assert.Equal("Test 0", schulungListRes.ElementAt(0).Titel);
            Assert.False(schulungListRes.ElementAt(0).Check);
            // this one is checked cause there already was a signup with this mail
            Assert.Equal("Test 1", schulungListRes.ElementAt(1).Titel);
            Assert.True(schulungListRes.ElementAt(1).Check);
            Assert.Equal("Test 4", schulungListRes.ElementAt(2).Titel);
            Assert.False(schulungListRes.ElementAt(2).Check);

            // sleep cause sending the mail is async
            System.Threading.Thread.Sleep(1000);

            // only mails for successful signups should have been sent
            Assert.Equal(2, MockEmailSender.sentMessages.Count);
        }

        [Fact]
        public void testAnmeldungError()
        {
            Utils.CreateTestData(context,DateTime.Now);

            // check types
            ActionResult result = ac.Anmeldung();
            Assert.IsType<ViewResult>(result);
            ViewResult vRes = (ViewResult)result;
            Assert.IsType<AnmeldungViewModel>(vRes.Model);
            AnmeldungViewModel avm = (AnmeldungViewModel)vRes.Model;
            
            // enter data
            avm.Nachname = "testN";
            avm.Vorname = "testV";
            avm.Nummer = "123";
            // try with invalid status
            avm.Status = "invalid";
            avm.Email = "test@test.test";

            // submit model back
            result = ac.Anmeldung(avm);
            Assert.IsType<ObjectResult>(result);
            ObjectResult statusCodeResult = (ObjectResult)result;

            // check error
            Assert.Equal(400, statusCodeResult.StatusCode);
            Assert.Equal("invalid status", statusCodeResult.Value);

            // fix data, add invalid id
            avm.Status = "Mitglied";
            avm.SchulungsCheckboxen.Add(new SchulungsCheckBox(true, "asdf", "invalid"));

            // submit model back
            result = ac.Anmeldung(avm);
            Assert.IsType<ObjectResult>(result);
            statusCodeResult = (ObjectResult)result;

            // check error
            Assert.Equal(404, statusCodeResult.StatusCode);
            Assert.Equal("Schulung existiert nicht", statusCodeResult.Value);
        }

        /*[Fact]
        public void TestSendMail()
        {
            Anmeldung a = new Anmeldung
            {
                Email = "test@test.test",
                Nachname = "TEAST",
                Vorname = "T.",
            };

            Schulung s = new Schulung
            {
                Titel = "Schulung 2",
                Beschreibung = "Schulung 2",
                EmailDozent = "test@test.test",
                Termine = CreateSingletonTermine(DateTime.Now.AddDays(2), DateTime.Now.AddDays(3)),
                IsAbgesagt = false,
                IsGeprüft = false,
                NameDozent = "Test Dozent",
                NummerDozent = "123",
                OrganisatorInstitution = "CC",
                Ort = "Büro",
                SchulungGUID = "00000000-0000-0000-0000-000000000001",
                Anmeldefrist = DateTime.Now.AddDays(-1),
            };

            MockEmailSender.sentMessages.Clear();

            MailingHelper.GenerateAndSendAbsageMail(a, s, "", emailSender).Wait();

            Assert.Equal("",MockEmailSender.sentMessages.First().ToString());
        }*/
    }
}
