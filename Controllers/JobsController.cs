using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobRankingSystem.Data;
using JobRankingSystem.Models;
using JobRankingSystem.Services;

namespace JobRankingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly DPFitScoreService _dpService;

        public JobsController(AppDbContext context, DPFitScoreService dpService)
        {
            _context = context;
            _dpService = dpService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
        {
            return await _context.Jobs.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Job>> PostJob(Job job)
        {
            // Ensure any new skills in the job description are added to the Skills table
            if (!string.IsNullOrEmpty(job.RequiredSkills))
            {
                var skillNames = job.RequiredSkills.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(s => s.Trim())
                                                   .Distinct();

                foreach (var name in skillNames)
                {
                    if (!await _context.Skills.AnyAsync(s => s.SkillName == name))
                    {
                        _context.Skills.Add(new Skill { SkillName = name });
                    }
                }
                await _context.SaveChangesAsync();
            }

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetJobs", new { id = job.Id }, job);
        }

        [HttpGet("{id}/match")]
        public async Task<ActionResult<object>> MatchCandidates(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            var candidates = await _context.Candidates
                .Include(c => c.CandidateSkills)
                .ThenInclude(cs => cs.Skill)
                .ToListAsync();

            var jobSkills = job.RequiredSkills.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            
            var results = new List<object>();

            foreach (var c in candidates)
            {
                var candidateSkills = c.CandidateSkills.Select(s => s.Skill.SkillName).ToList();
                var (score, trace) = _dpService.CalculateFitScore(jobSkills, candidateSkills);
                
                results.Add(new 
                { 
                    Candidate = c.FullName, 
                    Score = score,
                    Trace = trace 
                });
            }

            return Ok(results.OrderByDescending(r => ((dynamic)r).Score));
        }
    }
}
