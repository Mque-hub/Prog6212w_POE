using RaceDay.Api.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace RaceDay.Api.Tests.Tests
{
    public class EventEnrolmentTests
        : IClassFixture<RaceDayApiFactory>
    {
        private readonly HttpClient _client;

        public EventEnrolmentTests(RaceDayApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task UnauthenticatedUser_CannotEnrolInEvent()
        {
            // Arrange
            var request = new
            {
                CategoryId = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/api/events/1/enrolments",
                request);

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task Participant_CanEnrolInEvent()
        {
            // Arrange: use unique emails so repeated runs do not
            // conflict with previously registered accounts.
            var uniqueId = Guid.NewGuid().ToString("N");
            var password = "TestPassword123!";

            var organiserEmail = $"organiser_{uniqueId}@example.com";
            var participantEmail = $"participant_{uniqueId}@example.com";

            // 1. Register an Organiser.
            var organiserRegistration =
                await _client.PostAsJsonAsync(
                    "/api/auth/register/organiser",
                    new
                    {
                        FirstName = "Test",
                        LastName = "Organiser",
                        Address = "Test Address",
                        Email = organiserEmail,
                        Password = password
                    });

            Assert.Equal(
                HttpStatusCode.Created,
                organiserRegistration.StatusCode);

            // 2. Log in as the Organiser and get the JWT.
            var organiserToken = await LoginAsync(
                organiserEmail,
                password);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer", organiserToken);

            // 3. Create an Event.
            var eventResponse = await _client.PostAsJsonAsync(
                "/api/events",
                new
                {
                    EventName = "Test Running Event",
                    Description = "Event created for automated testing",
                    EventType = "Running",
                    EventDate = DateOnly.FromDateTime(
                        DateTime.Today.AddDays(30)),
                    EventDuration = TimeSpan.FromHours(2),
                    Location = "Test Venue",
                    Distance = 5.0m
                });

            Assert.Equal(
                HttpStatusCode.Created,
                eventResponse.StatusCode);

            // The EventDto does not contain EventID, so obtain
            // the ID from the Location header.
            var location = eventResponse.Headers.Location;

            Assert.NotNull(location);

            var eventIdText = location!.Segments.Last().Trim('/');

            Assert.True(
                int.TryParse(eventIdText, out var eventId),
                "Could not obtain the new Event ID from Location.");

            // 4. Create a Category for that Event.
            var categoryResponse = await _client.PostAsJsonAsync(
                $"/api/events/{eventId}/categories",
                new
                {
                    EventID = eventId,
                    CategoryName = "Open",
                    MinAge = 18,
                    MaxAge = 60
                });

            Assert.Equal(
                HttpStatusCode.Created,
                categoryResponse.StatusCode);

            using var categoryJson = JsonDocument.Parse(
                await categoryResponse.Content.ReadAsStringAsync());

            var categoryId = categoryJson.RootElement
                .GetProperty("categoryID")
                .GetInt32();

            // 5. Register a Participant.
            var participantRegistration =
                await _client.PostAsJsonAsync(
                    "/api/auth/register/participant",
                    new
                    {
                        FirstName = "Test",
                        LastName = "Participant",
                        CellNumber = "0712345678",
                        Address = "Test Address",
                        Email = participantEmail,
                        Password = password
                    });

            Assert.Equal(
                HttpStatusCode.Created,
                participantRegistration.StatusCode);

            // 6. Log in as the Participant.
            var participantToken = await LoginAsync(
                participantEmail,
                password);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer", participantToken);

            // 7. Enrol in the Event.
            var enrolmentResponse = await _client.PostAsJsonAsync(
                $"/api/events/{eventId}/enrolments",
                new
                {
                    CategoryId = categoryId
                });

            // Assert: the enrolment should be created.
            Assert.Equal(
                HttpStatusCode.Created,
                enrolmentResponse.StatusCode);

            using var enrolmentJson = JsonDocument.Parse(
                await enrolmentResponse.Content.ReadAsStringAsync());

            var enrolment = enrolmentJson.RootElement;

            Assert.Equal(
                eventId,
                enrolment.GetProperty("eventID").GetInt32());

            Assert.Equal(
                categoryId,
                enrolment.GetProperty("categoryId").GetInt32());
        }

        private async Task<string> LoginAsync(
            string email,
            string password)
        {
            var response = await _client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    AuthenticationRole = "Participant",
                    Email = email,
                    Password = password
                });

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            using var json = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync());

            return json.RootElement
                .GetProperty("accessToken")
                .GetString()!;
        }
        [Fact]
        public async Task Organiser_CannotEnrolInEvent()
        {
            // Arrange: Register an Organiser
            var email = $"organiser_{Guid.NewGuid():N}@example.com";
            var password = "TestPassword123!";

            var registrationResponse = await _client.PostAsJsonAsync(
                "/api/auth/register/organiser",
                new
                {
                    FirstName = "Test",
                    LastName = "Organiser",
                    Address = "Test Address",
                    Email = email,
                    Password = password
                });

            Assert.Equal(
                HttpStatusCode.Created,
                registrationResponse.StatusCode);

            // Arrange: Log in as the Organiser
            var loginResponse = await _client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    AuthenticationRole = "Organiser",
                    Email = email,
                    Password = password
                });

            Assert.Equal(
                HttpStatusCode.OK,
                loginResponse.StatusCode);

            var loginData =
                await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

            Assert.NotNull(loginData);
            Assert.False(string.IsNullOrWhiteSpace(loginData.AccessToken));

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    loginData.AccessToken);

            // Act: Attempt to enrol as an Organiser
            var enrolmentResponse = await _client.PostAsJsonAsync(
                "/api/events/1/enrolments",
                new
                {
                    CategoryId = 1
                });

            // Assert
            Assert.Equal(
                HttpStatusCode.Forbidden,
                enrolmentResponse.StatusCode);
        }

        private class LoginResponse
        {
            public string AccessToken { get; set; } = string.Empty;
        }
    }
}
