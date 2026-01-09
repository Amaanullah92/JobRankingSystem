using JobRankingSystem.Models;

namespace JobRankingSystem.Services
{
    public class HashIndexService
    {
        // Custom Hash Table Implementation
        // Mapping Skill Name -> List of Candidates

        public class HashNode
        {
            public string Key; // Skill Name
            public List<Candidate> Value; // Candidates
            public HashNode Next; // For chaining
        }

        private HashNode[] _buckets;
        private int _size;
        
        public HashIndexService(int size = 10)
        {
            _size = size;
            _buckets = new HashNode[size];
        }

        private int GetBucketIndex(string key)
        {
            int hash = Math.Abs(key.GetHashCode());
            return hash % _size;
        }

        public AlgorithmTrace BuildIndex(List<Candidate> candidates)
        {
            var trace = new AlgorithmTrace { AlgorithmName = "Hash Table Construction" };
            int stepId = 1;

            foreach (var c in candidates)
            {
                foreach (var s in c.CandidateSkills)
                {
                    string key = s.Skill.SkillName;
                    int index = GetBucketIndex(key);

                    trace.Steps.Add(new AlgorithmStep
                    {
                        StepId = stepId++,
                        Description = $"Hashing skill '{key}' for Candidate {c.FullName}",
                        Variables = new Dictionary<string, string> { 
                            { "Key", key }, 
                            { "Hash", key.GetHashCode().ToString() },
                            { "Bucket", index.ToString() }
                        }
                    });

                    // Insert into table (Chaining)
                    if (_buckets[index] == null)
                    {
                        _buckets[index] = new HashNode { Key = key, Value = new List<Candidate> { c } };
                    }
                    else
                    {
                        // Traverse chain
                        var head = _buckets[index];
                        while (head != null)
                        {
                            if (head.Key == key)
                            {
                                head.Value.Add(c);
                                trace.Steps.Add(new AlgorithmStep
                                {
                                    StepId = stepId++,
                                    Description = $"Collision! Key '{key}' exists. Appending candidate.",
                                    Variables = new Dictionary<string, string> { { "Collision", "True" } }
                                });
                                break;
                            }
                            if (head.Next == null)
                            {
                                head.Next = new HashNode { Key = key, Value = new List<Candidate> { c } };
                                trace.Steps.Add(new AlgorithmStep
                                {
                                    StepId = stepId++,
                                    Description = $"Collision! Key '{key}' not found in chain. Adding new node end of chain.",
                                    Variables = new Dictionary<string, string> { { "Collision", "True" }, { "Action", "Chaining" } }
                                });
                                break;
                            }
                            head = head.Next;
                        }
                    }
                }
            }

            return trace;
        }
    }
}
