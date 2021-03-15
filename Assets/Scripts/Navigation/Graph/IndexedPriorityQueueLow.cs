
namespace Thot.GameAI
{
    using System.Collections.Generic;
	
	using UnityEngine;
	
	/// <summary>
    /// Priority queue based on an index into a set of keys. The queue is maintained as a 2-way
    /// heap. The lowest valued key has the highest priority in this implementation.
    /// </summary>
	public sealed class IndexedPriorityQueueLow 
	{
		private readonly List<float> _keys;
        private readonly List<int> _heap;
        private readonly List<int> _invHeap;
        private readonly int _maximumSize;
        private int _size;

        /// <summary>
        /// Initializes a new instance of the IndexedPriorityQueueLow class given the list that
        /// will be indexed into and the maximum size of the queue.
        /// </summary>
        /// <param name="keys">List of keys.</param>
        /// <param name="maximumSize">Maximum size of the priority queue.</param>
        public IndexedPriorityQueueLow(List<float> keys, int maximumSize)
        {
            _keys = keys;
            _maximumSize = maximumSize;
            _size = 0;
            _heap = new List<int>(maximumSize + 1);

            for (int i = 0; i < maximumSize + 1; i++)
            {
                _heap.Add(0);
            }

            _invHeap = new List<int>(maximumSize + 1);

            for (int i = 0; i < maximumSize + 1; i++)
            {
                _invHeap.Add(0);
            }
        }

        /// <summary>
        /// Tests if the priority queue is empty.
        /// </summary>
        /// <returns>True if the priority queue is empty. Otherwise, false.</returns>
        public bool Empty()
        {
            return _size == 0;
        }

        /// <summary>
        /// Insert an item into the queue. The item gets added to the end of the heap
        /// and then the heap is reordered from the bottom up.
        /// </summary>
        /// <param name="index">The index to insert.</param>
        public void Insert(int index)
        {
            if (_size + 1 > _maximumSize)
            {
                Debug.LogError("IndexedPriorityQueue.Insert: overflow.");
                throw new System.Exception("IndexedPriorityQueue.Insert: overflow.");
            }

            ++_size;

            _heap[_size] = index;

            _invHeap[index] = _size;

            ReorderUpwards(_size);
        }

        /// <summary>
        /// Pop the minimum item. To get the minimum item the first element is exchanged with the
        /// lowest in the heap and then the heap is reordered from the top down. 
        /// </summary>
        /// <returns>The minimum item.</returns>
        public int Pop()
        {
            Swap(1, _size);

            ReorderDownwards(1, _size - 1);

            return _heap[_size--];
        }

        /// <summary>
        /// Change the priority of an item. If the value of one of the client key's changes then
        /// call this with the key's index to adjust the queue accordingly.
        /// </summary>
        /// <param name="index">The index of the item to change.</param>
        public void ChangePriority(int index)
        {
            ReorderUpwards(_invHeap[index]);
        }

        private void Swap(int a, int b)
        {
            int temp = _heap[a];
            _heap[a] = _heap[b];
            _heap[b] = temp;

            // change the handles too
            _invHeap[_heap[a]] = a;
            _invHeap[_heap[b]] = b;
        }

        private void ReorderUpwards(int nodeIndex)
        {
            // move up the heap swapping the elements until the heap is ordered
            while ((nodeIndex > 1) &&
                (_keys[_heap[nodeIndex / 2]] > _keys[_heap[nodeIndex]]))
            {
                Swap(nodeIndex / 2, nodeIndex);
                nodeIndex /= 2;
            }
        }

        private void ReorderDownwards(int nodeIndex, int heapSize)
        {
            // move down the heap from node nodeIndex swapping the elements until
            // the heap is reordered
            while (2 * nodeIndex <= heapSize)
            {
                int child = 2 * nodeIndex;

                // set child to smaller of nodeIndex's two children
                if ((child < heapSize) &&
                    (_keys[_heap[child]] > _keys[_heap[child + 1]]))
                {
                    ++child;
                }

                // if this nodeIndex is larger than its child, swap
                if (_keys[_heap[nodeIndex]] <= _keys[_heap[child]])
                {
                    break;
                }

                Swap(child, nodeIndex);

                // move the current node down the tree
                nodeIndex = child;
            }
        }
	}
}
