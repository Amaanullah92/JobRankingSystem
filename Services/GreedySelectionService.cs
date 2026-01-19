using JobRankingSystem.Models;

namespace JobRankingSystem.Services
{
    public class GreedySelectionService
    {
        // Greedy Algorithm: Select best candidates within a total budget
        // Strategy: Sort by Ratio (Experience / Salary) or just Experience if Salary is within budget?
        // Let's maximize Experience for a given Budget.
        // Or simply "Select Top N candidates who fit within Budget"

        public (List<Candidate> selected, AlgorithmTrace trace) SelectCandidates(List<Candidate> candidates, decimal budget)
        {
            var trace = new AlgorithmTrace { AlgorithmName = "Greedy Selection (Max Experience for Budget)" };
            int stepId = 1;

            // 1. Sort by "Value" (Heuristic: Experience / Salary gives 'bang for buck', or just simplified: Sort by Experience/low salary)
            // Let's try to Maximize Total Experience. This maps to the Knapsack problem (0/1), which is DP.
            // But if we want Greedy, we can pick the "Best Ratio" or just "Cheapest High Exp".
            // Let's use a simpler Greedy heuristic: Pick highest experience candidates as long as they fit in the remaining budget.
            
            var sortedCandidates = candidates.OrderByDescending(c => c.ExperienceYears).ToList();
            
            trace.AddStep(new AlgorithmStep 
            { 
                StepId = stepId++, 
                Description = "Sorted candidates by Experience (Greedy Heuristic)",
                Variables = new Dictionary<string, string> { { "Total Budget", budget.ToString() } }
            });

            var selected = new List<Candidate>();
            decimal currentCost = 0;

            foreach (var c in sortedCandidates)
            {
                if (currentCost + c.ExpectedSalary <= budget)
                {
                    selected.Add(c);
                    currentCost += c.ExpectedSalary;
                     trace.AddStep(new AlgorithmStep
                    {
                        StepId = stepId++,
                        Description = $"Selected Candidate {c.FullName} (Exp: {c.ExperienceYears}, Salary: {c.ExpectedSalary})",
                        Variables = new Dictionary<string, string> 
                        { 
                            { "Current Cost", currentCost.ToString() }, 
                            { "Remaining", (budget - currentCost).ToString() } 
                        }
                    });
                }
                else
                {
                     trace.AddStep(new AlgorithmStep
                    {
                        StepId = stepId++,
                        Description = $"Skipped Candidate {c.FullName} (Salary {c.ExpectedSalary} exceeds remaining budget)",
                        Variables = new Dictionary<string, string> 
                        { 
                            { "Current Cost", currentCost.ToString() }, 
                            { "Attempted Add", c.ExpectedSalary.ToString() } 
                        }
                    });
                }
            }

            return (selected, trace);
        }
    }
}
