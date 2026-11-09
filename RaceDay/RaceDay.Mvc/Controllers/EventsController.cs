
using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts.DTOs;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;
using RaceDay.Mvc.Views;

namespace RaceDay.Mvc.Controllers
{
    public class EventsController : Controller
    {
        private readonly RaceDayApiClient _api;

        public EventsController(RaceDayApiClient api)
        {
            _api = api;
        }

        // Checks whether the logged-in user is an organiser
        private bool IsOrganiser()
        {
            return string.Equals(
                HttpContext.Session.GetString("Role"),
                "Organiser",
                StringComparison.OrdinalIgnoreCase);
        }

        private bool IsParticipant()
        {
            return string.Equals(
                HttpContext.Session.GetString("Role"),
                "Participant",
                StringComparison.OrdinalIgnoreCase);
        }

        // GET: /Events/Events
        // Retrieves events belonging to the logged-in organiser
        [HttpGet]
        public async Task<IActionResult> Events()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(
                HttpMethod.Get,
                "api/events/mine");

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "",
                    "Could not retrieve your events.");

                return View(new List<EventDto>());
            }

            var events = await _api.ReadAsync<List<EventDto>>(response)
                         ?? new List<EventDto>();

            return View(events);
        }

        // GET: Displays all available events to Participants
        [HttpGet]
        public async Task<IActionResult> ParticipantEvents()
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Auth");
            }

            var response = await _api.SendAsync(
                HttpMethod.Get,
                "api/events");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Could not retrieve available events.";
                return View("ParticipantEvents", new List<EventDto>());
            }

            var events = await _api.ReadAsync<List<EventDto>>(response)
                         ?? new List<EventDto>();

            return View("ParticipantEvents", events);
        }
        // GET: Displays the Create Event form
        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            return View();
        }

        // POST: Sends the new event to the API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(
    EventRequest model, IFormFile? banner)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View(model);

            var response = await _api.SendAsync(
    HttpMethod.Post,
    "api/events",
    model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "",
                    await response.Content.ReadAsStringAsync());

                return View(model);
            }

            // Read the newly created event
            var createdEvent =
                await _api.ReadAsync<EventDto>(response);

            if (createdEvent == null)
            {
                ModelState.AddModelError(
                    "",
                    "Event was created, but its details could not be retrieved.");

                return View(model);
            }

            // Upload banner if one was selected
            if (banner != null && banner.Length > 0)
            {
                var uploadResponse = await _api.UploadFileAsync(
                    $"api/events/{createdEvent.EventID}/banner",
                    banner);

                if (!uploadResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] =
                        "Event created, but banner upload failed: " +
                        await uploadResponse.Content.ReadAsStringAsync();

                    return RedirectToAction(nameof(Events));
                }
            }

            TempData["Success"] = "Event created successfully.";

            return RedirectToAction(nameof(Events));
        }
        // GET: Displays categories belonging to a selected event
        [HttpGet]
        public async Task<IActionResult> Categories(int eventId)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            if (eventId <= 0)
                return BadRequest();

            var response = await _api.SendAsync(
                HttpMethod.Get,
                $"api/events/{eventId}/categories");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Could not retrieve event categories.";
                return RedirectToAction(nameof(Events));
            }

            var categories =
                await _api.ReadAsync<List<CategoryDto>>(response)
                ?? new List<CategoryDto>();

            ViewBag.EventId = eventId;

            return View(categories);
        }

        // GET: Displays participants enrolled in a selected event
        [HttpGet]
        public async Task<IActionResult> Participants(int eventId)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            if (eventId <= 0)
                return BadRequest();

            // Call the API to retrieve event participants
            var response = await _api.SendAsync(
                HttpMethod.Get,
                $"api/events/{eventId}/participants");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Could not retrieve event participants.";
                return RedirectToAction(nameof(Events));
            }

            // Read the API response
            var participants =
                await _api.ReadAsync<List<EventParticipantDto>>(response)
                ?? new List<EventParticipantDto>();

            // Convert DTOs into MVC ViewModels
            var model = participants.Select(p => new EventParticipantViewModel
            {
                ParticipantName = p.ParticipantName,
                CategoryName = p.CategoryName,
                EnrolmentDate = p.EnrolmentDate,
                Status = null
            }).ToList();

            return View(model);
        }
        // GET: Displays events entered by the participant
        [HttpGet]
        public async Task<IActionResult> MyEvents()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(
                HttpMethod.Get,
                "api/enrolments/my");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Could not retrieve your events.";
                return View(new List<EnrolmentDto>());
            }

            var enrolments =
                await _api.ReadAsync<List<EnrolmentDto>>(response)
                ?? new List<EnrolmentDto>();

            return View(enrolments);
        }
        // GET: Displays full event details and available categories
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            // Retrieve event details
            var eventResponse = await _api.SendAsync(
                HttpMethod.Get,
                $"api/events/{id}");

            if (!eventResponse.IsSuccessStatusCode)
                return NotFound();

            var eventDetails =
                await _api.ReadAsync<EventDto>(eventResponse);

            // Retrieve available categories
            var categoryResponse = await _api.SendAsync(
                HttpMethod.Get,
                $"api/events/{id}/categories");

            if (!categoryResponse.IsSuccessStatusCode)
                return NotFound();

            var categories =
                await _api.ReadAsync<List<CategoryDto>>(categoryResponse)
                ?? new List<CategoryDto>();

            ViewBag.Categories = categories;

            return View(eventDetails);
        }
        // POST: Allows a participant to enter an event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnterEvent(int eventId, int categoryId)
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Auth");

            var request = new EnrolmentRequest
            {
                CategoryId = categoryId
            };

            var response = await _api.SendAsync(
                HttpMethod.Post,
                $"api/events/{eventId}/enrolments",
                request
            );

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();

                return RedirectToAction(nameof(Details),
                    new { id = eventId });
            }

            TempData["Success"] = "You have entered the event successfully.";

            return RedirectToAction(nameof(MyEvents));

        }

    }
}
