using JobRankingSystem.Models;

namespace JobRankingSystem.Services
{
    public class DPFitScoreService
    {
        // Use DP implementation used in "Longest Common Subsequence" (LCS) 
        // to find similarity between Job Requirements (List of skills) and Candidate Skills.
        // Similarity score = LCS length / Job Skills length.

        public (double score, AlgorithmTrace trace) CalculateFitScore(List<string> jobSkills, List<string> candidateSkills)
        {
            var trace = new AlgorithmTrace { AlgorithmName = "DP Fit Score (LCS)" };
            int stepId = 1;

            int m = jobSkills.Count;
            int n = candidateSkills.Count;
            int[,] dp = new int[m + 1, n + 1];

            // Initialize DP table
            trace.Steps.Add(new AlgorithmStep
            {
                StepId = stepId++,
                Description = "Initialized DP Table",
                Variables = new Dictionary<string, string> { { "Rows", m.ToString() }, { "Cols", n.ToString() } }
            });

            for (int i = 0; i <= m; i++)
            {
                for (int j = 0; j <= n; j++)
                {
                    if (i == 0 || j == 0)
                        dp[i, j] = 0;
                    else if (jobSkills[i - 1].Equals(candidateSkills[j - 1], StringComparison.OrdinalIgnoreCase))
                    {
                        dp[i, j] = dp[i - 1, j - 1] + 1;
                        
                        trace.Steps.Add(new AlgorithmStep
                        {
                            StepId = stepId++,
                            Description = $"Match found: {jobSkills[i - 1]}",
                            Variables = new Dictionary<string, string> 
                            { 
                                { "Skill", jobSkills[i - 1] }, 
                                { "DP[i,j]", dp[i, j].ToString() } 
                            },
                        });
                    }
                    else
                    {
                        dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                    }
                }
            }

            int lcs = dp[m, n];
            double score = m > 0 ? (double)lcs / m * 100 : 0;

            trace.Steps.Add(new AlgorithmStep
            {
                StepId = stepId++,
                Description = "Validation Complete",
                Variables = new Dictionary<string, string> 
                { 
                    { "LCS Length", lcs.ToString() }, 
                    { "Max Possible", m.ToString() }, 
                    { "Fit Score", score.ToString("F2") } 
                }
            });

            return (score, trace);
        }
    }
}
