using JobRankingSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobRankingSystem.Repositories
{
    public interface ICandidateRepository
    {
        Task<IEnumerable<Candidate>> GetAllCandidatesAsync();
        Task<(IEnumerable<Candidate> Candidates, int TotalCount)> GetCandidatesAsync(int page, int pageSize);
        Task<Candidate?> GetCandidateByIdAsync(int id);
        Task AddCandidateAsync(Candidate candidate);
        Task UpdateCandidateAsync(Candidate candidate);
        Task DeleteCandidateAsync(int id);
        Task<bool> CandidateExistsAsync(int id);
    }
}
