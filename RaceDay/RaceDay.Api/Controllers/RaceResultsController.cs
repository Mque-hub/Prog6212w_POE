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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/enrolments/1/results
        [HttpPost("enrolments/{enrolmentId:int}/results")]
        [Authorize(Roles = nameof(Role.Organiser))]
        public async Task<ActionResult<ResultDto>> CreateResult(
            int enrolmentId,
            ResultRequest request)
        {
            var accountId = GetAccountId();

            if (accountId == null)
                return Unauthorized();

            if (request.FinishTime < TimeSpan.Zero ||
                request.FinishPosition < 1)
            {
                return BadRequest(
                    "Finish time must not be negative and finish position must be at least 1.");
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == accountId.Value);

            if (account?.OrganiserId == null)
                return Forbid();

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentId == enrolmentId &&
                    e.Event.OrganiserID == account.OrganiserId.Value);

            if (enrolment == null)
                return NotFound(
                    "Enrolment not found, or you do not own its event.");

            var existingResult = await _context.RaceResults
                .AnyAsync(r => r.EnrolmentID == enrolmentId);

            if (existingResult)
                return Conflict(
                    "A result already exists for this enrolment.");

            var result = new RaceResults
            {
                EnrolmentID = enrolmentId,
                FinishTime = request.FinishTime,
                FinishPosition = request.FinishPosition,
                PerformanceDetails = request.PerformanceDetails
            };

            _context.RaceResults.Add(result);

            var awardNames = (request.Awards ?? new List<string>())
             .Where(a => !string.IsNullOrWhiteSpace(a))
             .Select(a => a.Trim())
             .Distinct(StringComparer.OrdinalIgnoreCase)
             .ToList();

            foreach (var awardName in awardNames)
            {
                result.Awards.Add(new Awards
                {
                    AwardsDetails = awardName
                });
            }

            await _context.SaveChangesAsync();

            return StatusCode(
                StatusCodes.Status201Created,
                new ResultDto
                {
                    EnrolmentID = result.EnrolmentID,
                    FinishTime = result.FinishTime,
                    FinishPosition = result.FinishPosition,
                    PerformanceDetails = result.PerformanceDetails,
                    Awards = result.Awards
                    .Select(a => a.AwardsDetails)
                    .ToList()
                });
        }

        // PUT: api/results/1
        [HttpPut("results/{id:int}")]
        [Authorize(Roles = nameof(Role.Organiser))]
        public async Task<IActionResult> UpdateResult(
            int id,
            ResultRequest request)
        {
            var accountId = GetAccountId();

            if (accountId == null)
                return Unauthorized();

            if (request.FinishTime < TimeSpan.Zero ||
                request.FinishPosition < 1)
            {
                return BadRequest(
                    "Finish time must not be negative and finish position must be at least 1.");
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == accountId.Value);

            if (account?.OrganiserId == null)
                return Forbid();

            var result = await _context.RaceResults
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Event)
                .Include(r => r.Awards)
                .FirstOrDefaultAsync(r =>
                    r.RaceResultsID == id &&
                    r.Enrolment.Event.OrganiserID ==
                        account.OrganiserId.Value);

            if (result == null)
                return NotFound(
                    "Result not found, or you do not own its event.");

            result.FinishTime = request.FinishTime;
            result.FinishPosition = request.FinishPosition;
            result.PerformanceDetails = request.PerformanceDetails;

            // This API version supports one to many awards.
            var awardNames = (request.Awards ?? new List<string>())
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

            foreach (var existingAward in result.Awards.ToList())
            {
                _context.Awards.Remove(existingAward);
            }

            result.Awards.Clear();

            foreach (var awardName in awardNames)
            {
                result.Awards.Add(new Awards
                {
                    AwardsDetails = awardName
                });
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/results/my
        [HttpGet("results/my")]
        [Authorize(Roles = nameof(Role.Participant))]
        public async Task<ActionResult<IEnumerable<ResultDto>>>
            GetMyResults()
        {
            var accountId = GetAccountId();

            if (accountId == null)
                return Unauthorized();

            var results = await _context.RaceResults
                .AsNoTracking()
                .Where(r => r.Enrolment.AccountID == accountId.Value)
                .Select(r => new ResultDto
                {
                    EnrolmentID = r.EnrolmentID,
                    FinishTime = r.FinishTime,
                    FinishPosition = r.FinishPosition,
                    PerformanceDetails = r.PerformanceDetails,
                    Awards = r.Awards
                    .Select(a => a.AwardsDetails)
                    .ToList()
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/events/1/results
        [HttpGet("events/{eventId:int}/results")]
        [Authorize(Roles = nameof(Role.Organiser))]
        public async Task<ActionResult<IEnumerable<ResultDto>>>
            GetEventResults(int eventId)
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

            var results = await _context.RaceResults
                .AsNoTracking()
                .Where(r => r.Enrolment.EventID == eventId)
                .OrderBy(r => r.FinishPosition)
                .Select(r => new ResultDto
                {
                    EnrolmentID = r.EnrolmentID,
                    FinishTime = r.FinishTime,
                    FinishPosition = r.FinishPosition,
                    PerformanceDetails = r.PerformanceDetails,
                    Awards = r.Awards
                        .Select(a => a.AwardsDetails)
                        .ToList()
                })
                .ToListAsync();

            return Ok(results);
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
