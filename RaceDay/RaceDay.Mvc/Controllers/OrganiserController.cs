using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;
using RaceDay.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly RaceDayApiClient _api;

        public OrganiserController(RaceDayApiClient api)
        {
            _api = api;
        }

        private bool IsOrganiser()
        {
            return string.Equals(
                HttpContext.Session.GetString("Role"),
                "Organiser",
                StringComparison.OrdinalIgnoreCase);
        }

        // GET: /Organiser/Index
        public async Task<IActionResult> Index()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(
                HttpMethod.Get,
                "api/events/mine");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] =
                    "Could not retrieve organiser events. API returned: " +
                    (int)response.StatusCode;

                return View(new OrganiserDashboardVm());
            }

            var events = await _api.ReadAsync<List<EventDto>>(response)
                         ?? new List<EventDto>();

            var today = DateOnly.FromDateTime(DateTime.Today);

            int upcomingEvents = events.Count(e =>
                e.EventDate >= today);

            int eventLocations = events
                .Where(e => !string.IsNullOrWhiteSpace(e.Location))
                .Select(e => e.Location!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            int totalEnrolments = 0;

            foreach (var eventItem in events)
            {
                var participantsResponse = await _api.SendAsync(
                    HttpMethod.Get,
                    $"api/events/{eventItem.EventID}/participants");

                if (participantsResponse.IsSuccessStatusCode)
                {
                    var participants =
                        await _api.ReadAsync<List<EventParticipantDto>>(
                            participantsResponse);

                    totalEnrolments += participants?.Count ?? 0;
                }
            }

            var model = new OrganiserDashboardVm
            {
                Events = events,
                TotalEvents = events.Count,
                TotalEnrolments = totalEnrolments,
                UpcomingEvents = upcomingEvents,
                EventLocations = eventLocations
            };

            return View(model);
        }


        // GET: /Organiser/Enrolments
        public async Task<IActionResult> Enrolments(int? eventId)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            var model = new OrganiserEnrolmentsVm();

            // Retrieve events belonging to the logged-in organiser.
            var eventsResponse = await _api.SendAsync(
                HttpMethod.Get,
                "api/events/mine");

            if (!eventsResponse.IsSuccessStatusCode)
            {
                TempData["EnrolmentsError"] =
                    $"Could not retrieve your events. API returned: {(int)eventsResponse.StatusCode}";

                return View(model);
            }

            model.Events =
                await _api.ReadAsync<List<EventDto>>(eventsResponse)
                ?? new List<EventDto>();

            
            if (!eventId.HasValue)
                return View(model);

            // Confirm that the selected event belongs to this organiser.
            var selectedEvent = model.Events.FirstOrDefault(
                e => e.EventID == eventId.Value);

            if (selectedEvent == null)
            {
                TempData["EnrolmentsError"] =
                    "The selected event could not be found.";

                return View(model);
            }

            model.SelectedEventId = selectedEvent.EventID;
            model.SelectedEventName = selectedEvent.EventName;

            // Retrieve participants enrolled in the selected event.
            var participantsResponse = await _api.SendAsync(
                HttpMethod.Get,
                $"api/events/{selectedEvent.EventID}/participants");

            if (!participantsResponse.IsSuccessStatusCode)
            {
                TempData["EnrolmentsError"] =
                    $"Could not retrieve participants. API returned: {(int)participantsResponse.StatusCode}";

                return View(model);
            }

            model.Participants =
                await _api.ReadAsync<List<EventParticipantDto>>(
                    participantsResponse)
                ?? new List<EventParticipantDto>();

            return View(model);
        }


        // GET: /Organiser/Results
        public async Task<IActionResult> Results(int? eventId)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            var model = new OrganiserResultsVm();

            // Retrieve events belonging to the logged-in organiser.
            var eventsResponse = await _api.SendAsync(
                HttpMethod.Get,
                "api/events/mine");

            if (!eventsResponse.IsSuccessStatusCode)
            {
                TempData["ResultsError"] =
                    $"Could not retrieve your events. API returned: {(int)eventsResponse.StatusCode}";

                return View(model);
            }

            model.Events =
                await _api.ReadAsync<List<EventDto>>(eventsResponse)
                ?? new List<EventDto>();

            // No event selected yet.
            if (!eventId.HasValue)
                return View(model);

            // Verify that the selected event belongs to this organiser.
            var selectedEvent = model.Events.FirstOrDefault(
                e => e.EventID == eventId.Value);

            if (selectedEvent == null)
            {
                TempData["ResultsError"] =
                    "The selected event could not be found.";

                return View(model);
            }

            model.SelectedEventId = selectedEvent.EventID;
            model.SelectedEventName = selectedEvent.EventName;

            // Retrieve participants enrolled in this event.
            var participantsResponse = await _api.SendAsync(
                HttpMethod.Get,
                $"api/events/{selectedEvent.EventID}/participants");

            if (!participantsResponse.IsSuccessStatusCode)
            {
                TempData["ResultsError"] =
                    $"Could not retrieve participants. API returned: {(int)participantsResponse.StatusCode}";

                return View(model);
            }

            model.Participants =
                await _api.ReadAsync<List<EventParticipantDto>>(
                    participantsResponse)
                ?? new List<EventParticipantDto>();

            // Retrieve results already recorded for this event.
            var resultsResponse = await _api.SendAsync(
                HttpMethod.Get,
                $"api/events/{selectedEvent.EventID}/results");

            if (!resultsResponse.IsSuccessStatusCode)
            {
                TempData["ResultsError"] =
                    $"Could not retrieve event results. API returned: {(int)resultsResponse.StatusCode}";

                return View(model);
            }

            model.Results =
                await _api.ReadAsync<List<ResultDto>>(resultsResponse)
                ?? new List<ResultDto>();

            return View(model);
        }

        // POST: /Organiser/CreateResult
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateResult(
            int eventId,
            int enrolmentId,
            string finishTime,
            int finishPosition,
            string? performanceDetails,
            string? awards)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            // Validate finish time.
            if (!TimeSpan.TryParse(
                finishTime,
                CultureInfo.InvariantCulture,
                out var parsedFinishTime) ||
                parsedFinishTime < TimeSpan.Zero)
            {
                TempData["ResultsError"] =
                    "Please enter a valid, non-negative finish time.";

                return RedirectToAction(
                    nameof(Results),
                    new { eventId });
            }

            // Validate finishing position.
            if (finishPosition < 1)
            {
                TempData["ResultsError"] =
                    "Finish position must be at least 1.";

                return RedirectToAction(
                    nameof(Results),
                    new { eventId });
            }

            // Convert the awards text into a list.
            var awardList = (awards ?? string.Empty)
                .Split(
                    new[] { ',', ';', '\n', '\r' },
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var request = new ResultRequest
            {
                FinishTime = parsedFinishTime,
                FinishPosition = finishPosition,
                PerformanceDetails = performanceDetails?.Trim()
                    ?? string.Empty,
                Awards = awardList
            };

            // Send the result to the API.
            var response = await _api.SendAsync(
                HttpMethod.Post,
                $"api/enrolments/{enrolmentId}/results",
                request);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ResultsError"] =
                    $"Could not record the result. API returned: {(int)response.StatusCode}";
            }
            else
            {
                TempData["ResultsSuccess"] =
                    "Participant result recorded successfully.";
            }

            return RedirectToAction(
                nameof(Results),
                new { eventId });
        }


        // POST: /Organiser/UpdateResult
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateResult(
            int eventId,
            int resultId,
            string finishTime,
            int finishPosition,
            string? performanceDetails,
            string? awards)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            // Validate finish time.
            if (!TimeSpan.TryParse(
                finishTime,
                CultureInfo.InvariantCulture,
                out var parsedFinishTime) ||
                parsedFinishTime < TimeSpan.Zero)
            {
                TempData["ResultsError"] =
                    "Please enter a valid, non-negative finish time.";

                return RedirectToAction(
                    nameof(Results),
                    new { eventId });
            }

            // Validate finishing position.
            if (finishPosition < 1)
            {
                TempData["ResultsError"] =
                    "Finish position must be at least 1.";

                return RedirectToAction(
                    nameof(Results),
                    new { eventId });
            }

            // Convert the awards text into a list.
            var awardList = (awards ?? string.Empty)
                .Split(
                    new[] { ',', ';', '\n', '\r' },
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var request = new ResultRequest
            {
                FinishTime = parsedFinishTime,
                FinishPosition = finishPosition,
                PerformanceDetails = performanceDetails?.Trim()
                    ?? string.Empty,
                Awards = awardList
            };

            // Send the updated result to the API.
            var response = await _api.SendAsync(
                HttpMethod.Put,
                $"api/results/{resultId}",
                request);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ResultsError"] =
                    $"Could not update the result. API returned: {(int)response.StatusCode}";
            }
            else
            {
                TempData["ResultsSuccess"] =
                    "Participant result updated successfully.";
            }

            return RedirectToAction(
                nameof(Results),
                new { eventId });
        }
        // GET: /Organiser/Profile
        public async Task<IActionResult> Profile()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(
                HttpMethod.Get,
                "api/profiles/me");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ProfileError"] =
                    $"Could not load your profile. API returned: {(int)response.StatusCode}";

                return View(new OrganiserProfileDTO());
            }

            var profile =
                await _api.ReadAsync<OrganiserProfileDTO>(response);

            return View(profile ?? new OrganiserProfileDTO());
        }

        // POST: /Organiser/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(
            UpdateOrganiserProfileRequestDTO request)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(
                HttpMethod.Put,
                "api/profiles/me/organiser",
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

        // POST: /Organiser/UploadProfilePicture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (!IsOrganiser())
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
                "api/profiles/me/organiser/profile-picture",
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
