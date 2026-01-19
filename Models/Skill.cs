using System.ComponentModel.DataAnnotations;

namespace JobRankingSystem.Models
{
    public class Skill
    {
        public int Id { get; set; }

        [Required]
        public string SkillName { get; set; } = string.Empty;

        public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
    }
}
