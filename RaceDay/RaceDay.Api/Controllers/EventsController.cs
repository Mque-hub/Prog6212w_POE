using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;
using RaceDay.Contracts.DTOs;
using System.Globalization;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    
        [ApiController]
        [Route("api/events")]
        public class EventsController : ControllerBase
        {
            private readonly RaceDayDbContext _context;

            public EventsController(RaceDayDbContext context)
            {
                _context = context;
            }

            // GET: api/events
            [HttpGet]
            [AllowAnonymous] // lets anyone browse events without logging in
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
            {
                var events = await _context.Events
                    .AsNoTracking()
                    .ToListAsync();

                var result = events.Select(e => new EventDto
                {
                    OrganiserID = e.OrganiserID,
                    EventName = e.EventName,
                    Description = e.Description,
                    EventType = e.EventType,
                    EventDate = e.EventDate,
                    EventDuration = e.EventDuration,
                    Location = e.Location,
                    Distance = e.Distance
                }).ToList();

                return Ok(result);
            }

            // GET: api/events/1
            [HttpGet("{id:int}")]
            [AllowAnonymous]
            public async Task<ActionResult<EventDto>> GetEventById(int id)
            {
                var e = await _context.Events
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.EventID == id);

                if (e == null)
                    return NotFound();

                var result = new EventDto
                {
                    OrganiserID = e.OrganiserID,
                    EventName = e.EventName,
                    Description = e.Description,
                    EventType = e.EventType,
                    EventDate = e.EventDate,
                    EventDuration = e.EventDuration,
                    Location = e.Location,
                    Distance = e.Distance
                };

                return Ok(result);
            }

            // POST: api/events
            [HttpPost]
            [Authorize(Roles = nameof(Role.Organiser))] // requires a valid JWT whose role is Organiser
        public async Task<ActionResult<EventDto>> CreateEvent(
                EventRequest request)
            {
                var accountIdValue = User.FindFirstValue(
                    ClaimTypes.NameIdentifier); // retrieves the account ID from the token.

            if (!int.TryParse(accountIdValue, out var accountId))
                    return Unauthorized();

                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountId == accountId);

                if (account?.OrganiserId == null)
                    return Forbid();

                var newEvent = new Event
                {
                    OrganiserID = account.OrganiserId.Value,
                    EventName = request.EventName,
                    Description = request.Description,
                    EventType = request.EventType,
                    EventDate = request.EventDate,
                    EventDuration = request.EventDuration,
                    Location = request.Location,
                    Distance = request.Distance.ToString(
                        CultureInfo.InvariantCulture)
                };

                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();

                var result = new EventDto
                {
                    OrganiserID = newEvent.OrganiserID,
                    EventName = newEvent.EventName,
                    Description = newEvent.Description,
                    EventType = newEvent.EventType,
                    EventDate = newEvent.EventDate,
                    EventDuration = newEvent.EventDuration,
                    Location = newEvent.Location,
                    Distance = newEvent.Distance
                };

                return CreatedAtAction(
                    nameof(GetEventById),
                    new { id = newEvent.EventID },
                    result); // returns 201 Created when an event is successfully created.
        }

            // PUT: api/events/1
            [HttpPut("{id:int}")]
            [Authorize(Roles = nameof(Role.Organiser))]
            public async Task<IActionResult> UpdateEvent(
                int id,
                UpdateEventRequest request)
            {
                var accountIdValue = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (!int.TryParse(accountIdValue, out var accountId))
                    return Unauthorized();

                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountId == accountId);

                if (account?.OrganiserId == null)
                    return Forbid();

                var existingEvent = await _context.Events
                    .FirstOrDefaultAsync(e =>
                        e.EventID == id &&
                        e.OrganiserID == account.OrganiserId.Value);

                if (existingEvent == null)
                    return NotFound();

                existingEvent.EventName = request.EventName;
                existingEvent.Description = request.Description;
                existingEvent.EventType = request.EventType;
                existingEvent.EventDate = request.EventDate;
                existingEvent.EventDuration = request.EventDuration;
                existingEvent.Location = request.Location;
                existingEvent.Distance = request.Distance;

                await _context.SaveChangesAsync();

                return NoContent(); //returns 204 No Content when an update or deletion succeeds.
        }

            // DELETE: api/events/1
            [HttpDelete("{id:int}")]
            [Authorize(Roles = nameof(Role.Organiser))]
            public async Task<IActionResult> DeleteEvent(int id)
            {
                var accountIdValue = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (!int.TryParse(accountIdValue, out var accountId))
                    return Unauthorized();

                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountId == accountId);

                if (account?.OrganiserId == null)
                    return Forbid();

                var existingEvent = await _context.Events
                    .FirstOrDefaultAsync(e =>
                        e.EventID == id &&
                        e.OrganiserID == account.OrganiserId.Value);

                if (existingEvent == null)
                    return NotFound();

                _context.Events.Remove(existingEvent);
                await _context.SaveChangesAsync();

                return NoContent();
            }
        }
    
}

