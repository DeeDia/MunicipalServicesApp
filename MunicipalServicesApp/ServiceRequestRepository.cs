using System;
using System.Collections.Generic;
using System.Linq;

namespace MunicipalServicesApp
{

    /// A request found near another one, and how many roads away it is.

    public class NearbyRequest
    {
        public ServiceRequest Request { get; private set; }
        public int Hops { get; private set; }

        public NearbyRequest(ServiceRequest request, int hops)
        {
            Request = request;
            Hops = hops;
        }
    }

    /// The single place that owns every service request. It keeps three trees
    /// in step with each other and exposes the heap, hierarchy and graph
    /// features, so the form only ever talks to this class.

    public static class ServiceRequestRepository
    {
        // Primary index: guaranteed-fast lookup by unique ID
        public static AvlTree<int, ServiceRequest> ById { get; }
            = new AvlTree<int, ServiceRequest>();

        // Baseline: same keys in a plain BST, kept so the app can show why balancing matters
        public static BinarySearchTree<int, ServiceRequest> BaselineById { get; }
            = new BinarySearchTree<int, ServiceRequest>();

        // Chronological index: requests in the order they were submitted
        public static RedBlackTree<DateKey, ServiceRequest> ByDate { get; }
            = new RedBlackTree<DateKey, ServiceRequest>();

        private static int nextId = 1001;

        static ServiceRequestRepository()
        {
            SeedData();
        }

        public static int Count
        {
            get { return ById.Count; }
        }

        // ADDING REQUESTS

        /// Gives the request its unique ID (if it has none), records its first
        /// status, and inserts it into all three trees.

        public static ServiceRequest Add(ServiceRequest request)
        {
            if (request.Id == 0)
                request.Id = nextId++;

            if (request.History.Count == 0)
                request.SetStatus(RequestStatus.Submitted, request.DateSubmitted);

            ById.Insert(request.Id, request);
            BaselineById.Insert(request.Id, request);
            ByDate.Insert(new DateKey(request.DateSubmitted, request.Id), request);

            return request;
        }

        /// Turns an issue submitted on the Report Issues form into a trackable
        /// service request, and returns it so the form can show its reference.
        /// The area comes from matching the typed location against the graph's
        /// areas; the priority comes from the category.

        public static ServiceRequest AddFromIssue(Issue issue)
        {
            string area = LocationGraph.Default.FindArea(issue.Location);

            var request = new ServiceRequest
            {
                Title = MakeTitle(issue.Category, area),
                Category = issue.Category,
                Area = area,
                Location = issue.Location,
                Description = issue.Description,
                Priority = PriorityFor(issue.Category),
                DateSubmitted = issue.DateReported
            };
            request.AttachmentPaths.AddRange(issue.AttachmentPaths);

            return Add(request);
        }

        private static string MakeTitle(string category, string area)
        {
            return category + " issue - " + area;
        }

        private static int PriorityFor(string category)
        {
            switch (category)
            {
                case "Public Safety":
                    return 1;

                case "Utilities (Water/Electricity)":
                case "Roads":
                case "Sanitation":
                    return 2;

                default:
                    return 3;
            }
        }

        // FINDING A REQUEST BY ITS UNIQUE IDENTIFIER

        /// Accepts "SR-1013", "sr-1013" or just "1013". Searches the AVL tree
        /// (the real lookup) and the plain BST (for comparison) and reports how
        /// many steps each needed.

        public static bool TryFindByReference(
            string text, out ServiceRequest request, out int avlSteps, out int bstSteps)
        {
            request = null;
            avlSteps = 0;
            bstSteps = 0;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            string digits = text.Trim();
            if (digits.StartsWith("SR-", StringComparison.OrdinalIgnoreCase))
                digits = digits.Substring(3);

            int id;
            if (!int.TryParse(digits.Trim(), out id))
                return false;

            ServiceRequest baselineResult;
            BaselineById.TryGet(id, out baselineResult, out bstSteps);

            return ById.TryGet(id, out request, out avlSteps);
        }

        // LISTS IN DIFFERENT ORDERS: EACH ONE USES A DIFFERENT STRUCTURE

        //By ID: in-order walk of the AVL tree.
        public static List<ServiceRequest> GetAll()
        {
            return ById.InOrder();
        }

        //By date submitted: in-order walk of the red-black tree.
        public static List<ServiceRequest> GetByDate()
        {
            return ByDate.InOrder();
        }

        //By urgency: extracted from the min-heap.
        public static List<ServiceRequest> GetByUrgency()
        {
            return UrgencyHeap.SortByUrgency(GetAll());
        }

        /// Requests submitted between two dates (inclusive), oldest first.
        /// The red-black tree skips whole branches that fall outside the range.

        public static List<ServiceRequest> GetBetween(DateTime from, DateTime to)
        {
            return ByDate.Range(
                new DateKey(from.Date, 0),
                new DateKey(to.Date.AddDays(1).AddTicks(-1), int.MaxValue));
        }

