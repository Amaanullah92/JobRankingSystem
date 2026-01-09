using System.ComponentModel.DataAnnotations;

namespace JobRankingSystem.Models
{
    public class Job
    {
        public int Id { get; set; }

        [Required]
        public string JobTitle { get; set; } = string.Empty;

        // Comma-separated or JSON list of required skills for simplicity in this Academic project
        // Or we could do a many-to-many. Requirements said "RequiredSkills", assuming string or list.
        // Let's keep it simple as a string for now, or match the requirements carefully.
        // Requirement: "RequiredSkills" column. 
        public string RequiredSkills { get; set; } = string.Empty; 

        public int MinExperience { get; set; }
        public decimal MaxSalary { get; set; }
    }
}
