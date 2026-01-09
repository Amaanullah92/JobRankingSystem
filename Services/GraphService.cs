using JobRankingSystem.Models;

namespace JobRankingSystem.Services
{
    public class GraphService
    {
        // Adjacency List: Skill Name -> List of Related Skills with Weight
        public Dictionary<string, Dictionary<string, int>> AdjacencyList = new Dictionary<string, Dictionary<string, int>>();

        public AlgorithmTrace BuildSkillGraph(List<Candidate> candidates)
        {
            var trace = new AlgorithmTrace { AlgorithmName = "Graph Construction (Skill Similarity)" };
            int stepId = 1;

            AdjacencyList.Clear();

            foreach (var c in candidates)
            {
                var skills = c.CandidateSkills.Select(s => s.Skill.SkillName).ToList();
                
                // For every pair of skills in the candidate's profile, add an edge
                for (int i = 0; i < skills.Count; i++)
                {
                    for (int j = i + 1; j < skills.Count; j++)
                    {
                        string u = skills[i];
                        string v = skills[j];

                        AddEdge(u, v);
                        AddEdge(v, u); // Undirected

                        trace.Steps.Add(new AlgorithmStep
                        {
                            StepId = stepId++,
                            Description = $"Added Edge between '{u}' and '{v}' (Co-occurrence)",
                            Variables = new Dictionary<string, string> 
                            { 
                                { "Skill A", u }, 
                                { "Skill B", v },
                                { "Weight", AdjacencyList[u][v].ToString() }
                            }
                        });
                    }
                }
            }

            return trace;
        }

        private void AddEdge(string u, string v)
        {
            if (!AdjacencyList.ContainsKey(u))
                AdjacencyList[u] = new Dictionary<string, int>();

            if (!AdjacencyList[u].ContainsKey(v))
                AdjacencyList[u][v] = 0;

            AdjacencyList[u][v]++;
        }

        public List<string> GetRelatedSkills(string skill)
        {
            if (AdjacencyList.ContainsKey(skill))
            {
                // Return top 5 related skills sorted by weight
                return AdjacencyList[skill]
                    .OrderByDescending(x => x.Value)
                    .Take(5)
                    .Select(x => x.Key)
                    .ToList();
            }
            return new List<string>();
        }
    }
}
