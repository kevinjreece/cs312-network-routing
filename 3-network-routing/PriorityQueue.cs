using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkRouting
{
    class PriorityQueue
    {
        private HeapElement[] heap;
        private int[] look_up;
        private int nxt_idx;
        private int max_size;

        // Returns the index of the parent of idx
        private int GetParent(int idx)
        {
            if (!HasParent(idx)) { return -1;  }
            return (idx - 1) / 2;
        }

        // Does idx have a parent?
        private bool HasParent(int idx)
        {
            return idx != 0;
        }

        // Returns the left child if is_right is 0
        private int GetChildIdx(int idx, int is_right)
        {
            return (idx * 2) + 1 + is_right;
        }

        // Returns the index of the left child of idx
        private int GetLeftChildIdx(int idx)
        {
            return GetChildIdx(idx, 0);
        }

        // Returns the index of the right child of idx
        private int GetRightChildIdx(int idx)
        {
            return GetChildIdx(idx, 1);
        }

        // Returns the index of the smallest child of idx
        private int GetSmallestChild(int idx)
        {
            if (!HasChild(idx)) { return -1;  }
            if (!HasRightChild(idx)) { return GetLeftChildIdx(idx); }

            int left_idx = GetLeftChildIdx(idx);
            int right_idx = GetRightChildIdx(idx);
            return heap[left_idx].value < heap[right_idx].value ? left_idx : right_idx;
        }

        // Does idx have a left child?
        private bool HasLeftChild(int idx)
        {
            return GetLeftChildIdx(idx) < nxt_idx;
        }

        // Does idx have a right child?
        private bool HasRightChild(int idx)
        {
            return GetRightChildIdx(idx) < nxt_idx;
        }

        // Does idx have any children?
        private bool HasChild(int idx)
        {
            return HasLeftChild(idx);
        }

        // Swaps the position of the two elements
        // Returns the new position of e1
        private int SwapElements(int e1, int e2)
        {
            MoveElement(nxt_idx, e2);
            MoveElement(e2, e1);
            MoveElement(e1, nxt_idx);
            return e2;
        }

        // Percolates the node at idx down until the tree is correct
        private void PercolateDown(int idx)
        {
            int e = idx;
            while (HasChild(e) && heap[GetSmallestChild(e)].value < heap[e].value)
            {
                e = SwapElements(e, GetSmallestChild(e));
            }
        }

        // Percolates the node at idx up until the tree is correct
        private void PercolateUp(int idx)
        {
            int e = idx;
            while (HasParent(e) && heap[GetParent(e)].value > heap[e].value)
            {
                e = SwapElements(e, GetParent(e));
            }
        }

        // Removes the root item and returns it
        public int PopMin()
        {
            int ret = MoveElement(-1, 0);
            nxt_idx--;
            if (IsEmpty()) { return ret; }
            MoveElement(0, nxt_idx);
            PercolateDown(0);
            return ret;
        }

        // Inserts a node (id, val) into the heap
        public bool Insert(int id, double val)
        {
            if (id >= max_size || look_up[id] == -1) { return false; }

            HeapElement e = new HeapElement(id, val);
            heap[nxt_idx] = e;
            look_up[id] = nxt_idx;
            nxt_idx++;
            PercolateUp(look_up[id]);
            return true;
        }

        // Moves an element from index src to index dst in the heap, writing over whatever was there
        // Adjusts look_up with the new location
        // Returns the id of the item being moved
        private int MoveElement(int dst, int src)
        {
            HeapElement e = heap[src];
  
            look_up[e.id] = dst;
            heap[src] = null;
            
            if (dst != -1)
            {
                heap[dst] = e;
            }
            return e.id;
        }

        // Is the heap empty?
        public bool IsEmpty()
        {
            return nxt_idx == 0;
        }

        override public String ToString()
        {
            if (IsEmpty()) { return "Queue is empty"; }

            StringBuilder ret = new StringBuilder();
            List<int> print_queue = new List<int>();
            print_queue.Add(0);
            int count = print_queue.Count();
            while (print_queue.Count() > 0)
            {
                int i = print_queue.ElementAt(0);
                print_queue.RemoveAt(0);
                if (HasLeftChild(i)) { print_queue.Add(GetLeftChildIdx(i)); }
                if (HasRightChild(i)) { print_queue.Add(GetRightChildIdx(i)); }
                ret.Append(" " + heap[i].value.ToString("#.##") + "(" + heap[i].id + ")");
                if (--count == 0)
                {
                    ret.Append("\n");
                    count = print_queue.Count();
                }
            }
            return ret.ToString();
        }

        // Constructor
        public PriorityQueue(int num_elements)
        {
            heap = new HeapElement[num_elements + 1];
            for (int i = 0; i < num_elements + 1; i++)
            {
                heap[i] = null;
            }

            look_up = new int[num_elements];
            for (int i = 0; i < max_size; i++)
            {
                look_up[i] = -1;
            }

            nxt_idx = 0;
            max_size = num_elements;
        }

        public static void TestPriorityQueue()
        {
            Random r = new Random();
            int size = 7;
            int range = 100;
            PriorityQueue q = new PriorityQueue(size);
            for (int i = 0; i < size; i++)
            {
                double val = r.NextDouble() * range;
                if (!q.Insert(i, val))
                {
                    Console.WriteLine("ERROR on index " + i + " and value " + val.ToString("#.##"));
                    return;
                }
                Console.WriteLine("inserted item " + i + " with value " + val.ToString("#.##"));
                Console.WriteLine("queue:" + q.ToString());
            }

            Console.WriteLine();

            for (int i = 0; i < size; i++)
            {
                int id = q.PopMin();
                Console.WriteLine("popped " + id + " off the queue");
                Console.WriteLine("queue:\n" + q.ToString());
            }

            Console.WriteLine("ALL DONE");

        }

        private class HeapElement
        {
            public int id;
            public double value;

            public HeapElement(int _id, double _value)
            {
                id = _id;
                value = _value;
            }
        }
    }
}
