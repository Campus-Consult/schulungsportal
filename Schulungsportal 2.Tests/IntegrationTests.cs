using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit;

using Schulungsportal_2.Controllers;

namespace Schulungsportal_2_Tests
{
    public class IntegrationTests 
        : IClassFixture<WebApplicationFactory<Schulungsportal_2.Startup>>
    {
        private readonly WebApplicationFactory<Schulungsportal_2.Startup> _factory;

        public IntegrationTests(WebApplicationFactory<Schulungsportal_2.Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Get_SchulungsApiAll()
        {
            // Arrange
            var client = CustomWebApplicationFactoryHelper.GetFactory(_factory, db => {
                Utils.CreateTestData(db, DateTime.Parse("2019-06-19T00:00:00"));
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/api/schulungen");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("application/json; charset=utf-8", 
                response.Content.Headers.ContentType.ToString());
            var gotJson = await response.Content.ReadAsStringAsync();
            var expectedJson = "[{\"anmeldungsZahl\":0,\"schulungGUID\":\"00000000-0000-0000-0000-000000000001\",\"titel\":\"Schulung 2\",\"organisatorInstitution\":\"CC\",\"beschreibung\":\"Schulung 2\",\"ort\":\"Büro\",\"anmeldefrist\":\"2019-06-18T00:00:00\",\"termine\":[{\"start\":\"2019-06-21T00:00:00\",\"end\":\"2019-06-22T00:00:00\"}],\"isAbgesagt\":false},{\"anmeldungsZahl\":4,\"schulungGUID\":\"00000000-0000-0000-0000-000000000000\",\"titel\":\"Schulung 1\",\"organisatorInstitution\":\"CC\",\"beschreibung\":\"Schulung 1\",\"ort\":\"Büro\",\"anmeldefrist\":\"2019-06-20T00:00:00\",\"termine\":[{\"start\":\"2019-06-21T00:00:00\",\"end\":\"2019-06-22T00:00:00\"}],\"isAbgesagt\":false}]";
            Assert.Equal(expectedJson, gotJson);
        }

        [Theory]
        // too low
        [InlineData(-1,5)]
        [InlineData(0,-5)]
        // too high
        [InlineData(0,201)]
        public async Task Get_SchulungsApiAllInvalidRange(int offset, int max) {
            // Arrange
            var client = CustomWebApplicationFactoryHelper.GetFactory(_factory, db => {
                Utils.CreateTestData(db, DateTime.Parse("2019-06-19T00:00:00"));
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/api/schulungen?offset="+offset+"&max="+max);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // second result
        [Theory]
        [InlineData(1,1)]
        [InlineData(1,100)]
        [InlineData(1,200)]
        public async Task Get_SchulungsApiAllSecond(int offset, int max) {
            // Arrange
            var client = CustomWebApplicationFactoryHelper.GetFactory(_factory, db => {
                Utils.CreateTestData(db, DateTime.Parse("2019-06-19T00:00:00"));
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/api/schulungen?offset="+offset+"&max="+max);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8", 
                response.Content.Headers.ContentType.ToString());
            var gotJson = await response.Content.ReadAsStringAsync();
            var expectedJson = "[{\"anmeldungsZahl\":4,\"schulungGUID\":\"00000000-0000-0000-0000-000000000000\",\"titel\":\"Schulung 1\",\"organisatorInstitution\":\"CC\",\"beschreibung\":\"Schulung 1\",\"ort\":\"Büro\",\"anmeldefrist\":\"2019-06-20T00:00:00\",\"termine\":[{\"start\":\"2019-06-21T00:00:00\",\"end\":\"2019-06-22T00:00:00\"}],\"isAbgesagt\":false}]";
            Assert.Equal(expectedJson, gotJson);
        }

        [Theory]
        [InlineData(0,0)]
        [InlineData(2,100)]
        [InlineData(123456,1)]
        public async Task Get_SchulungsApiAllNone(int offset, int max) {
            // Arrange
            var client = CustomWebApplicationFactoryHelper.GetFactory(_factory, db => {
                Utils.CreateTestData(db, DateTime.Parse("2019-06-19T00:00:00"));
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/api/schulungen?offset="+offset+"&max="+max);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8", 
                response.Content.Headers.ContentType.ToString());
            var gotJson = await response.Content.ReadAsStringAsync();
            var expectedJson = "[]";
            Assert.Equal(expectedJson, gotJson);
        }

        [Fact]
        public async Task Get_SchulungenForAnmeldungIDsCheckAuthorized() {
            // Arrange
            var client = CustomWebApplicationFactoryHelper.GetFactory(_factory, db => {
                Utils.CreateTestData(db, DateTime.Parse("2019-06-19T00:00:00"));
            }, isAuthenticated: false).CreateClient();

            // Act
            var response = await client.GetAsync("/api/schulungen/foranmeldungen?ids=1");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Get_SchulungenForAnmeldungIDsInvalid() {
            // Arrange
            var client = CustomWebApplicationFactoryHelper.GetFactory(_factory, db => {
                Utils.CreateTestData(db, DateTime.Parse("2019-06-19T00:00:00"));
            }, isAuthenticated: true).CreateClient();

            // Act
            var response = await client.GetAsync("/api/schulungen/foranmeldungen?ids=test,123");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_SchulungenForAnmeldungIDs() {
            // Arrange
            var client = CustomWebApplicationFactoryHelper.GetFactory(_factory, db => {
                Utils.CreateTestDataForSearch(db, DateTime.Parse("2019-06-19T00:00:00"));
            }, isAuthenticated: true).CreateClient();

            // Act
            var response = await client.GetAsync("/api/schulungen/foranmeldungen?ids=1,-1,123456,12");

            // Assert
            response.EnsureSuccessStatusCode();
            var expectedJson = "[{\"schulungGUID\":\"00000000-0000-0000-0000-000000000000\",\"titel\":\"Test 0\",\"organisatorInstitution\":\"CC\",\"beschreibung\":\"Test 0\",\"ort\":\"hier\",\"anmeldefrist\":\"2019-03-11T00:00:00\",\"termine\":[{\"start\":\"2019-03-12T00:00:00\",\"end\":\"2019-03-13T00:00:00\"}],\"isAbgesagt\":false}]";
            Assert.Equal(expectedJson, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_TeilnehmerSucheUnauthorized() {
            // Arrange
            var client = CustomWebApplicationFactoryHelper.GetFactory(_factory, db => {
                Utils.CreateTestDataForSearch(db, DateTime.Parse("2019-06-19T00:00:00"));
            }).CreateClient();

            // Act
            var response = await client.PostAsync("/api/suche/teilnehmer", new StringContent("{\"vorname\":\"Test\",\"nachname\":\"User\",\"email\":\"test@test.test\",\"handynummer\":\"12345\"}"));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal("", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_TeilnehmerSuche() {
            // Arrange
            var client = CustomWebApplicationFactoryHelper.GetFactory(_factory, db => {
                Utils.CreateTestDataForSearch(db, DateTime.Parse("2019-06-19T00:00:00"));
            }, isAuthenticated: true).CreateClient();

            // Act
            var response = await client.PostAsync("/api/suche/teilnehmer", new StringContent("{\"vorname\":\"Test\",\"nachname\":\"User\",\"email\":\"test@test.test\",\"handynummer\":\"12345\"}", Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            //throw new Exception(await response.Content.ReadAsStringAsync());
            var obj = JsonConvert.DeserializeObject<List<SucheApiController.AnmeldungWithMatchCountDTO>>(await response.Content.ReadAsStringAsync());
            Assert.Equal(4, obj.Count);
            var cur = obj[0];
            Assert.Equal(1, cur.AnmeldungID);
            Assert.Equal("00000000-0000-0000-0000-000000000000", cur.SchulungGUID);
            Assert.Equal("Test", cur.Vorname);
            Assert.Equal("User", cur.Nachname);
            Assert.Equal("test@test.test", cur.EMail);
            Assert.Equal("12345", cur.Handynummer);
            Assert.Equal("Mitglied", cur.Status);
            Assert.Equal(4, cur.MatchCount);
            cur = obj[1];
            Assert.Equal(3, cur.AnmeldungID);
            Assert.Equal("00000000-0000-0000-0000-000000000002", cur.SchulungGUID);
            Assert.Equal("Test2", cur.Vorname);
            Assert.Equal("User", cur.Nachname);
            Assert.Equal("test@test.test2", cur.EMail);
            Assert.Equal("123452", cur.Handynummer);
            Assert.Equal("Mitglied", cur.Status);
            Assert.Equal(1, cur.MatchCount);
            cur = obj[2];
            Assert.Equal(4, cur.AnmeldungID);
            Assert.Equal("00000000-0000-0000-0000-000000000003", cur.SchulungGUID);
            Assert.Equal("Test3", cur.Vorname);
            Assert.Equal("User3", cur.Nachname);
            Assert.Equal("test@test.test", cur.EMail);
            Assert.Equal("123453", cur.Handynummer);
            Assert.Equal("Mitglied", cur.Status);
            Assert.Equal(1, cur.MatchCount);
            cur = obj[3];
            Assert.Equal(5, cur.AnmeldungID);
            Assert.Equal("00000000-0000-0000-0000-000000000004", cur.SchulungGUID);
            Assert.Equal("Test4", cur.Vorname);
            Assert.Equal("User", cur.Nachname);
            Assert.Equal("test@test.test4", cur.EMail);
            Assert.Equal("123454", cur.Handynummer);
            Assert.Equal("Mitglied", cur.Status);
            Assert.Equal(1, cur.MatchCount);
        }
    }
}