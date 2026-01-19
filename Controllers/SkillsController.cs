using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using JobRankingSystem.Data;
using JobRankingSystem.Models;
using JobRankingSystem.Services;

namespace JobRankingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public class SkillsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TrieService _trieService;
        private readonly GraphService _graphService;
        private readonly HashIndexService _hashService;
        private readonly IMemoryCache _cache;

        public SkillsController(AppDbContext context, TrieService trieService, GraphService graphService, HashIndexService hashService, IMemoryCache cache)
        {
            _context = context;
            _trieService = trieService;
            _graphService = graphService;
            _hashService = hashService;
            _cache = cache;
        }

        [HttpGet("autocomplete")]
        public async Task<ActionResult<object>> AutoComplete([FromQuery] string prefix)
        {
            // Cache the Trie construction specifically? Or cache the list?
            // Rebuilding Trie for every request is expensive (O(N*L)). Caching is good.
            
            if (!_cache.TryGetValue("AllSkills", out List<string>? skills))
            {
                 skills = await _context.Skills.Select(s => s.SkillName).ToListAsync();
                 _cache.Set("AllSkills", skills, TimeSpan.FromMinutes(10));
            }

            // We could cache the Trie object itself if we made it thread-safe, 
            // but for now optimizing the DB fetch is step 1.
            
            _trieService.BuildTrie(skills!); 
            
            var (results, searchTrace) = _trieService.AutoComplete(prefix);
            
            return Ok(new { Results = results, Trace = searchTrace }); 
        }

        [HttpGet("network")]
        public async Task<ActionResult<object>> GetSkillNetwork()
        {
            try 
            {
                // Temporarily bypassing cache to debug if needed, but keeping it for now
                // If this fails, we will catch it.
                // Re-enable Cache now that IMemoryCache is registered
                return await _cache.GetOrCreateAsync("SkillGraph", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                    var candidates = await _context.Candidates
                        .AsNoTracking()
                        .Include(c => c.CandidateSkills)
                        .ThenInclude(cs => cs.Skill)
                        .ToListAsync();
                    
                    if (candidates == null) throw new Exception("Candidates list is null");

                    var trace = _graphService.BuildSkillGraph(candidates);
                    var graph = _graphService.AdjacencyList;
                    
                    if (graph == null) throw new Exception("Graph AdjacencyList is null");

                    var simpleGraph = graph.ToDictionary(k => k.Key, v => v.Value);

                    return Ok(new { Graph = simpleGraph, Trace = trace });
                }) ?? StatusCode(500, "Failed to caching graph");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message, Stack = ex.StackTrace, Inner = ex.InnerException?.Message });
            }
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
