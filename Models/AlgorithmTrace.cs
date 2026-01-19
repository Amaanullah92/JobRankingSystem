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
        public const int MaxSteps = 500;
        public string AlgorithmName { get; set; } = string.Empty;
        public List<AlgorithmStep> Steps { get; set; } = new List<AlgorithmStep>();
        public bool IsTruncated { get; set; } = false;

        public void AddStep(AlgorithmStep step)
        {
            if (Steps.Count >= MaxSteps)
            {
                if (!IsTruncated)
                {
                    IsTruncated = true;
                    Steps.Add(new AlgorithmStep 
                    { 
                        StepId = step.StepId, 
                        Description = "** Trace Truncated (Limit Reached) **",
                        HighlightIndices = new List<int>(),
                        Variables = new Dictionary<string, string>()
                    });
                }
                return;
            }
            Steps.Add(step);
        }
    }
}
