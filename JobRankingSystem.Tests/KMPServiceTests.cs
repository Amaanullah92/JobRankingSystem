using JobRankingSystem.Services;
using Xunit;
using System.Linq;

namespace JobRankingSystem.Tests
{
    public class KMPServiceTests
    {
        [Fact]
        public void SearchPattern_ShouldFindPattern()
        {
            // Arrange
            var service = new KMPService();
            string text = "DSA is important for Software Engineers.";
            string pattern = "Software";

            // Act
            var trace = service.SearchPattern(text, pattern);

            // Assert
            Assert.Contains(trace.Steps, s => s.Description.Contains("Pattern found"));
        }

        [Fact]
        public void SearchPattern_ShouldNotFindMissingPattern()
        {
            // Arrange
            var service = new KMPService();
            string text = "Hello World";
            string pattern = "Python";

            // Act
            var trace = service.SearchPattern(text, pattern);

            // Assert
            Assert.DoesNotContain(trace.Steps, s => s.Description.Contains("Pattern found"));
        }
    }
}
