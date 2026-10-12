using System;
using System.Collections.Generic;
using System.Linq;

namespace MunicipalServicesApp
{
    
    public static class EventService
    {
        public const string AllCategories = "All Categories";
        public const string AnyDate = "Any Date";

        // Dictionary: category -> how interested the user seems to be in it
        private static readonly Dictionary<string, int> interestScores =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Stack: most recently viewed event is on top (last in, first out)
        private static readonly Stack<EventItem> recentlyViewed = new Stack<EventItem>();

        // Set: IDs of events the user has already opened
        private static readonly HashSet<int> viewedIds = new HashSet<int>();

        // Queue: announcements waiting their turn in the ticker (first in, first out)
        private static Queue<EventItem> tickerQueue;

        // SEARCH
        
        public static List<EventItem> Search(string category, DateTime? date)
        {
            bool hasCategory = !string.IsNullOrWhiteSpace(category) && category != AllCategories;
            IEnumerable<EventItem> results;

            if (hasCategory)
            {
                // Hash lookup by category
                List<EventItem> byCategory;
                if (!EventRepository.EventsByCategory.TryGetValue(category, out byCategory))
                    byCategory = new List<EventItem>();

                results = byCategory;
                if (date.HasValue)
                    results = results.Where(e => e.Date == date.Value.Date);
            }
            else if (date.HasValue)
            {
                // Direct lookup by date key
                List<EventItem> byDate;
                if (!EventRepository.EventsByDate.TryGetValue(date.Value.Date, out byDate))
                    byDate = new List<EventItem>();

                results = byDate;
            }
            else
            {
                results = EventRepository.GetAllEvents();
            }

            return results.OrderBy(e => e.Date).ThenBy(e => e.Priority).ToList();
        }

        // TRACKING USER BEHAVIOUR
        
        public static void RecordSearch(string category)
        {
            if (string.IsNullOrWhiteSpace(category) || category == AllCategories)
                return;

            AddInterest(category, 1);
        }

        /// Records that the user opened an event. 
        
        public static void RecordView(EventItem item)
        {
            if (item == null)
                return;

            // Don't stack the same event twice in a row
            if (recentlyViewed.Count == 0 || recentlyViewed.Peek().Id != item.Id)
                recentlyViewed.Push(item);

            viewedIds.Add(item.Id);
            AddInterest(item.Category, 2);
        }

        private static void AddInterest(string category, int points)
        {
            int current;
            interestScores.TryGetValue(category, out current);
            interestScores[category] = current + points;
        }

        // STACK: RECENTLY VIEWED
       
        public static List<EventItem> GetRecentlyViewed(int max)
        {
            return recentlyViewed.Distinct().Take(max).ToList();
        }

        // QUEUE: ANNOUNCEMENT TICKER
        
        public static EventItem GetNextTickerItem()
        {
            if (tickerQueue == null || tickerQueue.Count == 0)
            {
                tickerQueue = new Queue<EventItem>(EventRepository.GetAnnouncementsByUrgency());
            }

            if (tickerQueue.Count == 0)
                return null;

            EventItem next = tickerQueue.Dequeue();
            tickerQueue.Enqueue(next);
            return next;
        }

        // RECOMMENDATIONS
       
       public static List<EventItem> GetRecommendations(int max)
        {
            if (interestScores.Count == 0)
                return new List<EventItem>();

            DateTime today = DateTime.Today;

            return EventRepository.GetAllEvents()
                .Where(e => e.Date >= today && !viewedIds.Contains(e.Id))
                .Select(e => new { Event = e, Score = ScoreFor(e) })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Event.Date)
                .Take(max)
                .Select(x => x.Event)
                .ToList();
        }

        private static int ScoreFor(EventItem item)
        {
            int score;
            return interestScores.TryGetValue(item.Category, out score) ? score : 0;
        }

        /// The user's top categories, for showing why recommendations appear.
       public static List<string> GetTopInterests(int max)
        {
            return interestScores
                .OrderByDescending(pair => pair.Value)
                .Take(max)
                .Select(pair => pair.Key)
                .ToList();
        }
    }
}