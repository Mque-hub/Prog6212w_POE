using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts.DTOs;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/profiles")]
    [Authorize]
    public class ProfilesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ProfilesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/profiles/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var account = await GetCurrentAccountAsync();

            if (account == null)
                return Unauthorized();

            if (account.Role == Role.Participant)
            {
                if (account.ParticipantId == null)
                    return NotFound("Participant profile not found.");

                var participant = await _context.Participants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p =>
                        p.ParticipantId == account.ParticipantId.Value);

                if (participant == null)
                    return NotFound("Participant profile not found.");

                return Ok(new ParticipantProfileDto
                {
                    FirstName = participant.FirstName,
                    LastName = participant.LastName,
                    CellNumber = participant.CellNumber,
                    Address = participant.Address
                });
            }

            if (account.Role == Role.Organiser)
            {
                if (account.OrganiserId == null)
                    return NotFound("Organiser profile not found.");

                var organiser = await _context.Organisers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o =>
                        o.OrganiserId == account.OrganiserId.Value);

                if (organiser == null)
                    return NotFound("Organiser profile not found.");

                return Ok(new OrganiserProfileDTO
                {
                    FirstName = organiser.FirstName,
                    LastName = organiser.LastName,
                    Address = organiser.Address
                });
            }

            return Forbid();
        }

        // PUT: api/profiles/me/participant
        [HttpPut("me/participant")]
        [Authorize(Roles = nameof(Role.Participant))]
        public async Task<IActionResult> UpdateParticipantProfile(
            UpdateParticipantProfileRequestDTO request)
        {
            var account = await GetCurrentAccountAsync();

            if (account == null)
                return Unauthorized();

            if (account.ParticipantId == null)
                return NotFound("Participant profile not found.");

            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.CellNumber) ||
                string.IsNullOrWhiteSpace(request.Address))
            {
                return BadRequest(
                    "First name, last name, cell number, and address are required.");
            }

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p =>
                    p.ParticipantId == account.ParticipantId.Value);

            if (participant == null)
                return NotFound("Participant profile not found.");

            participant.FirstName = request.FirstName.Trim();
            participant.LastName = request.LastName.Trim();
            participant.CellNumber = request.CellNumber.Trim();
            participant.Address = request.Address.Trim();

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/profiles/me/organiser
        [HttpPut("me/organiser")]
        [Authorize(Roles = nameof(Role.Organiser))]
        public async Task<IActionResult> UpdateOrganiserProfile(
            UpdateOrganiserProfileRequestDTO request)
        {
            var account = await GetCurrentAccountAsync();

            if (account == null)
                return Unauthorized();

            if (account.OrganiserId == null)
                return NotFound("Organiser profile not found.");

            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.Address))
            {
                return BadRequest(
                    "First name, last name, and address are required.");
            }

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o =>
                    o.OrganiserId == account.OrganiserId.Value);

            if (organiser == null)
                return NotFound("Organiser profile not found.");

            organiser.FirstName = request.FirstName.Trim();
            organiser.LastName = request.LastName.Trim();
            organiser.Address = request.Address.Trim();

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<Account?> GetCurrentAccountAsync()
        {
            var value = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(value, out var accountId))
                return null;

            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == accountId);
        }
    }
}
