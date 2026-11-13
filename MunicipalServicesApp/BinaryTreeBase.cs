using System;
using System.Collections.Generic;

namespace MunicipalServicesApp
{
    public class BinaryNode<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public BinaryNode<TKey, TValue> Left { get; set; }
        public BinaryNode<TKey, TValue> Right { get; set; }

        public int Height { get; set; }   // AVL: height of the subtree rooted here
        public bool IsRed { get; set; }   // Red-black: colour of this node

        public BinaryNode(TKey key, TValue value)
        {
            Key = key;
            Value = value;
            Height = 1;
        }
    }

    /// The foundation shared by every tree in this project. It provides
    /// searching, height, counting and the four traversals. Each subclass only
    /// has to say how it inserts (and how it keeps itself balanced, if at all).
    /// The key must be comparable so the tree knows which way to branch.
    public abstract class BinaryTreeBase<TKey, TValue> where TKey : IComparable<TKey>
    {
        public BinaryNode<TKey, TValue> Root { get; protected set; }
        public int Count { get; protected set; }

        /// Each kind of tree inserts differently, so subclasses supply this.
        /// If the key already exists, its value is replaced.

        public abstract void Insert(TKey key, TValue value);

        // SEARCH
        
        /// Looks for a key by walking down from the root, going left or right
        /// at each node. 'comparisons' reports how many nodes we had to look at,
        /// which lets the app compare how efficient each tree is.

        public bool TryGet(TKey key, out TValue value, out int comparisons)
        {
            comparisons = 0;
            BinaryNode<TKey, TValue> current = Root;

            while (current != null)
            {
                comparisons++;
                int cmp = key.CompareTo(current.Key);

                if (cmp == 0)
                {
                    value = current.Value;
                    return true;
                }

                current = cmp < 0 ? current.Left : current.Right;
            }

            value = default(TValue);
            return false;
        }

        // SHAPE

        /// The number of levels in the tree, worked out from its actual shape
        /// (so it is comparable across BST, AVL and red-black trees).

        public int Height()
        {
            return HeightOf(Root);
        }

        private static int HeightOf(BinaryNode<TKey, TValue> node)
        {
            return node == null ? 0 : 1 + Math.Max(HeightOf(node.Left), HeightOf(node.Right));
        }

        // TRAVERSALS
        ///Left, then node, then right: gives values in sorted key order.
        public List<TValue> InOrder()
        {
            var result = new List<TValue>();
            WalkInOrder(Root, result);
            return result;
        }

        private void WalkInOrder(BinaryNode<TKey, TValue> node, List<TValue> result)
        {
            if (node == null) return;
            WalkInOrder(node.Left, result);
            result.Add(node.Value);
            WalkInOrder(node.Right, result);
        }

        ///Node, then left, then right: shows the tree's structure from the top down.
        public List<TValue> PreOrder()
        {
            var result = new List<TValue>();
            WalkPreOrder(Root, result);
            return result;
        }

        private void WalkPreOrder(BinaryNode<TKey, TValue> node, List<TValue> result)
        {
            if (node == null) return;
            result.Add(node.Value);
            WalkPreOrder(node.Left, result);
            WalkPreOrder(node.Right, result);
        }

        ///Left, then right, then node: children always come before their parent.
        public List<TValue> PostOrder()
        {
            var result = new List<TValue>();
            WalkPostOrder(Root, result);
            return result;
        }

        private void WalkPostOrder(BinaryNode<TKey, TValue> node, List<TValue> result)
        {
            if (node == null) return;
            WalkPostOrder(node.Left, result);
            WalkPostOrder(node.Right, result);
            result.Add(node.Value);
        }

        ///Level by level, top to bottom, using a queue.
        public List<TValue> LevelOrder()
        {
            var result = new List<TValue>();
            if (Root == null) return result;

            var queue = new Queue<BinaryNode<TKey, TValue>>();
            queue.Enqueue(Root);

            while (queue.Count > 0)
            {
                BinaryNode<TKey, TValue> node = queue.Dequeue();
                result.Add(node.Value);

                if (node.Left != null) queue.Enqueue(node.Left);
                if (node.Right != null) queue.Enqueue(node.Right);
            }

            return result;
        }

        // RANGE QUERY
        /// All values whose key lies between low and high (inclusive), in order.
        /// Branches that cannot contain a match are skipped entirely, so this
        /// is much faster than checking every item.
        public List<TValue> Range(TKey low, TKey high)
        {
            var result = new List<TValue>();
            WalkRange(Root, low, high, result);
            return result;
        }

        private void WalkRange(BinaryNode<TKey, TValue> node, TKey low, TKey high, List<TValue> result)
        {
            if (node == null) return;

            // Only go left if there could be smaller keys still inside the range
            if (low.CompareTo(node.Key) < 0)
                WalkRange(node.Left, low, high, result);

            if (low.CompareTo(node.Key) <= 0 && high.CompareTo(node.Key) >= 0)
                result.Add(node.Value);

            // Only go right if there could be larger keys still inside the range
            if (high.CompareTo(node.Key) > 0)
                WalkRange(node.Right, low, high, result);
        }
    }
}