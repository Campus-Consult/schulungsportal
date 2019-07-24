using System.Collections.Generic;
using System.Net;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Schulungsportal_2_Tests
{
    public class IntegrationTests 
        : IClassFixture<CustomWebApplicationFactory<Schulungsportal_2.Startup>>
    {
        private readonly CustomWebApplicationFactory<Schulungsportal_2.Startup> _factory;

        public IntegrationTests(CustomWebApplicationFactory<Schulungsportal_2.Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Get_SchulungsApiAll()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/schulungen");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("application/json; charset=utf-8", 
                response.Content.Headers.ContentType.ToString());
            var gotJson = await response.Content.ReadAsStringAsync();
            var expectedJson = "[{\"schulungGUID\":\"00000000-0000-0000-0000-000000000001\",\"titel\":\"Schulung 2\",\"organisatorInstitution\":\"CC\",\"beschreibung\":\"Schulung 2\",\"ort\":\"Büro\",\"anmeldefrist\":\"2019-06-18T00:00:00\",\"termine\":[{\"start\":\"2019-06-21T00:00:00\",\"end\":\"2019-06-22T00:00:00\"}],\"anmeldungsZahl\":0,\"isAbgesagt\":false},{\"schulungGUID\":\"00000000-0000-0000-0000-000000000000\",\"titel\":\"Schulung 1\",\"organisatorInstitution\":\"CC\",\"beschreibung\":\"Schulung 1\",\"ort\":\"Büro\",\"anmeldefrist\":\"2019-06-20T00:00:00\",\"termine\":[{\"start\":\"2019-06-21T00:00:00\",\"end\":\"2019-06-22T00:00:00\"}],\"anmeldungsZahl\":4,\"isAbgesagt\":false}]";
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
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/schulungen?offset="+offset+"&max="+max);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}