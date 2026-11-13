using System;

namespace MunicipalServicesApp
{
    /// A left-leaning red-black tree. Nodes are red or black, and a few simple
    /// rules keep the longest path to at most twice the shortest, so the height
    /// never exceeds about 2 * log2(n). Search, height and traversals are
    /// inherited from BinaryTreeBase.
  
    public class RedBlackTree<TKey, TValue> : BinaryTreeBase<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        public int RotationCount { get; private set; }
        public int ColourFlipCount { get; private set; }

        public override void Insert(TKey key, TValue value)
        {
            bool added = false;
            Root = InsertNode(Root, key, value, ref added);
            Root.IsRed = false; // rule 2: the root is always black

            if (added)
                Count++;
        }

        private BinaryNode<TKey, TValue> InsertNode(
            BinaryNode<TKey, TValue> node, TKey key, TValue value, ref bool added)
        {
            if (node == null)
            {
                added = true;
                var fresh = new BinaryNode<TKey, TValue>(key, value);
                fresh.IsRed = true; // new nodes always start red
                return fresh;
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

            // The three repairs, in this order, on the way back up
            if (NodeIsRed(node.Right) && !NodeIsRed(node.Left))
                node = RotateLeft(node);

            if (NodeIsRed(node.Left) && NodeIsRed(node.Left.Left))
                node = RotateRight(node);

            if (NodeIsRed(node.Left) && NodeIsRed(node.Right))
                FlipColours(node);

            return node;
        }

        private static bool NodeIsRed(BinaryNode<TKey, TValue> node)
        {
            return node != null && node.IsRed; // empty spots count as black
        }

        /// Turns a right-leaning red link into a left-leaning one.

        private BinaryNode<TKey, TValue> RotateLeft(BinaryNode<TKey, TValue> h)
        {
            BinaryNode<TKey, TValue> x = h.Right;
            h.Right = x.Left;
            x.Left = h;

            x.IsRed = h.IsRed; // the new top takes the old top's colour
            h.IsRed = true;

            RotationCount++;
            return x;
        }

        /// Mirror image of RotateLeft.

        private BinaryNode<TKey, TValue> RotateRight(BinaryNode<TKey, TValue> h)
        {
            BinaryNode<TKey, TValue> x = h.Left;
            h.Left = x.Right;
            x.Right = h;

            x.IsRed = h.IsRed;
            h.IsRed = true;

            RotationCount++;
            return x;
        }

        /// When both children are red, push the redness up: children become
        /// black and the parent becomes red.

        private void FlipColours(BinaryNode<TKey, TValue> h)
        {
            h.IsRed = true;
            h.Left.IsRed = false;
            h.Right.IsRed = false;
            ColourFlipCount++;
        }

        // SELF-CHECK

        /// Verifies the red-black rules on the actual tree: black root, no red
        /// node with a red child, and the same number of black nodes on every
        /// path. Used to prove the tree is genuinely a valid red-black tree.

        public bool IsValid()
        {
            if (Root == null)
                return true;
            if (Root.IsRed)
                return false;

            int blackHeight;
            return CheckNode(Root, out blackHeight);
        }

        private bool CheckNode(BinaryNode<TKey, TValue> node, out int blackHeight)
        {
            if (node == null)
            {
                blackHeight = 1; // empty spots count as one black node
                return true;
            }

            int leftBlack;
            int rightBlack;
            bool leftOk = CheckNode(node.Left, out leftBlack);
            bool rightOk = CheckNode(node.Right, out rightBlack);

            blackHeight = leftBlack + (node.IsRed ? 0 : 1);

            if (!leftOk || !rightOk)
                return false;
            if (leftBlack != rightBlack)
                return false; // rule 4 broken
            if (node.IsRed && (NodeIsRed(node.Left) || NodeIsRed(node.Right)))
                return false; // rule 3 broken

            return true;
        }
    }
}