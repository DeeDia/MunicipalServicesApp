MUNICIPAL SERVICES APPLICATION - README
=========================================

DESCRIPTION
-----------
A C# .NET Framework Windows Forms application for a South African municipality.
Citizens can report service issues (Part 1) and browse local events and
announcements with search and personalised recommendations (Part 2).
"Service Request Status" is shown but disabled; it is implemented in a later part.

REQUIREMENTS
------------
- Windows OS
- Visual Studio 2019/2022 (or later) with the ".NET desktop development" workload
- .NET Framework 4.7.2 or later

HOW TO COMPILE AND RUN
----------------------
1. Open MunicipalServicesApp.sln (or .slnx) in Visual Studio.
2. Build: Build > Rebuild Solution (Ctrl+Shift+B).
3. Run: press F5 (Debug > Start Debugging).
4. The Main Menu appears.

HOW TO USE
----------
MAIN MENU
- "Report Issues": opens the issue reporting form (see below).
- "Local Events": opens the events and announcements page.
- "Service Status": disabled in this version.

REPORT ISSUES
1. Enter the location, choose a category, and describe the issue.
2. Optionally attach an image or document (JPG, PNG, PDF, DOCX).
3. The progress bar and message show how complete your report is.
4. Click "Submit Report". A confirmation shows your reference number.
5. "Back to Main Menu" returns without submitting.

LOCAL EVENTS AND ANNOUNCEMENTS
1. The page opens showing all upcoming events and announcements in date order.
2. The orange bar under the title rotates through announcements every 4 seconds,
   most urgent first.
3. To search: choose a Category and/or a Date from the dropdowns, then click
   "Search". Click "Clear" to show everything again.
4. Click any event in a list to see its full details on the right.
5. "Recommended for you" suggests upcoming events in the categories you have
   searched or opened most. It stays empty until you search or open something,
   and never suggests events you have already opened.
6. "Recently viewed" lists the events you opened, newest first.
7. "Back to Main Menu" returns to the menu.

DATA STRUCTURES USED
--------------------
- SortedDictionary<DateTime, List<EventItem>>  Events stored in date order
  (EventRepository.EventsByDate)
- Dictionary<string, List<EventItem>>  Fast lookup of events by category
  (EventRepository.EventsByCategory); also the interest scores that drive the
  recommendations (EventService)
- SortedSet<string> and SortedSet<DateTime>  Unique categories and dates that
  fill the search dropdowns (EventRepository.Categories / EventDates)
- HashSet<int>  IDs of events the user has viewed (EventService)
- Stack<EventItem>  Recently viewed events, newest first (EventService)
- Queue<EventItem>  Rotating announcement ticker (EventService)
- SimplePriorityQueue<T>  Custom binary min-heap that orders announcements by
  urgency (SimplePriorityQueue.cs, used by EventRepository)
- List<Issue>  Reported issues (IssueRepository)

HOW RECOMMENDATIONS WORK
------------------------
Each category search adds 1 point to that category, and opening an event adds
2 points. Upcoming events the user has not yet opened are ranked by their
category's points, with the soonest event winning ties.

PROJECT STRUCTURE
-----------------
MainMenuForm.cs         Main menu
ReportIssueForm.cs      Report Issues screen
EventsForm.cs           Events and announcements screen
Issue.cs                Data model for a reported issue
IssueRepository.cs      Stores submitted issues
EventItem.cs            Data model for an event or announcement
EventRepository.cs      Event collections (sorted dictionary, dictionary, sets)
EventService.cs         Search, history, ticker and recommendation logic
SimplePriorityQueue.cs  Custom priority queue
AppTheme.cs             Shared colours and fonts

KNOWN LIMITATIONS
-----------------
- Reported issues and events are held in memory only and reset when the
  application closes. Event data is sample data created at start-up.
- "Service Status" is a placeholder for a later part.