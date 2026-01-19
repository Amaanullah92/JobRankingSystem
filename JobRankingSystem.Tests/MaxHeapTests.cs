using JobRankingSystem.Models;
using JobRankingSystem.Services;
using System.Collections.Generic;
using Xunit;
using System.Linq;

namespace JobRankingSystem.Tests
{
    public class MaxHeapTests
    {
        [Fact]
        public void SortCandidates_ShouldSortByExperienceDescending()
        {
            // Arrange
            var service = new MaxHeapService();
            var candidates = new List<Candidate>
            {
                new Candidate { Id = 1, FullName = "A", ExperienceYears = 2 },
                new Candidate { Id = 2, FullName = "B", ExperienceYears = 5 },
                new Candidate { Id = 3, FullName = "C", ExperienceYears = 1 }
            };

            // Act
            var (sorted, trace) = service.SortCandidates(candidates);

            // Assert
            // Max Experience is 5 (Id 2), then 2 (Id 1), then 1 (Id 3)
            Assert.Equal(2, sorted[0].Id); 
            
            Assert.Equal(5, sorted[0].ExperienceYears);
            Assert.Equal(2, sorted[1].ExperienceYears);
            Assert.Equal(1, sorted[2].ExperienceYears);
        }

        [Fact]
        public void SortCandidates_WithSalaryComparer_ShouldSortBySalary()
        {
            // Arrange
            var service = new MaxHeapService();
            var candidates = new List<Candidate>
            {
                new Candidate { Id = 1, ExpectedSalary = 1000 },
                new Candidate { Id = 2, ExpectedSalary = 3000 },
                new Candidate { Id = 3, ExpectedSalary = 2000 }
            };

            var comparer = Comparer<Candidate>.Create((a, b) => a.ExpectedSalary.CompareTo(b.ExpectedSalary));

            // Act
            var (sorted, trace) = service.SortCandidates(candidates, comparer);

            // Assert
            Assert.Equal(3000, sorted[0].ExpectedSalary);
            Assert.Equal(2000, sorted[1].ExpectedSalary);
            Assert.Equal(1000, sorted[2].ExpectedSalary);
        }
    }
}
