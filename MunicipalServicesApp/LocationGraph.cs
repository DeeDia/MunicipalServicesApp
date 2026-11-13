using System;
using System.Collections.Generic;
using System.Linq;

namespace MunicipalServicesApp
{

    /// A road between two areas. A two-way road is stored as two of these,
    /// one in each direction.

    public class RoadEdge
    {
        public string From { get; private set; }
        public string To { get; private set; }
        public double Km { get; private set; }

        public RoadEdge(string from, string to, double km)
        {
            From = from;
            To = to;
            Km = km;
        }
    }

    /// An area and how many roads away it is from the starting area.

    public class AreaHop
    {
        public string Area { get; private set; }
        public int Hops { get; private set; }

        public AreaHop(string area, int hops)
        {
            Area = area;
            Hops = hops;
        }
    }

    /// The result of a minimum spanning tree calculation.

    public class MstResult
    {
        public List<RoadEdge> Edges { get; private set; }
        public List<string> Nodes { get; private set; }
        public double TotalKm { get; set; }

        // 1 means every area is reachable from the depot. More than 1 means some
        // areas are not directly linked by road, so we get a forest of several trees.
        public int Components { get; set; }

        public MstResult()
        {
            Edges = new List<RoadEdge>();
            Nodes = new List<string>();
        }
    }


    /// A weighted, undirected graph of the municipality's areas, stored as an
    /// adjacency list: each area maps to the list of roads leaving it.
    /// (For a sparse network like a road map, this uses far less memory than
    /// an adjacency matrix and makes "what is next to this area?" instant.)

    public class LocationGraph
    {
        // The crews' base. Every plan starts here, and it is also the
        // default area for a request whose location we cannot match.
        public const string Depot = "Town Centre";

        private readonly Dictionary<string, List<RoadEdge>> adjacency =
            new Dictionary<string, List<RoadEdge>>(StringComparer.OrdinalIgnoreCase);

        public static readonly LocationGraph Default = CreateDefault();

        // BUILDING THE GRAPH

        public void AddArea(string name)
        {
            if (!adjacency.ContainsKey(name))
                adjacency[name] = new List<RoadEdge>();
        }

        /// Adds a two-way road, creating either area if it is new.

        public void AddRoad(string a, string b, double km)
        {
            AddArea(a);
            AddArea(b);
            adjacency[a].Add(new RoadEdge(a, b, km));
            adjacency[b].Add(new RoadEdge(b, a, km));
        }

        private static LocationGraph CreateDefault()
        {
            var g = new LocationGraph();

            g.AddRoad("Town Centre", "Ward 3", 2.5);
            g.AddRoad("Town Centre", "Ward 4", 3.0);
            g.AddRoad("Town Centre", "Industrial Area", 4.0);
            g.AddRoad("Town Centre", "Ward 7", 5.5);
            g.AddRoad("Ward 3", "Ward 4", 2.0);
            g.AddRoad("Ward 3", "Ward 5", 3.5);
            g.AddRoad("Ward 4", "Ward 5", 2.8);
            g.AddRoad("Ward 4", "Industrial Area", 3.2);
            g.AddRoad("Ward 5", "Riverside", 4.5);
            g.AddRoad("Industrial Area", "Ward 8", 3.8);
            g.AddRoad("Ward 7", "Ward 8", 2.2);
            g.AddRoad("Ward 7", "Ward 12", 4.0);
            g.AddRoad("Ward 8", "Ward 12", 3.0);
            g.AddRoad("Riverside", "Ward 12", 6.0);

            return g;
        }

        // BASIC INFORMATION


        public int AreaCount
        {
            get { return adjacency.Count; }
        }

        // Each road is stored twice (once per direction), so halve the total
        public int RoadCount
        {
            get { return adjacency.Values.Sum(roads => roads.Count) / 2; }
        }

        public bool HasArea(string name)
        {
            return !string.IsNullOrEmpty(name) && adjacency.ContainsKey(name);
        }

        public List<string> GetAreas()
        {
            return adjacency.Keys.OrderBy(a => a, StringComparer.Ordinal).ToList();
        }

        /// Total length of every road in the network, counting each road once.
       
        public double TotalRoadKm()
        {
            return adjacency.Values
                .SelectMany(roads => roads)
                .Where(road => string.CompareOrdinal(road.From, road.To) < 0)
                .Sum(road => road.Km);
        }

        /// Works out which area a typed location belongs to by looking for an
        /// area name inside the text. If none is found, the depot is used.

        public string FindArea(string locationText)
        {
            if (!string.IsNullOrWhiteSpace(locationText))
            {
                // Longest names first, so "Industrial Area" is tried before shorter names
                foreach (string area in adjacency.Keys.OrderByDescending(a => a.Length))
                {
                    if (locationText.IndexOf(area, StringComparison.OrdinalIgnoreCase) >= 0)
                        return area;
                }
            }
            return Depot;
        }

        // BREADTH-FIRST SEARCH

        /// Explores outward from an area in rings, using a queue. Areas come
        /// back nearest-first, each with the number of roads it is from the start.

