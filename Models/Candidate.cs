using System.ComponentModel.DataAnnotations;

namespace JobRankingSystem.Models
{
    public class Candidate
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        public int ExperienceYears { get; set; }
        public string Education { get; set; } = string.Empty;
        public string ResumeText { get; set; } = string.Empty;
        public decimal ExpectedSalary { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for Many-to-Many relationship
        public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
    }
}
