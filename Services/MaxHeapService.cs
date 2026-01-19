using JobRankingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JobRankingSystem.Services
{
    public class MaxHeapService
    {
        // Now supports custom comparers! (e.g., sort by Salary, Experience, etc.)
        public (List<Candidate> sortedCandidates, AlgorithmTrace trace) SortCandidates(List<Candidate> candidates, IComparer<Candidate>? comparer = null)
        {
            var trace = new AlgorithmTrace { AlgorithmName = "Max Heap Sort" };
            int stepCounter = 1;
            
            // Default to ExperienceYears if no comparer provided
            comparer ??= Comparer<Candidate>.Create((a, b) => a.ExperienceYears.CompareTo(b.ExperienceYears));

            var heap = new List<Candidate>(candidates); 
            int n = heap.Count;

            // Build heap (rearrange array)
            for (int i = n / 2 - 1; i >= 0; i--)
            {
                Heapify(heap, n, i, trace, ref stepCounter, comparer);
            }

            // Extract elements
            for (int i = n - 1; i > 0; i--)
            {
                trace.AddStep(new AlgorithmStep
                {
                    StepId = stepCounter++,
                    Description = $"Swap root (Max) with element at index {i}",
                    StateSnapshot = heap.Select(c => c.Id).ToList(),
                    HighlightIndices = new List<int> { 0, i }
                });

                var temp = heap[0];
                heap[0] = heap[i];
                heap[i] = temp;

                Heapify(heap, i, 0, trace, ref stepCounter, comparer);
            }

            // Note: Max Heap Sort produces Ascending order (lowest first) if popping Max to end.
            // But usually users want Descending (Best First). 
            // So we reverse it at the end to get Top-Down.
            heap.Reverse();

            return (heap, trace);
        }

        private void Heapify(List<Candidate> heap, int n, int i, AlgorithmTrace trace, ref int stepCounter, IComparer<Candidate> comparer)
        {
            int largest = i; 
            int left = 2 * i + 1; 
            int right = 2 * i + 2; 

            // Use Comparer to check if left > largest
            if (left < n && comparer.Compare(heap[left], heap[largest]) > 0)
                largest = left;

            // Use Comparer to check if right > largest
            if (right < n && comparer.Compare(heap[right], heap[largest]) > 0)
                largest = right;

            if (largest != i)
            {
                trace.AddStep(new AlgorithmStep
                {
                    StepId = stepCounter++,
                    Description = $"Heapify: Swap index {i} with {largest} (Child larger than Parent)",
                    StateSnapshot = heap.Select(c => c.Id).ToList(),
                    HighlightIndices = new List<int> { i, largest }
                });

                var swap = heap[i];
                heap[i] = heap[largest];
                heap[largest] = swap;

                Heapify(heap, n, largest, trace, ref stepCounter, comparer);
            }
        }
    }
}
