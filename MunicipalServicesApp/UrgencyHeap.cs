using System.Collections.Generic;

namespace MunicipalServicesApp
{
    /// Ranks service requests by urgency using the binary min-heap
    /// (SimplePriorityQueue) built in Part 2. The heap always hands back the
    /// most urgent request first.

    public static class UrgencyHeap
    {
        /// The urgency rule. Returns a negative number if request 'a' should
        /// come out before 'b':
        ///   1. open requests before resolved ones
        ///   2. then the lowest priority number (1 = most urgent)
        ///   3. then the one that has been waiting longest
        ///   4. then the lower ID, so the order is always the same

        private static int Compare(ServiceRequest a, ServiceRequest b)
        {
            int byOpen = (a.IsOpen ? 0 : 1).CompareTo(b.IsOpen ? 0 : 1);
            if (byOpen != 0) return byOpen;

            int byPriority = a.Priority.CompareTo(b.Priority);
            if (byPriority != 0) return byPriority;

            int byDate = a.DateSubmitted.CompareTo(b.DateSubmitted);
            if (byDate != 0) return byDate;

            return a.Id.CompareTo(b.Id);
        }

        /// All requests, most urgent first (resolved ones last).

        public static List<ServiceRequest> SortByUrgency(IEnumerable<ServiceRequest> requests)
        {
            var heap = new SimplePriorityQueue<ServiceRequest>(Compare);
            foreach (ServiceRequest request in requests)
                heap.Enqueue(request);

            var ordered = new List<ServiceRequest>(heap.Count);
            while (heap.Count > 0)
                ordered.Add(heap.Dequeue());
            return ordered;
        }

        /// Just the few most urgent open requests, for a "needs attention" list.

        public static List<ServiceRequest> MostUrgentOpen(IEnumerable<ServiceRequest> requests, int count)
        {
            var heap = new SimplePriorityQueue<ServiceRequest>(Compare);
            foreach (ServiceRequest request in requests)
            {
                if (request.IsOpen)
                    heap.Enqueue(request);
            }

            var top = new List<ServiceRequest>();
            while (heap.Count > 0 && top.Count < count)
                top.Add(heap.Dequeue());
            return top;
        }
    }
}