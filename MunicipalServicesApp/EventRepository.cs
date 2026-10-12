using System;
using System.Collections.Generic;
using System.Linq;

namespace MunicipalServicesApp
{
    public static class EventRepository
    {
        public static SortedDictionary<DateTime, List<EventItem>> EventsByDate { get; }
            = new SortedDictionary<DateTime, List<EventItem>>();

        public static Dictionary<string, List<EventItem>> EventsByCategory { get; }
            = new Dictionary<string, List<EventItem>>(StringComparer.OrdinalIgnoreCase);

        public static SortedSet<string> Categories { get; }
            = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        public static SortedSet<DateTime> EventDates { get; }
            = new SortedSet<DateTime>();

        private static int nextId = 1;

        // Runs once, the first time the repository is used
        static EventRepository()
        {
            SeedData();
        }

        /// Adds an event and keeps every collection in sync.
        
        public static void AddEvent(EventItem item)
        {
            item.Id = nextId++;
            item.Date = item.Date.Date; //striped the time so same-day events group together

            List<EventItem> dayList;
            if (!EventsByDate.TryGetValue(item.Date, out dayList))
            {
                dayList = new List<EventItem>();
                EventsByDate[item.Date] = dayList;
            }
            dayList.Add(item);

            List<EventItem> categoryList;
            if (!EventsByCategory.TryGetValue(item.Category, out categoryList))
            {
                categoryList = new List<EventItem>();
                EventsByCategory[item.Category] = categoryList;
            }
            categoryList.Add(item);

            Categories.Add(item.Category);
            EventDates.Add(item.Date);
        }

        public static IEnumerable<EventItem> GetAllEvents()
        {
            return EventsByDate.Values.SelectMany(list => list);
        }

        /// Returns all announcements
        
        public static List<EventItem> GetAnnouncementsByUrgency()
        {
            var queue = new SimplePriorityQueue<EventItem>((a, b) =>
            {
                int byPriority = a.Priority.CompareTo(b.Priority);
                return byPriority != 0 ? byPriority : a.Date.CompareTo(b.Date);
            });

            foreach (EventItem item in GetAllEvents().Where(e => e.IsAnnouncement))
            {
                queue.Enqueue(item);
            }

            var ordered = new List<EventItem>();
            while (queue.Count > 0)
            {
                ordered.Add(queue.Dequeue());
            }
            return ordered;
        }


        private static void SeedData()
        {
            DateTime today = DateTime.Today;

            AddEvent(new EventItem { Title = "Water Supply Interruption", Category = "Utilities", Date = today.AddDays(1), Location = "Ward 12 Reservoir", IsAnnouncement = true, Priority = 1, Description = "Planned maintenance will interrupt water supply from 08:00 to 16:00. Please store water in advance." });
            AddEvent(new EventItem { Title = "Load Shedding Schedule Update", Category = "Utilities", Date = today.AddDays(2), Location = "All Wards", IsAnnouncement = true, Priority = 2, Description = "The updated schedule is available at all municipal offices and libraries." });
            AddEvent(new EventItem { Title = "Road Closure: Main Street Resurfacing", Category = "Roads", Date = today.AddDays(3), Location = "Main Street", IsAnnouncement = true, Priority = 2, Description = "Main Street will be closed between 06:00 and 18:00. Use the Church Street detour." });
            AddEvent(new EventItem { Title = "Community Clean-Up Day", Category = "Sanitation", Date = today.AddDays(5), Location = "Central Park", Description = "Volunteers welcome. Gloves and bags are provided. Meet at the main gate at 09:00." });
            AddEvent(new EventItem { Title = "Neighbourhood Watch Meeting", Category = "Public Safety", Date = today.AddDays(6), Location = "Community Hall", Description = "Monthly meeting to discuss safety in the area. Everyone is welcome." });
            AddEvent(new EventItem { Title = "Heritage Day Celebration", Category = "Culture", Date = today.AddDays(8), Location = "Town Square", Description = "Food stalls, live music, and traditional dance performances for the whole family." });
            AddEvent(new EventItem { Title = "Free Health Screening", Category = "Health", Date = today.AddDays(9), Location = "Municipal Clinic", Description = "Free blood pressure, glucose and cholesterol checks. First come, first served." });
            AddEvent(new EventItem { Title = "Waste Collection Day Change", Category = "Sanitation", Date = today.AddDays(10), Location = "Wards 3, 4 and 5", IsAnnouncement = true, Priority = 3, Description = "Collection moves from Monday to Tuesday for the next two weeks." });
            AddEvent(new EventItem { Title = "Youth Soccer Tournament", Category = "Sport", Date = today.AddDays(12), Location = "Municipal Stadium", Description = "Teams from across the district compete. Entry is free." });
            AddEvent(new EventItem { Title = "Pothole Repair Programme", Category = "Roads", Date = today.AddDays(14), Location = "Ward 7 and Ward 8", IsAnnouncement = true, Priority = 3, Description = "Crews will repair reported potholes. Please report any others through the app." });
            AddEvent(new EventItem { Title = "Farmers and Crafts Market", Category = "Community", Date = today.AddDays(15), Location = "Market Square", Description = "Local produce, crafts, and food from small businesses." });
            AddEvent(new EventItem { Title = "Library Reading Programme", Category = "Culture", Date = today.AddDays(18), Location = "Central Library", Description = "A weekly reading programme for children aged 6 to 12." });
        }
    }
}