        public static List<ServiceRequest> MostUrgentOpen(int count)
        {
            return UrgencyHeap.MostUrgentOpen(GetAll(), count);
        }

        /// <summary>The Municipality -> Department -> Category -> Request tree.</summary>
        public static HierarchyNode BuildHierarchy()
        {
            return HierarchyBuilder.Build(GetAll());
        }

        // GRAPH FEATURES

        public static List<string> GetOpenAreas()
        {
            return GetAll()
                .Where(r => r.IsOpen)
                .Select(r => r.Area)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// The least road needed to connect the depot and every area that
        /// currently has an open request (a minimum spanning tree).

        public static MstResult PlanCrewRoute()
        {
            return LocationGraph.Default.MinimumSpanningTree(GetOpenAreas());
        }

        /// Open requests in the same area as 'origin' or in areas within
        /// 'maxHops' roads of it, nearest first (breadth-first search).

        public static List<NearbyRequest> FindNearbyOpen(ServiceRequest origin, int maxHops)
        {
            ILookup<string, ServiceRequest> openByArea = GetAll()
                .Where(r => r.IsOpen && r.Id != origin.Id)
                .ToLookup(r => r.Area, StringComparer.OrdinalIgnoreCase);

            var result = new List<NearbyRequest>();

            foreach (AreaHop hop in LocationGraph.Default.BreadthFirst(origin.Area))
            {
                if (hop.Hops > maxHops)
                    break; // BFS returns nearest first, so everything after is further

                foreach (ServiceRequest request in openByArea[hop.Area].OrderBy(r => r.Priority).ThenBy(r => r.Id))
                    result.Add(new NearbyRequest(request, hop.Hops));
            }

            return result;
        }

        // SAMPLE DATA

        private static void Seed(
            string category, string area, string location, string description,
            int priority, int daysAgo, RequestStatus finalStatus)
        {
            // Different hours of the day, so requests from the same day still differ in time
            DateTime submitted = DateTime.Today.AddDays(-daysAgo).AddHours(8 + (nextId % 9));

            var request = new ServiceRequest
            {
                Title = MakeTitle(category, area),
                Category = category,
                Area = area,
                Location = location,
                Description = description,
                Priority = priority,
                DateSubmitted = submitted
            };
            Add(request);

            // Walk it through each stage up to its final one
            for (int stage = 1; stage <= (int)finalStatus; stage++)
                request.SetStatus((RequestStatus)stage, submitted.AddHours(stage * 12));
        }

        private static void SeedData()
        {
            // Dates are deliberately not in ID order, so the red-black tree has real sorting to do
            Seed("Roads", "Ward 7", "Corner of Main Street, Ward 7", "Large pothole near the intersection is damaging vehicles.", 2, 12, RequestStatus.Resolved);
            Seed("Utilities (Water/Electricity)", "Ward 12", "Reservoir Road, Ward 12", "Burst water pipe is flooding the street.", 1, 10, RequestStatus.InProgress);
            Seed("Sanitation", "Ward 3", "Ward 3 informal settlement", "Blocked sewer line is overflowing near homes.", 1, 9, RequestStatus.UnderReview);
            Seed("Waste Management", "Ward 4", "Ward 4 shopping centre", "Refuse has not been collected for two weeks.", 3, 14, RequestStatus.Resolved);
            Seed("Public Safety", "Riverside", "Riverside footbridge", "Streetlights along the footbridge are not working.", 1, 6, RequestStatus.InProgress);
            Seed("Roads", "Industrial Area", "Industrial Area Road, near gate 3", "Road markings have faded and the traffic light is faulty.", 2, 4, RequestStatus.Submitted);
            Seed("Utilities (Water/Electricity)", "Ward 5", "Mandela Street, Ward 5", "Power outage is affecting several houses.", 1, 3, RequestStatus.UnderReview);
            Seed("Sanitation", "Town Centre", "Town Centre taxi rank", "Public toilets are out of order.", 2, 8, RequestStatus.InProgress);
            Seed("Waste Management", "Ward 8", "Ward 8 park", "Illegal dumping next to the park entrance.", 3, 5, RequestStatus.Submitted);
            Seed("Roads", "Ward 4", "School Road, Ward 4", "A blocked stormwater drain floods the road when it rains.", 2, 11, RequestStatus.UnderReview);
            Seed("Public Safety", "Ward 3", "Ward 3 community hall", "A broken fence leaves the hall grounds open at night.", 2, 7, RequestStatus.Submitted);
            Seed("Other", "Ward 12", "Ward 12 library", "The library opening-hours sign is missing.", 3, 6, RequestStatus.Resolved);
            Seed("Utilities (Water/Electricity)", "Ward 7", "Hill Street, Ward 7", "Water pressure has been very low for a week.", 2, 13, RequestStatus.InProgress);
        }
    }
}