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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events/3/categories
        [HttpGet("events/{eventId:int}/categories")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDto>>>
            GetCategories(int eventId)
        {
            var eventExists = await _context.Events
                .AnyAsync(e => e.EventID == eventId);

            if (!eventExists)
                return NotFound("Event not found.");

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.EventID == eventId)
                .Select(c => new CategoryDto
                {
                    CategoryID = c.CategoryID,
                    EventID = c.EventID,
                    CategoryName = c.CategoryName,
                    MinAge = c.MinAge,
                    MaxAge = c.MaxAge
                })
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/events/3/categories
        [HttpPost("events/{eventId:int}/categories")]
        [Authorize(Roles = nameof(Role.Organiser))]
        public async Task<ActionResult<CategoryDto>> CreateCategory(
            int eventId,
            CategoryRequest request)
        {
            if (request.MinAge < 0 ||
                request.MaxAge < request.MinAge)
            {
                return BadRequest(
                    "Age limits must be valid, and maximum age must be at least minimum age.");
            }

            var accountIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(accountIdValue, out var accountId))
                return Unauthorized();

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (account?.OrganiserId == null)
                return Forbid();

            var eventToManage = await _context.Events
                .FirstOrDefaultAsync(e =>
                    e.EventID == eventId &&
                    e.OrganiserID == account.OrganiserId.Value);

            if (eventToManage == null)
                return NotFound(
                    "Event not found, or you do not own this event.");

            var category = new Category
            {
                EventID = eventId,
                CategoryName = request.CategoryName,
                MinAge = request.MinAge,
                MaxAge = request.MaxAge
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var result = new CategoryDto
            {
                CategoryID = category.CategoryID,
                EventID = category.EventID,
                CategoryName = category.CategoryName,
                MinAge = category.MinAge,
                MaxAge = category.MaxAge
            };

            return CreatedAtAction(
                nameof(GetCategories),
                new { eventId = category.EventID },
                result);
        }

        // PUT: api/categories/1
        [HttpPut("categories/{id:int}")]
        [Authorize(Roles = nameof(Role.Organiser))]
        public async Task<IActionResult> UpdateCategory(
            int id,
            CategoryRequest request)
        {
            if (request.MinAge < 0 ||
                request.MaxAge < request.MinAge)
            {
                return BadRequest(
                    "Age limits must be valid, and maximum age must be at least minimum age.");
            }

            var accountIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(accountIdValue, out var accountId))
                return Unauthorized();

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (account?.OrganiserId == null)
                return Forbid();

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c =>
                    c.CategoryID == id &&
                    c.Event.OrganiserID == account.OrganiserId.Value);

            if (category == null)
                return NotFound(
                    "Category not found, or you do not own its event.");

            // Do not allow moving a category to another event
            // through the request body.
            category.CategoryName = request.CategoryName;
            category.MinAge = request.MinAge;
            category.MaxAge = request.MaxAge;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/categories/1
        [HttpDelete("categories/{id:int}")]
        [Authorize(Roles = nameof(Role.Organiser))]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var accountIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(accountIdValue, out var accountId))
                return Unauthorized();

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (account?.OrganiserId == null)
                return Forbid();

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c =>
                    c.CategoryID == id &&
                    c.Event.OrganiserID == account.OrganiserId.Value);

            if (category == null)
                return NotFound(
                    "Category not found, or you do not own its event.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

