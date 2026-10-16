using RaceDay.Api.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Api.Tests.Tests
{
    public class AuthenticationTests
        : IClassFixture<RaceDayApiFactory>
    {
        private readonly HttpClient _client;

        public AuthenticationTests(RaceDayApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
        {
            // Arrange
            var request = new
            {
                AuthenticationRole = "Participant",
                Email = "unknown@example.com",
                Password = "WrongPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/api/auth/login",
                request);

            // Assert
            var responseBody = await response.Content.ReadAsStringAsync();

            Assert.True(
                response.StatusCode == HttpStatusCode.Unauthorized,
                $"Expected 401 Unauthorized, but received " +
                $"{(int)response.StatusCode} ({response.StatusCode}). " +
                $"Response body: {responseBody}");
        }

        [Fact]
        public async Task Login_WithMissingCredentials_ReturnsBadRequest()
        {
            // Arrange
            var request = new
            {
                Email = "",
                Password = ""
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/api/auth/login",
                request);

            // Assert
            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);
        }
        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkAndToken()
        {
            // Arrange: register a Participant with a unique email.
            var email = $"participant_{Guid.NewGuid():N}@example.com";
            var password = "TestPassword123!";

            var registrationResponse = await _client.PostAsJsonAsync(
                "/api/auth/register/participant",
                new
                {
                    FirstName = "Test",
                    LastName = "Participant",
                    CellNumber = "0712345678",
                    Address = "Test Address",
                    Email = email,
                    Password = password
                });

            Assert.Equal(
                HttpStatusCode.Created,
                registrationResponse.StatusCode);

            // Act: log in with the registered credentials.
            var loginResponse = await _client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    AuthenticationRole = "Participant",
                    Email = email,
                    Password = password
                });

            // Assert: login succeeds and a JWT is returned.
            Assert.Equal(
                HttpStatusCode.OK,
                loginResponse.StatusCode);

            var responseBody = await loginResponse.Content
                .ReadFromJsonAsync<System.Text.Json.JsonElement>();

            Assert.True(
                responseBody.TryGetProperty("accessToken", out var token));

            Assert.False(string.IsNullOrWhiteSpace(token.GetString()));
        }

        [Fact]
        public async Task Login_WithIncorrectPassword_ReturnsUnauthorized()
        {
            // Arrange: register a Participant.
            var email = $"participant_{Guid.NewGuid():N}@example.com";
            var password = "CorrectPassword123!";

            var registrationResponse = await _client.PostAsJsonAsync(
                "/api/auth/register/participant",
                new
                {
                    FirstName = "Test",
                    LastName = "Participant",
                    CellNumber = "0712345678",
                    Address = "Test Address",
                    Email = email,
                    Password = password
                });

            Assert.Equal(
                HttpStatusCode.Created,
                registrationResponse.StatusCode);

            // Act: use the correct email but an incorrect password.
            var loginResponse = await _client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    AuthenticationRole = "Participant",
                    Email = email,
                    Password = "WrongPassword123!"
                });

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                loginResponse.StatusCode);
        }
        [Fact]
        public async Task RegisterParticipant_WithValidDetails_ReturnsCreated()
        {
            // Arrange
            var email = $"participant_{Guid.NewGuid():N}@example.com";

            var request = new
            {
                FirstName = "Test",
                LastName = "Participant",
                CellNumber = "0712345678",
                Address = "Test Address",
                Email = email,
                Password = "TestPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/api/auth/register/participant",
                request);

            // Assert
            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);
        }
    }
}
