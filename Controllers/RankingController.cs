using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobRankingSystem.Data;
using JobRankingSystem.Models;
using JobRankingSystem.Services;

namespace JobRankingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RankingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly MaxHeapService _heapService;
        private readonly SortingService _sortingService;
        private readonly GreedySelectionService _greedyService;

        public RankingController(AppDbContext context, MaxHeapService heapService, SortingService sortingService, GreedySelectionService greedyService)
        {
            _context = context;
            _heapService = heapService;
            _sortingService = sortingService;
            _greedyService = greedyService;
        }

        [HttpGet("rank")]
        public async Task<ActionResult<object>> GetRankedCandidates()
        {
            var candidates = await _context.Candidates.ToListAsync();
            var (sorted, trace) = _heapService.SortCandidates(candidates);
            return Ok(new { Candidates = sorted, Trace = trace });
        }

        [HttpGet("sort")]
        public async Task<ActionResult<object>> GetSortedCandidates([FromQuery] string algorithm = "MergeSort")
        {
            var candidates = await _context.Candidates.ToListAsync();
            SortingService.SortType type = algorithm == "QuickSort" ? SortingService.SortType.QuickSort : SortingService.SortType.MergeSort;
            
            var (sorted, trace) = _sortingService.Sort(candidates, type);
            return Ok(new { Candidates = sorted, Trace = trace });
        }

        [HttpGet("shortlist")]
        public async Task<ActionResult<object>> ShortlistCandidates([FromQuery] decimal budget)
        {
            var candidates = await _context.Candidates.ToListAsync();
            var (selected, trace) = _greedyService.SelectCandidates(candidates, budget);
            return Ok(new { Candidates = selected, Trace = trace });
        }
    }
}
