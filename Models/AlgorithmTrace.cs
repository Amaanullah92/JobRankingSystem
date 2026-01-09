namespace JobRankingSystem.Models
{
    public class AlgorithmStep
    {
        public int StepId { get; set; }
        public string Description { get; set; } = string.Empty;
        public object? StateSnapshot { get; set; } // The state of the data structure
        public List<int> HighlightIndices { get; set; } = new List<int>(); // Indices to highlight (e.g. array 
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>(); // Key variables like pointers
    }

    public class AlgorithmTrace
    {
        public string AlgorithmName { get; set; } = string.Empty;
        public List<AlgorithmStep> Steps { get; set; } = new List<AlgorithmStep>();
    }
}
