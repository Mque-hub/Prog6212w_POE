using Microsoft.AspNetCore.Mvc.Testing;
using RaceDay.Api.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RaceDay.Api.Tests.Tests
{
    public class EventManagementTests
        : IClassFixture<RaceDayApiFactory>
    {
        private readonly HttpClient _client;

        public EventManagementTests(RaceDayApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task UnauthenticatedUser_CannotCreateEvent()
        {
            // Arrange
            var request = new
            {
                EventName = "Test Race",
                Description = "A test running event",
                EventType = "Running",
                EventDate = "2026-11-15",
                EventDuration = "02:00",
                Location = "Johannesburg",
                Distance = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/api/events",
                request);

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }
        [Fact]
        public async Task Participant_CannotCreateEvent()
        {
            // Arrange: register a Participant.
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

            // Log in to obtain the Participant's JWT.
            var loginResponse = await _client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    AuthenticationRole = "Participant",
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
                    "Bearer", loginData.AccessToken);

            // Act: try to create an Event as a Participant.
            var eventResponse = await _client.PostAsJsonAsync(
                "/api/events",
                new
                {
                    EventName = "Test Event",
                    Description = "Event created for testing",
                    EventType = "Running",
                    EventDate = DateOnly.FromDateTime(
                        DateTime.Today.AddDays(30)),
                    EventDuration = TimeSpan.FromHours(2),
                    Location = "Test Venue",
                    Distance = 5.0m
                });

            // Assert: only Organisers can create Events.
            Assert.Equal(
                HttpStatusCode.Forbidden,
                eventResponse.StatusCode);
        }
        [Fact]
        public async Task Organiser_CanCreateEvent()
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

            // Act: Create an Event
            var eventResponse = await _client.PostAsJsonAsync(
                "/api/events",
                new
                {
                    EventName = "Test Running Event",
                    Description = "An event created by an Organiser",
                    EventType = "Running",
                    EventDate = DateOnly.FromDateTime(
                        DateTime.Today.AddDays(30)),
                    EventDuration = TimeSpan.FromHours(2),
                    Location = "Johannesburg",
                    Distance = 5.0m
                });

            // Assert
            Assert.Equal(
                HttpStatusCode.Created,
                eventResponse.StatusCode);

            Assert.NotNull(eventResponse.Headers.Location);
        }
        private class LoginResponse
        {
            public string AccessToken { get; set; } = string.Empty;
        }
    

    }
}
