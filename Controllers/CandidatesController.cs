using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobRankingSystem.Data;
using JobRankingSystem.Models;
using JobRankingSystem.Services;

namespace JobRankingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidatesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly KMPService _kmpService;
        private readonly AVLTreeService _avlService;

        public CandidatesController(AppDbContext context, KMPService kmpService, AVLTreeService avlService)
        {
            _context = context;
            _kmpService = kmpService;
            _avlService = avlService;
        }

        // GET: api/Candidates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Candidate>>> GetCandidates()
        {
            return await _context.Candidates.Include(c => c.CandidateSkills).ThenInclude(cs => cs.Skill).ToListAsync();
        }

        // POST: api/Candidates
        [HttpPost]
        public async Task<ActionResult<Candidate>> PostCandidate([FromBody] Candidate candidate)
        {
            try 
            {
                if (candidate.CreatedAt == default)
                {
                    candidate.CreatedAt = DateTime.UtcNow;
                }
                
                // Ensure skills list is not null
                if (candidate.CandidateSkills == null) candidate.CandidateSkills = new List<CandidateSkill>();

                // FEATURE: Automatic Skill Extraction
                // Check if any known skills from the DB appear in the ResumeText
                if (!string.IsNullOrEmpty(candidate.ResumeText))
                {
                    var allSkills = await _context.Skills.ToListAsync();
                    var resumeLower = candidate.ResumeText.ToLower();

                    foreach (var skill in allSkills)
                    {
                        if (resumeLower.Contains(skill.SkillName.ToLower()))
                        {
                            // Avoid duplicates if frontend sent it (unlikely but safe)
                            if (!candidate.CandidateSkills.Any(cs => cs.SkillId == skill.Id))
                            {
                                candidate.CandidateSkills.Add(new CandidateSkill 
                                { 
                                    SkillId = skill.Id,
                                    // EF Core will handle the CandidateId linkage upon Add()
                                });
                            }
                        }
                    }
                }

                _context.Candidates.Add(candidate);
                await _context.SaveChangesAsync();
                
                return CreatedAtAction("GetCandidates", new { id = candidate.Id }, candidate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating candidate: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Candidates/search?keyword=Java
        [HttpGet("search")]
        public async Task<ActionResult<object>> SearchCandidates([FromQuery] string keyword)
        {
            var candidates = await _context.Candidates.ToListAsync();
            // Use KMP to search in ResumeText? Or AVL for specific fields?
            // Requirement: "KMP Pattern Matching... Resume keyword detection"
            
            var matchedCandidates = new List<Candidate>();
            var traces = new List<AlgorithmTrace>();

            foreach (var c in candidates)
            {
                var trace = _kmpService.SearchPattern(c.ResumeText, keyword);
                if (trace.Steps.Any(s => s.Description.Contains("Pattern found")))
                {
                    matchedCandidates.Add(c);
                }
                
                // Add trace if we haven't flooded the response, or if it's a match.
                // Logic update: Prioritize showing the match trace!
                if (matchedCandidates.Count <= 1 || trace.Steps.Any(s => s.Description.Contains("Pattern found"))) 
                {
                     if (traces.Count < 3) traces.Add(trace); // Limit to 3 traces max
                }
            }

            return Ok(new { candidates = matchedCandidates, traces = traces });
        }

        // GET: api/Candidates/avl-build
        [HttpGet("avl-build")]
        public async Task<ActionResult<object>> GetAVLVisual()
        {
             var candidates = await _context.Candidates.ToListAsync();
             var trace = _avlService.InsertAndGetTrace(candidates);
             return Ok(trace);
        }
    }
}
