using JobRankingSystem.Models;

namespace JobRankingSystem.Services
{
    public class MaxHeapService
    {
        // A generic Max Heap could be useful, but let's stick to Candidate ranking based on fit score or experience?
        // Requirement: "Rank candidates", "Fast extraction of Top-N"
        // We will store Candidates. We need a comparison criteria. Let's assume we rank by specific score passed in.
        
        // For visualization, we need to return the trace.

        public (List<Candidate> sortedCandidates, AlgorithmTrace trace) SortCandidates(List<Candidate> candidates)
        {
            var trace = new AlgorithmTrace { AlgorithmName = "Max Heap Sort" };
            int stepCounter = 1;

            // 1. Build Heap
            // We'll simulate heap structure with an array/list
            var heap = new List<Candidate>(candidates); 
            // Note: In a real heap sort, we heapify first. 
            // Visualization needs to show the array state at each swap.
            
            int n = heap.Count;

            // Build heap (rearrange array)
            for (int i = n / 2 - 1; i >= 0; i--)
            {
                Heapify(heap, n, i, trace, ref stepCounter);
            }

            // One by one extract an element from heap
            for (int i = n - 1; i > 0; i--)
            {
                // Move current root to end
                trace.Steps.Add(new AlgorithmStep
                {
                    StepId = stepCounter++,
                    Description = $"Swap root (Max) with element at index {i}",
                    StateSnapshot = heap.Select(c => c.Id).ToList(), // Snapshot IDs or Names?
                    HighlightIndices = new List<int> { 0, i }
                });

                var temp = heap[0];
                heap[0] = heap[i];
                heap[i] = temp;

                // call max heapify on the reduced heap
                Heapify(heap, i, 0, trace, ref stepCounter);
            }

            // Reverse to get descending order (Max Heap Sort usually gives ascending if we put max at end, 
            // but for "Top N" we want Max first. So we can just reverse the result or change the logic.)
            // Standard heap sort moves Max to the end. So the array is Ascending.
            // We want Descending for "Top N". So let's reverse it.
            heap.Reverse();

            return (heap, trace);
        }

        private void Heapify(List<Candidate> heap, int n, int i, AlgorithmTrace trace, ref int stepCounter)
        {
            int largest = i; // Initialize largest as root
            int left = 2 * i + 1; // left = 2*i + 1
            int right = 2 * i + 2; // right = 2*i + 2

            // If left child is larger than root (Using ExperienceYears as proxy for rank for now, or ExpectedSalary? Let's use ExperienceYears)
            if (left < n && heap[left].ExperienceYears > heap[largest].ExperienceYears)
                largest = left;

            // If right child is larger than largest so far
            if (right < n && heap[right].ExperienceYears > heap[largest].ExperienceYears)
                largest = right;

            // If largest is not root
            if (largest != i)
            {
                trace.Steps.Add(new AlgorithmStep
                {
                    StepId = stepCounter++,
                    Description = $"Heapify: Swap index {i} with {largest} (Child larger than Parent)",
                    StateSnapshot = heap.Select(c => c.Id).ToList(),
                    HighlightIndices = new List<int> { i, largest }
                });

                var swap = heap[i];
                heap[i] = heap[largest];
                heap[largest] = swap;

                // Recursively heapify the affected sub-tree
                Heapify(heap, n, largest, trace, ref stepCounter);
            }
        }
    }
}
