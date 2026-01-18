using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobRankingSystem.Data;

namespace JobRankingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DebugController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> CheckConnection()
        {
            try
            {
                // Try to open connection explicitly
                await _context.Database.OpenConnectionAsync();
                await _context.Database.CloseConnectionAsync();
                return Ok(new { status = "Success", message = "Database connection established successfully!", provider = _context.Database.ProviderName });
            }
            catch (Exception ex)
            {
                // Return the full error to the caller (Only for debugging!! - Delete later)
                return StatusCode(500, new { 
                    status = "Error", 
                    message = ex.Message, 
                    innerException = ex.InnerException?.Message ?? "None", 
                    stackTrace = ex.StackTrace 
                });
            }
        }
    }
}