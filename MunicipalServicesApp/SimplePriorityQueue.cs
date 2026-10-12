using System;
using System.Collections.Generic;

namespace MunicipalServicesApp
{
    public class SimplePriorityQueue<T>
    {
        private readonly List<T> heap = new List<T>();
        private readonly Comparison<T> compare;

        /// <param name="comparison">
        /// Decides which of two items is more urgent. Returns a negative number
        /// if the first item should come out before the second.
        /// </param>
        public SimplePriorityQueue(Comparison<T> comparison)
        {
            if (comparison == null)
                throw new ArgumentNullException("comparison");
            compare = comparison;
        }

        public int Count
        {
            get { return heap.Count; }
        }

        /// Adds an item, then moves it up the heap until it sits below
        /// something at least as urgent as itself.
       
        public void Enqueue(T item)
        {
            heap.Add(item);
            SiftUp(heap.Count - 1);
        }

        public T Dequeue()
        {
            if (heap.Count == 0)
                throw new InvalidOperationException("The priority queue is empty.");

            T top = heap[0];

            // Move the last item to the top, then push it down into place
            int last = heap.Count - 1;
            heap[0] = heap[last];
            heap.RemoveAt(last);

            if (heap.Count > 0)
                SiftDown(0);

            return top;
        }

        public T Peek()
        {
            if (heap.Count == 0)
                throw new InvalidOperationException("The priority queue is empty.");
            return heap[0];
        }

        // Swap a new item upward while it is more urgent than its parent
        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (compare(heap[index], heap[parent]) >= 0)
                    break;

                Swap(index, parent);
                index = parent;
            }
        }

        // Swap an item downward while a child is more urgent than it
        private void SiftDown(int index)
        {
            int count = heap.Count;
            while (true)
            {
                int left = 2 * index + 1;
                int right = left + 1;
                int mostUrgent = index;

                if (left < count && compare(heap[left], heap[mostUrgent]) < 0)
                    mostUrgent = left;
                if (right < count && compare(heap[right], heap[mostUrgent]) < 0)
                    mostUrgent = right;

                if (mostUrgent == index)
                    break;

                Swap(index, mostUrgent);
                index = mostUrgent;
            }
        }

        private void Swap(int a, int b)
        {
            T temp = heap[a];
            heap[a] = heap[b];
            heap[b] = temp;
        }
    }
}