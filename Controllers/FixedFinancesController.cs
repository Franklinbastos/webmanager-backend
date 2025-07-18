
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebManager.Data;
using WebManager.Models;
using System.Security.Claims;

namespace WebManager.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FixedFinancesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FixedFinancesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/FixedFinances
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FixedFinance>>> GetFixedFinances()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            return await _context.FixedFinances.Where(ff => ff.UserId == userId).ToListAsync();
        }

        // GET: api/FixedFinances/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FixedFinance>> GetFixedFinance(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var fixedFinance = await _context.FixedFinances.FindAsync(id);

            if (fixedFinance == null)
            {
                return NotFound();
            }

            if (fixedFinance.UserId != userId)
            {
                return Forbid();
            }

            return fixedFinance;
        }

        // POST: api/FixedFinances
        [HttpPost]
        public async Task<ActionResult<FixedFinance>> PostFixedFinance(FixedFinance fixedFinance)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            fixedFinance.UserId = userId; 

            _context.FixedFinances.Add(fixedFinance);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFixedFinance", new { id = fixedFinance.Id }, fixedFinance);
        }

        // PUT: api/FixedFinances/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFixedFinance(int id, FixedFinance fixedFinance)
        {
            if (id != fixedFinance.Id)
            {
                return BadRequest();
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var existingFixedFinance = await _context.FixedFinances.AsNoTracking().FirstOrDefaultAsync(ff => ff.Id == id);
            if (existingFixedFinance == null)
            {
                return NotFound();
            }

            if (existingFixedFinance.UserId != userId)
            {
                return Forbid();
            }

            fixedFinance.UserId = userId; 
            _context.Entry(fixedFinance).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                
                var financeService = HttpContext.RequestServices.GetRequiredService<WebManager.Services.FinanceService>();
                await financeService.ProcessFixedFinanceUpdate(fixedFinance.Id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FixedFinanceExists(id, userId))
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

        // DELETE: api/FixedFinances/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFixedFinance(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var fixedFinance = await _context.FixedFinances.FindAsync(id);
            if (fixedFinance == null)
            {
                return NotFound();
            }

            if (fixedFinance.UserId != userId)
            {
                return Forbid();
            }

            _context.FixedFinances.Remove(fixedFinance);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FixedFinanceExists(int id, int userId)
        {
            return _context.FixedFinances.Any(e => e.Id == id && e.UserId == userId);
        }

        // POST: api/FixedFinances/generate-recurrent
        [HttpPost("generate-recurrent")]
        public async Task<IActionResult> GenerateRecurrentFinances()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var fixedFinances = await _context.FixedFinances.Where(ff => ff.IsActive && ff.UserId == userId).ToListAsync();
            var newFinances = new List<Finance>();

            foreach (var fixedFinance in fixedFinances)
            {
                for (int i = 0; i < fixedFinance.NumberOfMonths; i++)
                {
                    var targetMonth = fixedFinance.StartDate.AddMonths(i);
                    var billingDay = Math.Min(fixedFinance.BillingDay, DateTime.DaysInMonth(targetMonth.Year, targetMonth.Month));
                    var date = new DateTime(targetMonth.Year, targetMonth.Month, billingDay);

                    var existingFinance = await _context.Finances.FirstOrDefaultAsync(f =>
                        f.UserId == fixedFinance.UserId &&
                        f.Description == fixedFinance.Description &&
                        f.Amount == fixedFinance.Amount &&
                        f.Type == fixedFinance.Type &&
                        f.Date.Year == date.Year &&
                        f.Date.Month == date.Month);

                    if (existingFinance == null)
                    {
                        newFinances.Add(new Finance
                        {
                            UserId = fixedFinance.UserId,
                            Date = date,
                            Description = fixedFinance.Description,
                            Amount = fixedFinance.Amount,
                            Type = fixedFinance.Type,
                            FixedFinanceId = fixedFinance.Id 
                        });
                    }
                }
            }

            if (newFinances.Any())
            {
                _context.Finances.AddRange(newFinances);
                await _context.SaveChangesAsync();
                return Ok(new { message = $"{newFinances.Count} recurrent finance entries generated successfully." });
            }

            return Ok(new { message = "No new recurrent finance entries to generate." });
        }
    }
}
