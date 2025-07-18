using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebManager.Data;
using WebManager.Models;

namespace WebManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class FinancesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FinancesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Finances
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Finance>>> GetAll()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"FinancesController: Attempting to fetch finances. Raw UserId from token: {userIdString ?? "null"}");

            if (string.IsNullOrEmpty(userIdString))
            {
                Console.WriteLine("FinancesController: Unauthorized - UserId is null or empty.");
                return Unauthorized();
            }

            if (!int.TryParse(userIdString, out var parsedUserId))
            {
                Console.WriteLine($"FinancesController: Unauthorized - Could not parse UserId: {userIdString}");
                return Unauthorized();
            }
            Console.WriteLine($"FinancesController: Fetching finances for UserId: {parsedUserId}");

            var finances = await _context.Finances.Where(f => f.UserId == parsedUserId).ToListAsync();
            Console.WriteLine($"FinancesController: Found {finances.Count} finances for UserId: {parsedUserId}");
            return Ok(finances);
        }

        // GET: /Finances/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Finance>> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var finance = await _context.Finances.FindAsync(id);

            if (finance == null)
            {
                return NotFound();
            }

            if (finance.UserId != int.Parse(userId!))
            {
                return Forbid();
            }

            return finance;
        }

        // POST: /Finances
        [HttpPost]
        public async Task<ActionResult<Finance>> Create(Finance newFinance)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            newFinance.UserId = int.Parse(userId);
            _context.Finances.Add(newFinance);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = newFinance.Id }, newFinance);
        }

        // PUT: /Finances/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Finance updatedFinance)
        {
            if (id != updatedFinance.Id)
            {
                return BadRequest();
            }
            
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
            {
                return Unauthorized();
            }
            var userId = int.Parse(userIdString);

            var finance = await _context.Finances.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);

            if (finance == null)
            {
                return NotFound();
            }

            if (finance.UserId != userId)
            {
                return Forbid();
            }

            updatedFinance.UserId = userId;
            _context.Entry(updatedFinance).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FinanceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: /Finances/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var finance = await _context.Finances.FindAsync(id);
            
            if (finance == null)
            {
                return NotFound();
            }

            if (finance.UserId != int.Parse(userId!))
            {
                return Forbid();
            }

            _context.Finances.Remove(finance);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FinanceExists(int id)
        {
            return _context.Finances.Any(e => e.Id == id);
        }
    }
}
