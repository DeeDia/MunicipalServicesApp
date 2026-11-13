using System;

namespace MunicipalServicesApp
{
    
    /// A plain, unbalanced binary search tree. Smaller keys go left, larger keys go right.
    /// It is fast when keys arrive in random order, but if keys arrive already
    
    public class BinarySearchTree<TKey, TValue> : BinaryTreeBase<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        public override void Insert(TKey key, TValue value)
        {
            var newNode = new BinaryNode<TKey, TValue>(key, value);

            if (Root == null)
            {
                Root = newNode;
                Count = 1;
                return;
            }

            BinaryNode<TKey, TValue> current = Root;

            while (true)
            {
                int cmp = key.CompareTo(current.Key);

                if (cmp == 0)
                {
                    current.Value = value; // key exists: replace, don't duplicate
                    return;
                }

                if (cmp < 0)
                {
                    if (current.Left == null)
                    {
                        current.Left = newNode;
                        Count++;
                        return;
                    }
                    current = current.Left;
                }
                else
                {
                    if (current.Right == null)
                    {
                        current.Right = newNode;
                        Count++;
                        return;
                    }
                    current = current.Right;
                }
            }
        }
    }
}