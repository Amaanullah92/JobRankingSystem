using JobRankingSystem.Data;
using JobRankingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobRankingSystem
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new AppDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>()))
            {
                // Ensure DB is created
                context.Database.EnsureCreated();

                // 1. Ensure Skills Exist (Add if missing)
                var skillNames = new[] 
                { 
                    "Java", "Python", "C#", "SQL", "React", "Data Structures", 
                    "C++", "HTML", "CSS", "TypeScript", "Next.js", "Figma", "Machine Learning", "JavaScript", "Docker", "Kubernetes",
                    "AWS", "Azure", "CI/CD", "Linux", "Networking", "Security", "Excel", "Tableau", "Jira", "Agile", "Selenium", "Testing", "NoSQL", "PostgreSQL",
                    "Rust", "Go", "Golang", "Swift", "Kotlin",
                    // Backend specific skills
                    "ASP.NET", "ASP.NET Core", ".NET", ".NET Core", "RESTful", "REST", "API", "Authentication", "Authorization",
                    // Job Titles as Skills
                    "Frontend Developer", "Backend Engineer", "Data Scientist", "Cloud Engineer", 
                    "DevOps Engineer", "Offensive Security", "Data Analyst", "Project Manager", 
                    "SQA", "Database Engineer"
                };

                var existingSkills = context.Skills.ToList();
                var newSkills = new List<Skill>();

                foreach (var name in skillNames)
                {
                    if (!existingSkills.Any(s => s.SkillName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        var s = new Skill { SkillName = name };
                        newSkills.Add(s);
                        existingSkills.Add(s); // Add to local list for referencing below
                    }
                }

                if (newSkills.Any())
                {
                    context.Skills.AddRange(newSkills);
                    context.SaveChanges();
                }

                // 2. Ensure Jobs Exist (Add if missing)
                var jobs = new[]
                {
                    new Job { JobTitle = "Senior Backend Engineer", RequiredSkills = "Senior Backend Engineer, C#, SQL, Data Structures", MinExperience = 5, MaxSalary = 130000 },
                    new Job { JobTitle = "Frontend Developer", RequiredSkills = "Frontend Developer, React, JavaScript", MinExperience = 1, MaxSalary = 70000 },
                    new Job { JobTitle = "Data Scientist", RequiredSkills = "Data Scientist, Python, SQL, Machine Learning", MinExperience = 3, MaxSalary = 110000 },
                    new Job { JobTitle = "Cloud Engineer", RequiredSkills = "Cloud Engineer, AWS, Docker, Kubernetes, SQL", MinExperience = 4, MaxSalary = 125000 },
                    new Job { JobTitle = "DevOps Engineer", RequiredSkills = "DevOps Engineer, Docker, Kubernetes, Python, CI/CD", MinExperience = 3, MaxSalary = 120000 },
                    new Job { JobTitle = "Offensive Security", RequiredSkills = "Offensive Security, Python, C++, Networking, Security", MinExperience = 4, MaxSalary = 140000 },
                    new Job { JobTitle = "Data Analyst", RequiredSkills = "Data Analyst, Python, SQL, Excel, Tableau", MinExperience = 2, MaxSalary = 85000 },
                    new Job { JobTitle = "Project Manager", RequiredSkills = "Project Manager, Jira, Agile, Communication", MinExperience = 5, MaxSalary = 115000 },
                    new Job { JobTitle = "SQA", RequiredSkills = "SQA, Java, Python, SQL, Selenium, Testing", MinExperience = 2, MaxSalary = 80000 },
                    new Job { JobTitle = "Database Engineer", RequiredSkills = "Database Engineer, SQL, PostgreSQL, NoSQL, Python", MinExperience = 5, MaxSalary = 125000 }
                };

                var existingJobs = context.Jobs.ToList();
                var newJobs = new List<Job>();

                foreach (var job in jobs)
                {
                    var existingJob = existingJobs.FirstOrDefault(j => j.JobTitle.Equals(job.JobTitle, StringComparison.OrdinalIgnoreCase));
                    if (existingJob == null)
                    {
                        newJobs.Add(job);
                    }
                    else
                    {
                        // Ensure existing jobs get the new skill requirements
                        if (existingJob.RequiredSkills != job.RequiredSkills)
                        {
                            existingJob.RequiredSkills = job.RequiredSkills;
                        }
                    }
                }

                if (newJobs.Any())
                {
                    context.Jobs.AddRange(newJobs);
                }
                
                // Always save to persist updates to existing jobs
                context.SaveChanges();

                // Look for any candidates.
                if (context.Candidates.Any())
                {
                    return;   // Candidates already seeded, skipping demo data
                }

                // Candidates (Demo Data)
                // We need to resolve skills from the DB (or our local list) to link them
                var java = existingSkills.First(s => s.SkillName == "Java");
                var python = existingSkills.First(s => s.SkillName == "Python");
                var csharp = existingSkills.First(s => s.SkillName == "C#");
                var sql = existingSkills.First(s => s.SkillName == "SQL");
                var react = existingSkills.First(s => s.SkillName == "React");
                var dsa = existingSkills.First(s => s.SkillName == "Data Structures");
                // Note: C++ might be new, so finding it:
                var cpp = existingSkills.FirstOrDefault(s => s.SkillName == "C++");

                var c1 = new Candidate { FullName = "Alice Smith", ExperienceYears = 5, Education = "BS CS", ExpectedSalary = 90000, ResumeText = "Experienced Java and SQL developer. Good with Algorithms." };
                var c2 = new Candidate { FullName = "Bob Jones", ExperienceYears = 2, Education = "Bootcamp", ExpectedSalary = 60000, ResumeText = "Junior React developer. Learning Python." };
                var c3 = new Candidate { FullName = "Charlie Day", ExperienceYears = 8, Education = "MS CS", ExpectedSalary = 120000, ResumeText = "Senior C# Architect. Expert in Data Structures and High Performance Computing." };
                var c4 = new Candidate { FullName = "Diana Prince", ExperienceYears = 4, Education = "BS Eng", ExpectedSalary = 85000, ResumeText = "Full stack Python and React. SQL optimization expert." };
                var c5 = new Candidate { FullName = "Evan Wright", ExperienceYears = 10, Education = "PhD AI", ExpectedSalary = 150000, ResumeText = "Machine Learning, Python, C++, Advanced Algorithms." };

                context.Candidates.AddRange(c1, c2, c3, c4, c5);
                context.SaveChanges();

                // Candidate Skills
                var csList = new List<CandidateSkill>
                {
                    new CandidateSkill { Candidate = c1, Skill = java },
                    new CandidateSkill { Candidate = c1, Skill = sql },
                    new CandidateSkill { Candidate = c2, Skill = react },
                    new CandidateSkill { Candidate = c2, Skill = python },
                    new CandidateSkill { Candidate = c3, Skill = csharp },
                    new CandidateSkill { Candidate = c3, Skill = dsa },
                    new CandidateSkill { Candidate = c4, Skill = python },
                    new CandidateSkill { Candidate = c4, Skill = react },
                    new CandidateSkill { Candidate = c4, Skill = sql },
                    new CandidateSkill { Candidate = c5, Skill = python },
                    new CandidateSkill { Candidate = c5, Skill = dsa }
                };
                
                if (cpp != null) csList.Add(new CandidateSkill { Candidate = c5, Skill = cpp });

                context.CandidateSkills.AddRange(csList);
                context.SaveChanges();
                // 3. Ensure Roles and Admin User
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                EnsureRoles(roleManager).Wait();
                EnsureAdminUser(userManager).Wait();
            }
        }

        private static async Task EnsureRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task EnsureAdminUser(UserManager<ApplicationUser> userManager)
        {
            var adminUser = await userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser { UserName = "admin", Email = "admin@skillmesh.com", FullName = "System Admin" };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                // Ensure existing admin has the role
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
