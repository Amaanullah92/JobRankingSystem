using JobRankingSystem.Models;

namespace JobRankingSystem.Services
{
    public class SortingService
    {
        public enum SortType { MergeSort, QuickSort }

        public (List<Candidate> sorted, AlgorithmTrace trace) Sort(List<Candidate> candidates, SortType type)
        {
            var trace = new AlgorithmTrace { AlgorithmName = type.ToString() };
            int stepId = 1;
            var list = new List<Candidate>(candidates);

            if (type == SortType.MergeSort)
            {
                MergeSort(list, 0, list.Count - 1, trace, ref stepId);
            }
            else
            {
                QuickSort(list, 0, list.Count - 1, trace, ref stepId);
            }

            return (list, trace);
        }

        private void MergeSort(List<Candidate> list, int left, int right, AlgorithmTrace trace, ref int stepId)
        {
            if (left < right)
            {
                int mid = (left + right) / 2;
                MergeSort(list, left, mid, trace, ref stepId);
                MergeSort(list, mid + 1, right, trace, ref stepId);
                Merge(list, left, mid, right, trace, ref stepId);
            }
        }

        private void Merge(List<Candidate> list, int left, int mid, int right, AlgorithmTrace trace, ref int stepId)
        {
            // Simplified merge logic for brevity, focusing on visualization hook
            trace.Steps.Add(new AlgorithmStep
            {
                StepId = stepId++,
                Description = $"Merging range [{left}, {mid}] and [{mid+1}, {right}]",
                HighlightIndices = Enumerable.Range(left, right - left + 1).ToList()
            });

            // Actual logic omitted for brevity in prompt context but essentially: 
            // Create temps, copy data, merge back. 
            // Since this is a core requirement, I'll write a full one.
            
            int n1 = mid - left + 1;
            int n2 = right - mid;

            var L = new List<Candidate>();
            var R = new List<Candidate>();

            for (int i = 0; i < n1; i++) L.Add(list[left + i]);
            for (int j = 0; j < n2; j++) R.Add(list[mid + 1 + j]);

            int k = left;
            int ii = 0, jj = 0;

            while (ii < n1 && jj < n2)
            {
                if (L[ii].ExpectedSalary <= R[jj].ExpectedSalary) // Sort by Salary Asc
                {
                    list[k] = L[ii];
                    ii++;
                }
                else
                {
                    list[k] = R[jj];
                    jj++;
                }
                k++;
            }

            while (ii < n1) { list[k] = L[ii]; ii++; k++; }
            while (jj < n2) { list[k] = R[jj]; jj++; k++; }
        }

        private void QuickSort(List<Candidate> list, int low, int high, AlgorithmTrace trace, ref int stepId)
        {
            if (low < high)
            {
                int pi = Partition(list, low, high, trace, ref stepId);
                QuickSort(list, low, pi - 1, trace, ref stepId);
                QuickSort(list, pi + 1, high, trace, ref stepId);
            }
        }

        private int Partition(List<Candidate> list, int low, int high, AlgorithmTrace trace, ref int stepId)
        {
            var pivot = list[high];
            int i = (low - 1);

            for (int j = low; j < high; j++)
            {
                // Sort by ExperienceYears Descending for QuickSort example
                if (list[j].ExperienceYears > pivot.ExperienceYears) 
                {
                    i++;
                    Swap(list, i, j);
                }
            }
            Swap(list, i + 1, high);
            
            trace.Steps.Add(new AlgorithmStep
            {
                StepId = stepId++,
                Description = $"Partition around pivot {pivot.ExperienceYears} at index {high}. New Pivot Pos: {i+1}",
                HighlightIndices = new List<int> { i+1 }
            });

            return i + 1;
        }

        private void Swap(List<Candidate> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
