using System;

namespace MunicipalServicesApp
{
    /// A self-balancing binary search tree. After every insert it re-checks the
    /// nodes on the path back up and rotates where one side has become more
    /// than one level taller than the other. This keeps the height close to
    /// log2(n), so search is always fast, even when keys arrive in sorted order.
    /// Search, height and traversals are inherited from BinaryTreeBase.

    public class AvlTree<TKey, TValue> : BinaryTreeBase<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        /// How many rotations the tree has performed. It shows the balancing work
        /// actually happening, and the page uses it in the structure insights.

        public int RotationCount { get; private set; }

        public override void Insert(TKey key, TValue value)
        {
            bool added = false;
            Root = InsertNode(Root, key, value, ref added);
            if (added)
                Count++;
        }

        /// Inserts recursively. Each call returns the root of its (possibly rotated)
        /// subtree, so the parent can re-attach it.

        private BinaryNode<TKey, TValue> InsertNode(
            BinaryNode<TKey, TValue> node, TKey key, TValue value, ref bool added)
        {
            if (node == null)
            {
                added = true;
                return new BinaryNode<TKey, TValue>(key, value);
            }

            int cmp = key.CompareTo(node.Key);

            if (cmp < 0)
            {
                node.Left = InsertNode(node.Left, key, value, ref added);
            }
            else if (cmp > 0)
            {
                node.Right = InsertNode(node.Right, key, value, ref added);
            }
            else
            {
                node.Value = value; // key exists: replace, don't duplicate
                return node;
            }

            return Rebalance(node);
        }

        /// Updates this node's height, then fixes it if it has become unbalanced.

        private BinaryNode<TKey, TValue> Rebalance(BinaryNode<TKey, TValue> node)
        {
            UpdateHeight(node);
            int balance = BalanceFactor(node);

            // Too tall on the left
            if (balance > 1)
            {
                if (BalanceFactor(node.Left) < 0)       // Left-Right case
                    node.Left = RotateLeft(node.Left);
                return RotateRight(node);               // Left-Left case
            }

            // Too tall on the right
            if (balance < -1)
            {
                if (BalanceFactor(node.Right) > 0)      // Right-Left case
                    node.Right = RotateRight(node.Right);
                return RotateLeft(node);                // Right-Right case
            }

            return node;
        }

        /// Lifts the left child above its parent:
        ///       y                x
        ///      / \              / \
        ///     x   C    -->     A   y
        ///    / \                  / \
        ///   A   B                B   C

        private BinaryNode<TKey, TValue> RotateRight(BinaryNode<TKey, TValue> y)
        {
            BinaryNode<TKey, TValue> x = y.Left;
            y.Left = x.Right;
            x.Right = y;

            UpdateHeight(y); // y is now lower, so update it first
            UpdateHeight(x);

            RotationCount++;
            return x;
        }

        /// Lifts the right child above its parent (the mirror image of RotateRight).
     
        private BinaryNode<TKey, TValue> RotateLeft(BinaryNode<TKey, TValue> x)
        {
            BinaryNode<TKey, TValue> y = x.Right;
            x.Right = y.Left;
            y.Left = x;

            UpdateHeight(x);
            UpdateHeight(y);

            RotationCount++;
            return y;
        }

        private static int HeightOfNode(BinaryNode<TKey, TValue> node)
        {
            return node == null ? 0 : node.Height;
        }

        private static void UpdateHeight(BinaryNode<TKey, TValue> node)
        {
            node.Height = 1 + Math.Max(HeightOfNode(node.Left), HeightOfNode(node.Right));
        }

        private static int BalanceFactor(BinaryNode<TKey, TValue> node)
        {
            return node == null ? 0 : HeightOfNode(node.Left) - HeightOfNode(node.Right);
        }
    }
}