        public List<AreaHop> BreadthFirst(string start)
        {
            var result = new List<AreaHop>();
            if (!HasArea(start))
                return result;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { start };
            var queue = new Queue<AreaHop>();
            queue.Enqueue(new AreaHop(start, 0));

            while (queue.Count > 0)
            {
                AreaHop current = queue.Dequeue();
                result.Add(current);

                // Alphabetical, so the output is always in the same order
                foreach (RoadEdge road in adjacency[current.Area].OrderBy(r => r.To, StringComparer.Ordinal))
                {
                    if (seen.Add(road.To)) // Add returns false if it was already there
                        queue.Enqueue(new AreaHop(road.To, current.Hops + 1));
                }
            }

            return result;
        }

        // DEPTH-FIRST SEARCH

        /// Follows one road as far as it goes, then backs up and tries the next
        /// (nearest road first). Works on the whole network.

        public List<string> DepthFirst(string start)
        {
            var order = new List<string>();
            if (!HasArea(start))
                return order;

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            DepthFirstWalk(adjacency, start, visited, order);
            return order;
        }

        /// The depth-first walk itself. It takes the adjacency list as a
        /// parameter so the same code can walk the full road network
        /// or just a spanning tree built from part of it.

        private static void DepthFirstWalk(
            Dictionary<string, List<RoadEdge>> graph, string node,
            HashSet<string> visited, List<string> order)
        {
            visited.Add(node);
            order.Add(node);

            foreach (RoadEdge road in graph[node].OrderBy(r => r.Km).ThenBy(r => r.To, StringComparer.Ordinal))
            {
                if (!visited.Contains(road.To))
                    DepthFirstWalk(graph, road.To, visited, order);
            }
        }

        // MINIMUM SPANNING TREE (PRIM'S ALGORITHM)

        /// Finds the least road needed to connect the depot and the given areas,
        /// using only roads that run directly between those areas. Prim's
        /// algorithm: start at the depot, then repeatedly take the shortest
        /// road that reaches a new area. The shortest-road-next choice is made
        /// by a min-heap (our SimplePriorityQueue).

        public MstResult MinimumSpanningTree(IEnumerable<string> areasNeeded)
        {
            var included = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Depot };
            foreach (string area in areasNeeded)
            {
                if (HasArea(area))
                    included.Add(area);
            }

            var result = new MstResult();
            result.Nodes.Add(Depot);
            result.Nodes.AddRange(included
                .Where(a => !a.Equals(Depot, StringComparison.OrdinalIgnoreCase))
                .OrderBy(a => a, StringComparer.Ordinal));

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Normally one pass from the depot reaches everything. If some areas are
            // not linked to the rest, we start a fresh tree from each leftover area.
            foreach (string root in result.Nodes)
            {
                if (visited.Contains(root))
                    continue;

                result.Components++;
                visited.Add(root);

                var heap = new SimplePriorityQueue<RoadEdge>(CompareRoads);
                AddCandidateRoads(root, included, visited, heap);

                while (heap.Count > 0)
                {
                    RoadEdge road = heap.Dequeue(); // the shortest road available

                    if (visited.Contains(road.To))
                        continue; // that area is already connected: skip (this avoids loops)

                    visited.Add(road.To);
                    result.Edges.Add(road);
                    result.TotalKm += road.Km;
                    AddCandidateRoads(road.To, included, visited, heap);
                }
            }

            return result;
        }

        private void AddCandidateRoads(
            string area, HashSet<string> included, HashSet<string> visited,
            SimplePriorityQueue<RoadEdge> heap)
        {
            foreach (RoadEdge road in adjacency[area])
            {
                if (included.Contains(road.To) && !visited.Contains(road.To))
                    heap.Enqueue(road);
            }
        }

        // Shortest road first; ties are broken by name so results never vary
        private static int CompareRoads(RoadEdge x, RoadEdge y)
        {
            int byLength = x.Km.CompareTo(y.Km);
            if (byLength != 0) return byLength;

            int byFrom = string.CompareOrdinal(x.From, y.From);
            if (byFrom != 0) return byFrom;

            return string.CompareOrdinal(x.To, y.To);
        }

        // VISITING ORDER: DFS OVER THE SPANNING TREE

        /// Turns a spanning tree into a visiting order: a depth-first walk from
        /// the depot along the tree's roads, nearest first. This is a sensible
        /// route that never uses a road the tree did not need, but it is not
        /// guaranteed to be the shortest possible tour.

        public List<string> RouteThroughTree(MstResult mst)
        {
            // Rebuild the tree as its own small adjacency list (roads in both directions)
            var tree = new Dictionary<string, List<RoadEdge>>(StringComparer.OrdinalIgnoreCase);
            foreach (string node in mst.Nodes)
                tree[node] = new List<RoadEdge>();

            foreach (RoadEdge road in mst.Edges)
            {
                tree[road.From].Add(road);
                tree[road.To].Add(new RoadEdge(road.To, road.From, road.Km));
            }

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var route = new List<string>();

            // The depot is first in mst.Nodes, so the route always starts there
            foreach (string node in mst.Nodes)
            {
                if (!visited.Contains(node))
                    DepthFirstWalk(tree, node, visited, route);
            }

            return route;
        }
    }
}