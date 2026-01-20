using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobRankingSystem.Data;
using JobRankingSystem.Models;
using JobRankingSystem.Services;
using JobRankingSystem.Repositories;

namespace JobRankingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Removed [Authorize] - public access for demo
    public class CandidatesController : ControllerBase
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly KMPService _kmpService;
        private readonly AVLTreeService _avlService;
        private readonly AppDbContext _context;

        public CandidatesController(ICandidateRepository candidateRepository, KMPService kmpService, AVLTreeService avlService, AppDbContext context)
        {
            _candidateRepository = candidateRepository;
            _kmpService = kmpService;
            _avlService = avlService;
            _context = context;
        }

        // GET: api/Candidates?page=1&pageSize=10
        [HttpGet]
        [AllowAnonymous] // Temporarily public for debugging
        public async Task<IActionResult> GetCandidates([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;

                var (candidates, total) = await _candidateRepository.GetCandidatesAsync(page, pageSize);

                var dtos = candidates.Select(c => new CandidateDto 
                {
                    Id = c.Id,
                    FullName = c.FullName,
                    ExperienceYears = c.ExperienceYears,
                    Education = c.Education,
                    ResumeText = c.ResumeText,
                    ExpectedSalary = c.ExpectedSalary,
                    Skills = c.CandidateSkills.Select(cs => cs.Skill.SkillName).ToList()
                });
                
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    status = "Error", 
                    message = "Failed to fetch candidates", 
                    detail = ex.Message 
                });
            }
        }

        // POST: api/Candidates
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Candidate>> PostCandidate([FromBody] Candidate candidate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try 
            {
                if (candidate.CreatedAt == default)
                {
                    candidate.CreatedAt = DateTime.UtcNow;
                }
                
                if (candidate.CandidateSkills == null) candidate.CandidateSkills = new List<CandidateSkill>();

                // Extract skills from resume text
                if (!string.IsNullOrEmpty(candidate.ResumeText))
                {
                    await ExtractSkillsFromResume(candidate);
                }

                await _candidateRepository.AddCandidateAsync(candidate);
                
                return CreatedAtAction("GetCandidates", new { id = candidate.Id }, candidate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating candidate: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task ExtractSkillsFromResume(Candidate candidate)
        {
            // Get all skills from database
            var allSkills = await _context.Skills.ToListAsync();
            
            // Simple keyword matching - check if skill names appear in resume
            var resumeLower = candidate.ResumeText.ToLower();
            var extractedSkills = new List<CandidateSkill>();

            foreach (var skill in allSkills)
            {
                // Check if the skill name appears in the resume (case-insensitive)
                if (resumeLower.Contains(skill.SkillName.ToLower()))
                {
                    extractedSkills.Add(new CandidateSkill
                    {
                        Candidate = candidate,
                        SkillId = skill.Id
                    });
                }
            }

            candidate.CandidateSkills = extractedSkills;
        }

        // DELETE: api/Candidates/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCandidate(int id)
        {
            if (!await _candidateRepository.CandidateExistsAsync(id))
            {
                return NotFound();
            }

            try
            {
                await _candidateRepository.DeleteCandidateAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                 return StatusCode(500, new { error = ex.Message });
            }
        }

        // PUT: api/Candidates/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutCandidate(int id, Candidate candidate)
        {
            if (id != candidate.Id)
            {
                return BadRequest();
            }

            try
            {
                // Update skills if resume is provided
                if (!string.IsNullOrEmpty(candidate.ResumeText))
                {
                    await ExtractSkillsFromResume(candidate);
                }
                
                await _candidateRepository.UpdateCandidateAsync(candidate);
                return NoContent();
            }
            catch (Exception ex)
            {
                if (!await _candidateRepository.CandidateExistsAsync(id))
                {
                     return NotFound();
                }
                else 
                {
                     throw;
                }
            }
        }


        // GET: api/Candidates/search?keyword=Java
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> SearchCandidates([FromQuery] string keyword)
        {
            var candidates = await _candidateRepository.GetAllCandidatesAsync();
            
            var matchedCandidates = new List<Candidate>();
            var traces = new List<AlgorithmTrace>();

            foreach (var c in candidates)
            {
                var trace = _kmpService.SearchPattern(c.ResumeText, keyword);
                if (trace.Steps.Any(s => s.Description.Contains("Pattern found")))
                {
                    matchedCandidates.Add(c);
                }
                
                if (matchedCandidates.Count <= 1 || trace.Steps.Any(s => s.Description.Contains("Pattern found"))) 
                {
                     if (traces.Count < 3) traces.Add(trace); 
                }
            }

            var dtos = matchedCandidates.Select(c => new CandidateDto
            {
                Id = c.Id,
                FullName = c.FullName,
                ExperienceYears = c.ExperienceYears,
                Education = c.Education,
                ResumeText = c.ResumeText,
                ExpectedSalary = c.ExpectedSalary,
                Skills = c.CandidateSkills.Select(cs => cs.Skill.SkillName).ToList()
            });

            return Ok(new { candidates = dtos, traces = traces });
        }

        // GET: api/Candidates/avl-build
        [HttpGet("avl-build")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetAVLVisual()
        {
             var candidates = (await _candidateRepository.GetAllCandidatesAsync()).ToList();
             var trace = _avlService.InsertAndGetTrace(candidates);
             return Ok(trace);
        }
    }
}
