using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobRankingSystem.Models;
using JobRankingSystem.Services;
using JobRankingSystem.Repositories;

namespace JobRankingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Removed [Authorize] - ranking is public (view-only operation)
    public class RankingController : ControllerBase
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly MaxHeapService _heapService;
        private readonly SortingService _sortingService;
        private readonly GreedySelectionService _greedyService;

        public RankingController(ICandidateRepository candidateRepository, MaxHeapService heapService, SortingService sortingService, GreedySelectionService greedyService)
        {
            _candidateRepository = candidateRepository;
            _heapService = heapService;
            _sortingService = sortingService;
            _greedyService = greedyService;
        }

        [HttpGet("rank")]
        public async Task<ActionResult<object>> GetRankedCandidates()
        {
            var candidates = (await _candidateRepository.GetAllCandidatesAsync()).ToList();
                
            var (sorted, trace) = _heapService.SortCandidates(candidates);
            
            var dtos = sorted.Select(c => new CandidateDto
            {
                Id = c.Id,
                FullName = c.FullName,
                ExperienceYears = c.ExperienceYears,
                Education = c.Education,
                ResumeText = c.ResumeText,
                ExpectedSalary = c.ExpectedSalary,
                Skills = c.CandidateSkills.Select(cs => cs.Skill.SkillName).ToList()
            });

            return Ok(new { Candidates = dtos, Trace = trace });
        }

        [HttpGet("sort")]
        public async Task<ActionResult<object>> GetSortedCandidates([FromQuery] string algorithm = "MergeSort")
        {
            var candidates = (await _candidateRepository.GetAllCandidatesAsync()).ToList();
                
            SortingService.SortType type = algorithm == "QuickSort" ? SortingService.SortType.QuickSort : SortingService.SortType.MergeSort;
            
            var (sorted, trace) = _sortingService.Sort(candidates, type);

            var dtos = sorted.Select(c => new CandidateDto
            {
                Id = c.Id,
                FullName = c.FullName,
                ExperienceYears = c.ExperienceYears,
                Education = c.Education,
                ResumeText = c.ResumeText,
                ExpectedSalary = c.ExpectedSalary,
                Skills = c.CandidateSkills.Select(cs => cs.Skill.SkillName).ToList()
            });

            return Ok(new { Candidates = dtos, Trace = trace });
        }

        [HttpGet("shortlist")]
        public async Task<ActionResult<object>> ShortlistCandidates([FromQuery] decimal budget)
        {
            var candidates = (await _candidateRepository.GetAllCandidatesAsync()).ToList();
                
            var (selected, trace) = _greedyService.SelectCandidates(candidates, budget);

            var dtos = selected.Select(c => new CandidateDto
            {
                Id = c.Id,
                FullName = c.FullName,
                ExperienceYears = c.ExperienceYears,
                Education = c.Education,
                ResumeText = c.ResumeText,
                ExpectedSalary = c.ExpectedSalary,
                Skills = c.CandidateSkills.Select(cs => cs.Skill.SkillName).ToList()
            });

            return Ok(new { Candidates = dtos, Trace = trace });
        }
    }
}
