using System;

namespace MunicipalServicesApp
{
        
    public class EventItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }

        // True = official announcement, False = community event
        public bool IsAnnouncement { get; set; }

        // 1 = most urgent. Used by the priority queue for announcements.
        public int Priority { get; set; }

        // What the user sees in list boxes
        public override string ToString()
        {
            string tag = IsAnnouncement ? "[Announcement] " : "";
            return Date.ToString("dd MMM yyyy") + "  |  " + tag + Title + "  (" + Category + ")";
        }
    }
}