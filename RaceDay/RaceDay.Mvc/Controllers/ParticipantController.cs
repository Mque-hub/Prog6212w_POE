using System.Net.Http.Json;
using RaceDay.Mvc.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts.DTOs;
using RaceDay.Mvc.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

// Keep the using statement for the namespace
// containing RaceDayApiClient if it is in another namespace.

namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly RaceDayApiClient _api;

        // Constructor
        public ParticipantController(RaceDayApiClient api)
        {
            _api = api;
        }

        // Check whether the logged-in user is a Participant
        private bool IsParticipant()
        {
            return string.Equals(
                HttpContext.Session.GetString("Role"),
                "Participant",
                StringComparison.OrdinalIgnoreCase);
        }

        private readonly IHttpClientFactory _httpClientFactory;

        public ParticipantController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        // Participant home page
        public async Task<IActionResult> Index(string? type, string? search)
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(
                HttpMethod.Get, "api/events");

            var events = await _api.ReadAsync<List<EventDto>>(response)
                         ?? new List<EventDto>();

            // Display upcoming events only
            events = events
            .Where(e => e.EventDate >= DateOnly.FromDateTime(DateTime.Today))
            .ToList();

            // Filter by event type
            if (!string.IsNullOrWhiteSpace(type))
            {
                events = events
                    .Where(e => e.EventType == type)
                    .ToList();
            }

            // Search by event name or location
            if (!string.IsNullOrWhiteSpace(search))
            {
                events = events.Where(e =>
                    (e.EventName != null &&
                     e.EventName.Contains(search,
                         StringComparison.OrdinalIgnoreCase)) ||

                    (e.Location != null &&
                     e.Location.Contains(search,
                         StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            return View(events);
        }
        public async Task<IActionResult> MyEnrolments()
        {
            // Check whether a participant is logged in
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            // Connect to the existing API
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            // Forward the participant's authentication cookie
            var cookie = Request.Headers.Cookie.ToString();

            if (!string.IsNullOrEmpty(cookie))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(
                    "Cookie", cookie);
            }

            // Call the Part 2 API endpoint
            var response = await client.GetAsync("api/enrolments/my");

            if (!response.IsSuccessStatusCode)
            {
                return View(new MyEnrolmentsVm());
            }

            // Convert the API response into DTO objects
            var enrolments = await response.Content
                .ReadFromJsonAsync<List<EnrolmentDto>>();

            var model = new MyEnrolmentsVm
            {
                Enrolments = enrolments ?? new List<EnrolmentDto>()
            };

            return View(model);
        }
        
    }
}
