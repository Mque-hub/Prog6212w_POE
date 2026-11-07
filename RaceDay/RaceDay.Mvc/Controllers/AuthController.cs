using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Contracts.DTOs;
using RaceDay.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace RaceDay.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Tags("Authentication")]
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

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email and password are required.");
            }

            var email = request.Email.Trim();

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Email == email); // this will find the account matching the supplied email. 

            if (account == null)
            {
                return Unauthorized("Invalid email or password."); //This should return HTTP 401 for invalid credentials.
            }

            var passwordHasher = new PasswordHasher<Account>();

            var verificationResult = passwordHasher.VerifyHashedPassword(
                account,
                account.PasswordHash,
                request.Password); // This checks the submitted password without storing or comparing plain-text passwords.

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid email or password.");
            }

            var jwtKey = HttpContext.RequestServices
            .GetRequiredService<IConfiguration>()["Jwt:Key"];

            var jwtIssuer = HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()["Jwt:Issuer"];

            var jwtAudience = HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()["Jwt:Audience"];

            var expiryMinutes = HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()
                .GetValue<int>("Jwt:ExpiryMinutes");

            if (string.IsNullOrWhiteSpace(jwtKey) ||
                string.IsNullOrWhiteSpace(jwtIssuer) ||
                string.IsNullOrWhiteSpace(jwtAudience))
            {
                throw new InvalidOperationException(
                    "JWT configuration is incomplete.");
            }

            var claims = new List<Claim>
{
            new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Role, account.Role.ToString())
};

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler()
                .WriteToken(token);

            return Ok(new
            {
                userID = account.AccountId,
                authenticationRole = account.Role.ToString(),
                email = account.Email,
                accessToken = tokenString,
                tokenType = "Bearer",
                expiresInMinutes = expiryMinutes
            });
        }
    }


}
