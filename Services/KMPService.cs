using JobRankingSystem.Models;

namespace JobRankingSystem.Services
{
    public class KMPService
    {
        public AlgorithmTrace SearchPattern(string text, string pattern)
        {
            var trace = new AlgorithmTrace { AlgorithmName = "KMP Pattern Search" };
            int stepId = 1;

            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(pattern))
                return trace;

            int[] lps = ComputeLPS(pattern, trace, ref stepId);
            
            int i = 0; // index for text
            int j = 0; // index for pattern

            trace.Steps.Add(new AlgorithmStep
            {
                StepId = stepId++,
                Description = "Starting Search",
                Variables = new Dictionary<string, string> { { "Text", text }, { "Pattern", pattern } }
            });

            while (i < text.Length)
            {
                if (pattern[j] == text[i])
                {
                    j++;
                    i++;
                }

                if (j == pattern.Length)
                {
                    trace.Steps.Add(new AlgorithmStep
                    {
                        StepId = stepId++,
                        Description = $"Pattern found at index {i - j}",
                        HighlightIndices = new List<int> { i - j }
                    });
                    j = lps[j - 1];
                }
                else if (i < text.Length && pattern[j] != text[i])
                {
                    if (j != 0)
                    {
                        j = lps[j - 1];
                        trace.Steps.Add(new AlgorithmStep
                        {
                            StepId = stepId++,
                            Description = $"Mismatch at {i}. Jumping 'j' to {j} using LPS.",
                            Variables = new Dictionary<string, string> { { "i", i.ToString() }, { "j", j.ToString() } }
                        });
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            return trace;
        }

        private int[] ComputeLPS(string pattern, AlgorithmTrace trace, ref int stepId)
        {
            int len = 0;
            int i = 1;
            int[] lps = new int[pattern.Length];
            lps[0] = 0;

            trace.Steps.Add(new AlgorithmStep
            {
                StepId = stepId++,
                Description = "Computing LPS Array",
                Variables = new Dictionary<string, string> { { "Pattern", pattern } }
            });

            while (i < pattern.Length)
            {
                if (pattern[i] == pattern[len])
                {
                    len++;
                    lps[i] = len;
                    i++;
                }
                else
                {
                    if (len != 0)
                    {
                        len = lps[len - 1];
                    }
                    else
                    {
                        lps[i] = 0;
                        i++;
                    }
                }
            }
            return lps;
        }
    }
}
