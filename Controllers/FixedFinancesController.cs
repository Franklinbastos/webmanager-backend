
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebManager.Data;
using WebManager.Models;

namespace WebManager.Controllers
{
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
            return await _context.FixedFinances.ToListAsync();
        }

        // GET: api/FixedFinances/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FixedFinance>> GetFixedFinance(int id)
        {
            var fixedFinance = await _context.FixedFinances.FindAsync(id);

            if (fixedFinance == null)
            {
                return NotFound();
            }

            return fixedFinance;
        }

        // POST: api/FixedFinances
        [HttpPost]
        public async Task<ActionResult<FixedFinance>> PostFixedFinance(FixedFinance fixedFinance)
        {
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

            _context.Entry(fixedFinance).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FixedFinanceExists(id))
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
            var fixedFinance = await _context.FixedFinances.FindAsync(id);
            if (fixedFinance == null)
            {
                return NotFound();
            }

            _context.FixedFinances.Remove(fixedFinance);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FixedFinanceExists(int id)
        {
            return _context.FixedFinances.Any(e => e.Id == id);
        }
    }
}
