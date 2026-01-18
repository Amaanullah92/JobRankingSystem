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
        private readonly JobRankingSystem.Services.KMPService _kmpService;
        private readonly JobRankingSystem.Services.AVLTreeService _avlService;

        public DebugController(AppDbContext context, JobRankingSystem.Services.KMPService kmpService, JobRankingSystem.Services.AVLTreeService avlService)
        {
            _context = context;
            _kmpService = kmpService;
            _avlService = avlService; // Just injecting to see if it crashes
        }

        [HttpGet]
        public async Task<IActionResult> CheckConnection()
        {
            try
            {
                // Try to open connection explicitly
                await _context.Database.OpenConnectionAsync();
                await _context.Database.CloseConnectionAsync();

                // Check if tables exist by querying counts
                int candidatesCount = -1;
                int skillsCount = -1;
                try {
                    candidatesCount = await _context.Candidates.CountAsync();
                    skillsCount = await _context.Skills.CountAsync();
                } catch {
                     // Start seeding if tables exist but empty? No, just report.
                }
                
                return Ok(new { 
                    status = "Success", 
                    message = "Database connection established successfully!", 
                    provider = _context.Database.ProviderName,
                    candidatesCount = candidatesCount,
                    skillsCount = skillsCount,
                     // If counts are -1, it means querying the table failed (likely table missing)
                });
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

        [HttpGet("test-query")]
        public async Task<IActionResult> TestQuery()
        {
            try {
                // Mimic the exact query from CandidatesController
                var candidates = await _context.Candidates
                    .Include(c => c.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
                    // .Take(5) // REMOVED LIMIT to text full dataset
                    .ToListAsync();
                
                return Ok(new { status = "Success", count = candidates.Count, sample = candidates });
            } catch (Exception ex) {
                 return StatusCode(500, new { 
                    status = "Query Error", 
                    message = ex.Message, 
                    innerException = ex.InnerException?.Message ?? "None", 
                    stackTrace = ex.StackTrace 
                });
            }
        }
    }
}