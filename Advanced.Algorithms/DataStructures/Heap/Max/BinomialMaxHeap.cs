﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Advanced.Algorithms.DataStructures
{
    public class BinomialMaxHeap<T> where T : IComparable
    {
        private Dictionary<T, List<BinomialHeapNode<T>>> heapMapping
           = new Dictionary<T, List<BinomialHeapNode<T>>>();

        public int Count { get; private set; }

        private DoublyLinkedList<BinomialHeapNode<T>> heapForest
            = new DoublyLinkedList<BinomialHeapNode<T>>();

        /// <summary>
        /// O(log(n)) complexity
        /// </summary>
        /// <param name="newItem"></param>
        public void Insert(T newItem)
        {
            var newNode = new BinomialHeapNode<T>(newItem);

            var newHeapForest = new DoublyLinkedList<BinomialHeapNode<T>>();
            newHeapForest.InsertFirst(newNode);

            //updated pointer
            mergeSortedForests(newHeapForest);

            meld();

            addMapping(newItem, newNode);

            Count++;
        }

        /// <summary>
        /// O(log(n)) complexity
        /// </summary>
        /// <returns></returns>
        public T ExtractMax()
        {
            if (heapForest.Head == null)
            {
                throw new Exception("Empty heap");
            }

            var maxTree = heapForest.Head;
            var current = heapForest.Head;

            //find maximum tree
            while (current.Next != null)
            {
                current = current.Next;

                if (maxTree.Data.Value.CompareTo(current.Data.Value) < 0)
                {
                    maxTree = current;
                }
            }

            //remove tree root
            heapForest.Delete(maxTree);

            var newHeapForest = new DoublyLinkedList<BinomialHeapNode<T>>();
            //add removed roots children as new trees to forest
            foreach (var child in maxTree.Data.Children)
            {
                child.Parent = null;
                newHeapForest.InsertLast(child);
            }

            mergeSortedForests(newHeapForest);

            meld();

            removeMapping(maxTree.Data.Value, maxTree.Data);

            Count--;

            return maxTree.Data.Value;
        }

        /// <summary>
        /// Update the Heap with new value for this node pointer
        /// O(log(n)) complexity
        /// </summary>
        public void IncrementKey(T currentValue, T newValue)
        {
            var node = heapMapping[currentValue]?.Where(x => x.Value.Equals(currentValue)).FirstOrDefault();

            if (node == null)
            {
                throw new Exception("Current value is not present in this heap.");
            }

            if (newValue.CompareTo(node.Value) < 0)
            {
                throw new Exception("New value is not greater than old value.");
            }

            updateNodeValue(currentValue, newValue, node);

            var current = node;

            while (current.Parent != null
                && current.Value.CompareTo(current.Parent.Value) > 0)
            {
                //swap parent with child
                var tmp = current.Value;
                updateNodeValue(tmp, current.Parent.Value, current);
                updateNodeValue(current.Parent.Value, tmp, current.Parent);

                current = current.Parent;
            }
        }

        /// <summary>
        /// Unions this heap with another
        /// O(log(n)) complexity
        /// </summary>
        /// <param name="binomialHeap"></param>
        public void Union(BinomialMaxHeap<T> binomialHeap)
        {
            mergeSortedForests(binomialHeap.heapForest);

            meld();

            Count += binomialHeap.Count;
        }

        /// <summary>
        /// O(log(n)) complexity
        /// </summary>
        /// <returns></returns>
        public T PeekMax()
        {
            if (heapForest.Head == null)
            {
                throw new Exception("Empty heap");
            }

            var maxTree = heapForest.Head;
            var current = heapForest.Head;

            //find maximum tree
            while (current.Next != null)
            {
                current = current.Next;

                if (maxTree.Data.Value.CompareTo(current.Data.Value) < 0)
                {
                    maxTree = current;
                }
            }

            return maxTree.Data.Value;
        }

        /// <summary>
        /// Merge roots with same degrees in Forest 
        /// </summary>
        private void meld()
        {
            if (heapForest.Head == null)
            {
                return;
            }


            var cur = heapForest.Head;
            var next = heapForest.Head.Next;

            while (next != null)
            {
                //case 1
                //degrees are differant 
                //we are good to move ahead
                if (cur.Data.Degree != next.Data.Degree)
                {
                    cur = next;
                    next = cur.Next;
                }
                //degress of cur & next are same
                else
                {
                    //case 2 next degree equals next-next degree
                    if (next.Next != null &&
                        cur.Data.Degree == next.Next.Data.Degree)
                    {
                        cur = next;
                        next = cur.Next;
                        continue;
                    }

                    //case 3 cur value is less than next
                    if (cur.Data.Value.CompareTo(next.Data.Value) >= 0)
                    {
                        //add next as child of current
                        cur.Data.Children.Add(next.Data);
                        next.Data.Parent = cur.Data;
                        heapForest.Delete(next);

                        next = cur.Next;
                        continue;
                    }

                    //case 4 cur value is greater than next
                    if (cur.Data.Value.CompareTo(next.Data.Value) < 0)
                    {
                        //add current as child of next
                        next.Data.Children.Add(cur.Data);
                        cur.Data.Parent = next.Data;

                        heapForest.Delete(cur);

                        cur = next;
                        next = cur.Next;
                    }

                }

            }
        }

        /// <summary>
        /// Merges the given sorted forest to current sorted Forest 
        /// & returns the last inserted node (pointer required for decrement-key)
        /// </summary>
        /// <param name="newHeapForest"></param>
        private void mergeSortedForests(DoublyLinkedList<BinomialHeapNode<T>> newHeapForest)
        {
            var @new = newHeapForest.Head;

            if (heapForest.Head == null)
            {
                heapForest = newHeapForest;
                return;
            }

            var current = heapForest.Head;

            //insert at right spot and move forward
            while (@new != null && current != null)
            {
                if (current.Data.Degree < @new.Data.Degree)
                {
                    current = current.Next;
                }
                else if (current.Data.Degree > @new.Data.Degree)
                {
                    heapForest.InsertBefore(current, new DoublyLinkedListNode<BinomialHeapNode<T>>(@new.Data));
                    @new = @new.Next;
                }
                else
                {
                    //equal
                    heapForest.InsertAfter(current, new DoublyLinkedListNode<BinomialHeapNode<T>>(@new.Data));
                    current = current.Next;
                    @new = @new.Next;
                }

            }

            //copy left overs
            while (@new != null)
            {
                heapForest.InsertAfter(heapForest.Tail, new DoublyLinkedListNode<BinomialHeapNode<T>>(@new.Data));
                @new = @new.Next;
            }

        }

        private void addMapping(T newItem, BinomialHeapNode<T> newNode)
        {
            if (heapMapping.ContainsKey(newItem))
            {
                heapMapping[newItem].Add(newNode);
            }
            else
            {
                heapMapping[newItem] = new List<BinomialHeapNode<T>>(new[] { newNode });
            }
        }

        private void updateNodeValue(T currentValue, T newValue, BinomialHeapNode<T> node)
        {
            removeMapping(currentValue, node);
            node.Value = newValue;
            addMapping(newValue, node);
        }

        private void removeMapping(T currentValue, BinomialHeapNode<T> node)
        {
            heapMapping[currentValue].Remove(node);
            if (heapMapping[currentValue].Count == 0)
            {
                heapMapping.Remove(currentValue);
            }
        }

    }
}
