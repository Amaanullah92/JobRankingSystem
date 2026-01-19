using Microsoft.AspNetCore.Identity;

namespace JobRankingSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add custom properties here if needed (e.g., Full Name)
        public string? FullName { get; set; }
    }
}
