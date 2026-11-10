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



namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly RaceDayApiClient _api;

        
        public ParticipantController(RaceDayApiClient api)
        {
            _api = api;
        }

        
        private bool IsParticipant()
        {
            return string.Equals(
                HttpContext.Session.GetString("Role"),
                "Participant",
                StringComparison.OrdinalIgnoreCase);
        }


        // Participant home page

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

           
            var eventsResponse = await _api.SendAsync(
                HttpMethod.Get, "api/events");

            var events = await _api.ReadAsync<List<EventDto>>(
                eventsResponse) ?? new List<EventDto>();

            
            var enrolmentsResponse = await _api.SendAsync(
                HttpMethod.Get, "api/enrolments/my");

            var enrolments = await _api.ReadAsync<List<EnrolmentDto>>(
                enrolmentsResponse) ?? new List<EnrolmentDto>();

            
            var resultsResponse = await _api.SendAsync(
                HttpMethod.Get, "api/results/my");

            var results = await _api.ReadAsync<List<ResultDto>>(
                resultsResponse) ?? new List<ResultDto>();

            
            var today = DateOnly.FromDateTime(DateTime.Today);

            var upcomingEvents = events
                .Where(e => e.EventDate >= today)
                .OrderBy(e => e.EventDate)
                .ToList();

            
            var model = new ParticipantDashboardVm
            {
                TotalUpcomingEvents = upcomingEvents.Count,

                TotalEnrolments = enrolments.Count,

                CompletedResults = results.Count,

                BestFinishPosition = results
                    .Where(r => r.FinishPosition > 0)
                    .Select(r => (int?)r.FinishPosition)
                    .Min(),

                UpcomingEvents = upcomingEvents
                    .Take(3)
                    .ToList(),

                RecentResults = results
                    .OrderByDescending(r => r.EventDate)
                    .Take(3)
                    .ToList()
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> AvailableEvents(
            string? type, string? search)
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(
                HttpMethod.Get, "api/events");

            var events = await _api.ReadAsync<List<EventDto>>(response)
                         ?? new List<EventDto>();

            
            events = events
                .Where(e => e.EventDate >=
                    DateOnly.FromDateTime(DateTime.Today))
                .ToList();

            
            if (!string.IsNullOrWhiteSpace(type))
            {
                events = events
                    .Where(e => e.EventType == type)
                    .ToList();
            }

            
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

            return View("AvailableEvents", events);
        }

        public async Task<IActionResult> MyEnrolments()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(
                HttpMethod.Get, "api/enrolments/my");

            var enrolments = await _api.ReadAsync<List<EnrolmentDto>>(response);

            var model = new MyEnrolmentsVm
            {
                Enrolments = enrolments ?? new List<EnrolmentDto>()
            };

            return View(model);
        }

        public async Task<IActionResult> MyResults()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(
                HttpMethod.Get, "api/results/my");

            var results = await _api.ReadAsync<List<ResultDto>>(response);

            var model = new ResultsVm
            {
                Results = results ?? new List<ResultDto>()
            };

            return View(model);
        }


        public async Task<IActionResult> Profile()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(
                HttpMethod.Get,
                "api/profiles/me");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ProfileError"] =
                    $"Could not load your profile. API returned: {(int)response.StatusCode}";

                return View(new ParticipantProfileDto());
            }

            var profile =
                await _api.ReadAsync<ParticipantProfileDto>(response);

            if (profile == null)
                profile = new ParticipantProfileDto();

            return View(profile);
        }


        // POST: /Participant/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(
            UpdateParticipantProfileRequestDTO request)
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(
                HttpMethod.Put,
                "api/profiles/me/participant",
                request);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ProfileError"] =
                    $"Could not update your profile. API returned: {(int)response.StatusCode}";
            }
            else
            {
                TempData["ProfileSuccess"] =
                    "Your profile has been updated successfully.";
            }

            return RedirectToAction(nameof(Profile));
        }


        // POST: /Participant/UploadProfilePicture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            if (file == null || file.Length == 0)
            {
                TempData["ProfileError"] =
                    "Please select an image to upload.";

                return RedirectToAction(nameof(Profile));
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                TempData["ProfileError"] =
                    "Image size must not exceed 5 MB.";

                return RedirectToAction(nameof(Profile));
            }

            var allowedTypes = new[]
            {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

            if (!allowedTypes.Contains(
                file.ContentType.ToLowerInvariant()))
            {
                TempData["ProfileError"] =
                    "Only JPG, PNG and WEBP images are allowed.";

                return RedirectToAction(nameof(Profile));
            }

            var response = await _api.UploadFileAsync(
                "api/profiles/me/participant/profile-picture",
                file);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ProfileError"] =
                    $"Could not upload your profile picture. API returned: {(int)response.StatusCode}";
            }
            else
            {
                TempData["ProfileSuccess"] =
                    "Your profile picture has been updated successfully.";
            }

            return RedirectToAction(nameof(Profile));
        }

    }
}
