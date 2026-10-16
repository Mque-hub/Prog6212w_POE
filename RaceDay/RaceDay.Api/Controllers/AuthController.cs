using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Contracts.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    
        [ApiController]
        [Route("api/[controller]")]
        public class AuthController : ControllerBase
        {
            private readonly RaceDayDbContext _context;
            private readonly PasswordHasher<Account> _passwordHasher;

            public AuthController(RaceDayDbContext context)
            {
                _context = context;
                _passwordHasher = new PasswordHasher<Account>();
            }

            [HttpPost("register/participant")]
            public async Task<IActionResult> RegisterParticipant(
                RegisterParticipantRequest request)
            {
                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest("Email and password are required.");
                }

                if (await _context.Accounts.AnyAsync(
                    a => a.Email == request.Email.Trim()))
                {
                    return Conflict("This email is already registered.");
                }

                if (string.IsNullOrWhiteSpace(request.FirstName) ||
                    string.IsNullOrWhiteSpace(request.LastName) ||
                    string.IsNullOrWhiteSpace(request.CellNumber) ||
                    string.IsNullOrWhiteSpace(request.Address))
                {
                    return BadRequest("All participant profile fields are required.");
                }

                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    var participant = new Participant
                    {
                        FirstName = request.FirstName.Trim(),
                        LastName = request.LastName.Trim(),
                        CellNumber = request.CellNumber.Trim(),
                        Address = request.Address.Trim()
                    };

                    _context.Participants.Add(participant);
                    await _context.SaveChangesAsync();

                    var account = new Account
                    {
                        ParticipantId = participant.ParticipantId,
                        OrganiserId = null,
                        Role = Role.Participant,
                        Email = request.Email.Trim(),
                        PasswordHash = string.Empty
                    };

                    account.PasswordHash =
                        _passwordHasher.HashPassword(
                            account, request.Password);

                    _context.Accounts.Add(account);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return StatusCode(201, new AuthResponse
                    {
                        UserID = account.AccountId,
                        AuthenticationRole = account.Role.ToString(),
                        Email = account.Email
                    });
                }
                catch (DbUpdateException)
                {
                    await transaction.RollbackAsync();

                    return Conflict(
                        "Registration could not be completed. " +
                        "The email may already be registered.");
                }
            }

            [HttpPost("register/organiser")]
            public async Task<IActionResult> RegisterOrganiser(
                RegisterOrganiserRequest request)
            {
                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest("Email and password are required.");
                }

                if (await _context.Accounts.AnyAsync(
                    a => a.Email == request.Email.Trim()))
                {
                    return Conflict("This email is already registered.");
                }

                if (string.IsNullOrWhiteSpace(request.FirstName) ||
                    string.IsNullOrWhiteSpace(request.LastName) ||
                    string.IsNullOrWhiteSpace(request.Address))
                {
                    return BadRequest("All organiser profile fields are required.");
                }

                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    var organiser = new Organiser
                    {
                        FirstName = request.FirstName.Trim(),
                        LastName = request.LastName.Trim(),
                        Address = request.Address.Trim()
                    };

                    _context.Organisers.Add(organiser);
                    await _context.SaveChangesAsync();

                    var account = new Account
                    {
                        ParticipantId = null,
                        OrganiserId = organiser.OrganiserId,
                        Role = Role.Organiser,
                        Email = request.Email.Trim(),
                        PasswordHash = string.Empty
                    };

                    account.PasswordHash =
                        _passwordHasher.HashPassword(
                            account, request.Password);

                    _context.Accounts.Add(account);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return StatusCode(201, new AuthResponse
                    {
                        UserID = account.AccountId,
                        AuthenticationRole = account.Role.ToString(),
                        Email = account.Email
                    });
                }
                catch (DbUpdateException)
                {
                    await transaction.RollbackAsync();

                    return Conflict(
                        "Registration could not be completed. " +
                        "The email may already be registered.");
                }
            }
        }
}
