using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;
using RaceDay.Contracts.DTOs;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/events/1/enrolments
        [HttpPost("events/{eventId:int}/enrolments")]
        [Authorize(Roles = nameof(Role.Participant))]
        public async Task<ActionResult<EnrolmentDto>> CreateEnrolment(
            int eventId,
            EnrolmentRequest request)
        {
            var accountId = GetAccountId();

            if (accountId == null)
                return Unauthorized();

            var eventExists = await _context.Events
                .AnyAsync(e => e.EventID == eventId);

            if (!eventExists)
                return NotFound("Event not found.");

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryID == request.CategoryId &&
                    c.EventID == eventId);

            if (category == null)
                return BadRequest(
                    "The selected category does not belong to this event.");

            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.AccountID == accountId.Value &&
                    e.EventID == eventId);

            if (alreadyEnrolled)
                return Conflict(
                    "You are already enrolled in this event.");

            var enrolment = new Enrolment
            {
                AccountID = accountId.Value,
                EventID = eventId,
                CategoryId = request.CategoryId,
                EnrolmentDate = DateTime.UtcNow
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            var result = new EnrolmentDto
            {
                EnrolmentId = enrolment.EnrolmentId,
                AccountId = enrolment.AccountID,
                EventID = enrolment.EventID,
                CategoryId = enrolment.CategoryId,
                EnrolmentDate = enrolment.EnrolmentDate
            };

            return StatusCode(
                StatusCodes.Status201Created, result);
        }

        // GET: api/enrolments/my
        [HttpGet("enrolments/my")]
        [Authorize(Roles = nameof(Role.Participant))]
        public async Task<ActionResult<IEnumerable<EnrolmentDto>>>
            GetMyEnrolments()
        {
            var accountId = GetAccountId();

            if (accountId == null)
                return Unauthorized();

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.AccountID == accountId.Value)
                .Select(e => new EnrolmentDto
                {
                    EnrolmentId = e.EnrolmentId,
                    AccountId = e.AccountID,
                    EventID = e.EventID,
                    CategoryId = e.CategoryId,
                    EnrolmentDate = e.EnrolmentDate
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/events/1/enrolments
        [HttpGet("events/{eventId:int}/enrolments")]
        [Authorize(Roles = nameof(Role.Organiser))]
        public async Task<ActionResult<IEnumerable<EnrolmentDto>>>
            GetEventEnrolments(int eventId)
        {
            var accountId = GetAccountId();

            if (accountId == null)
                return Unauthorized();

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    a.AccountId == accountId.Value);

            if (account?.OrganiserId == null)
                return Forbid();

            var eventExists = await _context.Events
                .AnyAsync(e =>
                    e.EventID == eventId &&
                    e.OrganiserID == account.OrganiserId.Value);

            if (!eventExists)
                return NotFound(
                    "Event not found, or you do not own this event.");

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.EventID == eventId)
                .Select(e => new EnrolmentDto
                {
                    EnrolmentId = e.EnrolmentId,
                    AccountId = e.AccountID,
                    EventID = e.EventID,
                    CategoryId = e.CategoryId,
                    EnrolmentDate = e.EnrolmentDate
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // DELETE: api/enrolments/1
        [HttpDelete("enrolments/{id:int}")]
        [Authorize(Roles = nameof(Role.Participant))]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            var accountId = GetAccountId();

            if (accountId == null)
                return Unauthorized();

            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentId == id &&
                    e.AccountID == accountId.Value);

            if (enrolment == null)
                return NotFound(
                    "Enrolment not found, or it does not belong to you.");

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private int? GetAccountId()
        {
            var value = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return int.TryParse(value, out var accountId)
                ? accountId
                : null;
        }
    }
}
