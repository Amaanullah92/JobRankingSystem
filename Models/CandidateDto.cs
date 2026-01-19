using System.Collections.Generic;

namespace JobRankingSystem.Models
{
    public class CandidateDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public string Education { get; set; } = string.Empty;
        public string ResumeText { get; set; } = string.Empty;
        public decimal ExpectedSalary { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
    }
}
