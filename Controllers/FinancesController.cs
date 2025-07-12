using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebManager.Data;
using WebManager.Models;

namespace WebManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
            return await _context.Finances.ToListAsync();
        }

        // GET: /Finances/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Finance>> GetById(int id)
        {
            var finance = await _context.Finances.FindAsync(id);

            if (finance == null)
            {
                return NotFound();
            }

            return finance;
        }

        // POST: /Finances
        [HttpPost]
        public async Task<ActionResult<Finance>> Create(Finance newFinance)
        {
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
            var finance = await _context.Finances.FindAsync(id);
            if (finance == null)
            {
                return NotFound();
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
