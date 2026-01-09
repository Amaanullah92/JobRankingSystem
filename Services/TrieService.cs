using JobRankingSystem.Models;

namespace JobRankingSystem.Services
{
    public class TrieService
    {
        public class TrieNode
        {
            public Dictionary<char, TrieNode> Children = new Dictionary<char, TrieNode>();
            public bool IsEndOfWord;
        }

        private TrieNode _root = new TrieNode();

        public AlgorithmTrace BuildTrie(List<string> skills)
        {
            var trace = new AlgorithmTrace { AlgorithmName = "Trie Construction" };
            int stepId = 1;

            foreach (var skill in skills)
            {
                Insert(skill, trace, ref stepId);
            }
            return trace;
        }

        public void Insert(string word, AlgorithmTrace trace, ref int stepId)
        {
            var node = _root;
            trace.Steps.Add(new AlgorithmStep
            {
                StepId = stepId++,
                Description = $"Inserting word: {word}",
                Variables = new Dictionary<string, string> { { "Word", word } }
            });

            foreach (var ch in word.ToLower())
            {
                if (!node.Children.ContainsKey(ch))
                {
                    node.Children[ch] = new TrieNode();
                }
                node = node.Children[ch];
            }
            node.IsEndOfWord = true;
        }

        public (List<string>, AlgorithmTrace) AutoComplete(string prefix)
        {
            var results = new List<string>();
            var trace = new AlgorithmTrace { AlgorithmName = "Trie Search" };
            int stepId = 1;

            if (string.IsNullOrEmpty(prefix)) return (results, trace);

            var node = _root;
            
            trace.Steps.Add(new AlgorithmStep 
            { 
                StepId = stepId++, 
                Description = $"Starting search for prefix: '{prefix}'",
                Variables = new Dictionary<string, string> { { "Node", "ROOT" } }
            });

            foreach (var ch in prefix.ToLower())
            {
                if (!node.Children.ContainsKey(ch)) 
                {
                    trace.Steps.Add(new AlgorithmStep 
                    { 
                        StepId = stepId++, 
                        Description = $"Character '{ch}' not found. Search ends.",
                        Variables = new Dictionary<string, string> { { "Missing Char", ch.ToString() } }
                    });
                    return (results, trace);
                }

                trace.Steps.Add(new AlgorithmStep 
                { 
                    StepId = stepId++, 
                    Description = $"Traversing to child node: '{ch}'",
                    Variables = new Dictionary<string, string> { { "Current Char", ch.ToString() } }
                });

                node = node.Children[ch];
            }
            
            trace.Steps.Add(new AlgorithmStep 
            { 
                StepId = stepId++, 
                Description = $"Prefix found. Collecting all words from this node...",
                Variables = new Dictionary<string, string> { { "Prefix", prefix } }
            });

            FindWords(node, prefix.ToLower(), results);
            
            trace.Steps.Add(new AlgorithmStep 
            { 
                StepId = stepId++, 
                Description = $"Search complete. Found {results.Count} matches.",
                Variables = new Dictionary<string, string> { { "Matches", string.Join(", ", results.Take(3)) + (results.Count > 3 ? "..." : "") } }
            });

            return (results, trace);
        }

        private void FindWords(TrieNode node, string currentWord, List<string> results)
        {
            if (node.IsEndOfWord)
                results.Add(currentWord);

            foreach (var child in node.Children)
            {
                FindWords(child.Value, currentWord + child.Key, results);
            }
        }
    }
}
