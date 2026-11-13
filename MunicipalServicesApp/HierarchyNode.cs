using System;
using System.Collections.Generic;

namespace MunicipalServicesApp
{

    /// One node in a general tree (a tree where each node can have any number
    /// of children). Departments and categories are "folder" nodes; individual
    /// service requests are the leaves.

    public class HierarchyNode
    {
        public string Label { get; private set; }

        // Null for department/category nodes; set only on leaf nodes
        public ServiceRequest Request { get; private set; }

        public HierarchyNode Parent { get; private set; }
        public List<HierarchyNode> Children { get; private set; }

        public HierarchyNode(string label, ServiceRequest request)
        {
            Label = label;
            Request = request;
            Children = new List<HierarchyNode>();
        }

        public bool IsLeaf
        {
            get { return Children.Count == 0; }
        }

        /// How many levels below the root this node sits (the root is 0).

        public int Depth
        {
            get
            {
                int depth = 0;
                HierarchyNode ancestor = Parent;
                while (ancestor != null)
                {
                    depth++;
                    ancestor = ancestor.Parent;
                }
                return depth;
            }
        }

        public HierarchyNode AddChild(string label, ServiceRequest request)
        {
            var child = new HierarchyNode(label, request);
            child.Parent = this;
            Children.Add(child);
            return child;
        }

        /// Returns the child with this label, creating it first if it doesn't exist.
        /// This is how departments and categories get created the first time
        /// a request needs them.
 
        public HierarchyNode GetOrAddChild(string label)
        {
            foreach (HierarchyNode child in Children)
            {
                if (child.Label == label)
                    return child;
            }
            return AddChild(label, null);
        }

 
        /// Counts the requests in this node's whole branch. Because each node
        /// asks its children, a department can total up everything beneath it.

        public int CountRequests(bool openOnly)
        {
            if (Request != null)
                return (!openOnly || Request.IsOpen) ? 1 : 0;

            int total = 0;
            foreach (HierarchyNode child in Children)
                total += child.CountRequests(openOnly);
            return total;
        }

        /// Text shown in the TreeView: folders show how many requests are open.

        public string DisplayText
        {
            get
            {
                if (Request != null)
                    return Label;

                return Label + " (" + CountRequests(true) + " open of " + CountRequests(false) + ")";
            }
        }

        /// Number of levels in the branch below (and including) this node.

        public int Height()
        {
            int tallest = 0;
            foreach (HierarchyNode child in Children)
                tallest = Math.Max(tallest, child.Height());
            return 1 + tallest;
        }


        /// Depth-first walk: this node first, then each child's branch in turn.

        public IEnumerable<HierarchyNode> PreOrder()
        {
            yield return this;

            foreach (HierarchyNode child in Children)
            {
                foreach (HierarchyNode descendant in child.PreOrder())
                    yield return descendant;
            }
        }

        /// Level-by-level walk using a queue: the root, then all departments,
        /// then all categories, then all requests.

        public List<HierarchyNode> LevelOrder()
        {
            var result = new List<HierarchyNode>();
            var queue = new Queue<HierarchyNode>();
            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                HierarchyNode node = queue.Dequeue();
                result.Add(node);
                foreach (HierarchyNode child in node.Children)
                    queue.Enqueue(child);
            }
            return result;
        }

        /// Sorts every level alphabetically so the tree always displays in a stable order.

        public void SortChildren()
        {
            Children.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase));
            foreach (HierarchyNode child in Children)
                child.SortChildren();
        }
    }

    /// Builds the Municipality -> Department -> Category -> Request tree.

    public static class HierarchyBuilder
    {
        public static HierarchyNode Build(IEnumerable<ServiceRequest> requests)
        {
            var root = new HierarchyNode("Municipality", null);

            foreach (ServiceRequest request in requests)
            {
                HierarchyNode department = root.GetOrAddChild(DepartmentMap.DepartmentFor(request.Category));
                HierarchyNode category = department.GetOrAddChild(request.Category);
                category.AddChild(request.Reference + "  |  " + request.StatusText, request);
            }

            root.SortChildren();
            return root;
        }
    }
}