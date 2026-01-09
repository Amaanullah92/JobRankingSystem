using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobRankingSystem.Data;
using JobRankingSystem.Models;
using JobRankingSystem.Services;

namespace JobRankingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TrieService _trieService;
        private readonly GraphService _graphService;
        private readonly HashIndexService _hashService;

        public SkillsController(AppDbContext context, TrieService trieService, GraphService graphService, HashIndexService hashService)
        {
            _context = context;
            _trieService = trieService;
            _graphService = graphService;
            _hashService = hashService;
        }

        [HttpGet("autocomplete")]
        public async Task<ActionResult<object>> AutoComplete([FromQuery] string prefix)
        {
            var skills = await _context.Skills.Select(s => s.SkillName).ToListAsync();
            
            // Build trace serves as context, but Search Trace is more relevant for the user's request.
            // Ideally we'd separate them. For now, let's prioritize the Search Trace if prefix exists.
            
            _trieService.BuildTrie(skills); // We build it but discard trace if we are searching
            
            var (results, searchTrace) = _trieService.AutoComplete(prefix);
            
            return Ok(new { Results = results, Trace = searchTrace }); 
        }

        [HttpGet("network")]
        public async Task<ActionResult<object>> GetSkillNetwork()
        {
            var candidates = await _context.Candidates
                .Include(c => c.CandidateSkills)
                .ThenInclude(cs => cs.Skill)
                .ToListAsync();

            var trace = _graphService.BuildSkillGraph(candidates);
            var graph = _graphService.AdjacencyList; // Only show non-empty or simple representation

            // Simplify graph for JSON response if needed
            var simpleGraph = graph.ToDictionary(k => k.Key, v => v.Value);

            return Ok(new { Graph = simpleGraph, Trace = trace });
        }

        [HttpGet("index")]
        public async Task<ActionResult<object>> GetHashIndex()
        {
             var candidates = await _context.Candidates
                .Include(c => c.CandidateSkills)
                .ThenInclude(cs => cs.Skill)
                .ToListAsync();
            
            var trace = _hashService.BuildIndex(candidates);
            return Ok(new { Trace = trace });
        }
    }
}
