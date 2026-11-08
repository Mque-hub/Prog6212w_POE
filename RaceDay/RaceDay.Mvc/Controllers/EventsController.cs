
using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts.DTOs;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

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
        public async Task<IActionResult> CreateEvent(EventRequest model)
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

            TempData["Success"] = "Event created successfully.";

            return RedirectToAction(nameof(Events));
        }
    }
}
