using JobRankingSystem.Data;
using JobRankingSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobRankingSystem.Repositories
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly AppDbContext _context;

        public CandidateRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Candidate>> GetAllCandidatesAsync()
        {
            return await _context.Candidates
                .Include(c => c.CandidateSkills)
                .ThenInclude(cs => cs.Skill)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Candidate> Candidates, int TotalCount)> GetCandidatesAsync(int page, int pageSize)
        {
            var query = _context.Candidates
                .Include(c => c.CandidateSkills)
                .ThenInclude(cs => cs.Skill);

            var totalCount = await query.CountAsync();
            
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Candidate?> GetCandidateByIdAsync(int id)
        {
            return await _context.Candidates
                .Include(c => c.CandidateSkills)
                .ThenInclude(cs => cs.Skill)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddCandidateAsync(Candidate candidate)
        {
            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCandidateAsync(Candidate candidate)
        {
            _context.Entry(candidate).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCandidateAsync(int id)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate != null)
            {
                _context.Candidates.Remove(candidate);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> CandidateExistsAsync(int id)
        {
            return await _context.Candidates.AnyAsync(e => e.Id == id);
        }
    }
}
