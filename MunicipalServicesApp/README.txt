MUNICIPAL SERVICES APPLICATION - README
=======================================

1. OVERVIEW
-----------
A C# .NET Framework Windows Forms application for a South African
municipality. Residents can:

  - Report Issues: log a problem with a location, category, description and
    an optional attachment, and receive a tracking reference (e.g. SR-1014).
  - Local Events: browse upcoming events and announcements, search by
    category and date, and get recommendations based on their searches.
  - Service Status: track any request by its unique reference, follow its
    progress through four stages, and see how requests relate to each other
    (by department, by urgency, by location).

The interface uses one colour scheme throughout (#BF5700, #E09540, #ED9121)
and lays itself out with table and flow panels, so it stretches with the
window. Forms fade in, and progress bars slide to their new value.


2. REQUIREMENTS
---------------
  - Windows
  - Visual Studio 2019 / 2022 / later with the ".NET desktop development"
    workload installed
  - .NET Framework 4.7.2 or later (no other packages or internet needed)


3. HOW TO COMPILE AND RUN
-------------------------
Using Visual Studio:
  1. Open MunicipalServicesApp.sln (or .slnx) in Visual Studio.
  2. Choose Build > Rebuild Solution (it should report "1 succeeded").
  3. Press F5 (Debug > Start Debugging) to run.

Using the command line (optional): open a "Developer Command Prompt for
Visual Studio", go to the solution folder and run:
  msbuild MunicipalServicesApp.sln /p:Configuration=Debug
The program is then at MunicipalServicesApp\bin\Debug\MunicipalServicesApp.exe

All data is created in memory at start-up. Nothing needs to be installed,
configured or connected.

UPDATES BASED ON FEEDBACK
=========================
Municipal Services Application - PoE Part 3

1. FEEDBACK RECEIVED
--------------------
Part 1: Result [your exact mark] (80% or above).
        Written comments: [paste any comments or rubric bands you receive,
        or write "No written comments were provided with the mark."]
Part 2: Result [your exact mark] (80% or above).
        Written comments: [same as above]

[If your lecturer replies, list each comment here, then give each one its own
entry in section 2 saying what you changed. Use the source "Lecturer feedback"
only for changes that really came from a comment.]


2. UPDATES MADE
---------------
Where an update did not come from a lecturer comment, its source says so.
These changes came from testing the application and reviewing my own work.

U1. Report Issues is now connected to Service Status
    Source:  Self-review while integrating Part 3
    Before:  Submitting a report showed "Issue #n", which was only a count
             and could not be used to find the report again.
    Change:  Each submission now creates a service request with a unique
             reference (for example SR-1014). The confirmation message shows
             it and tells the user where to track it.
    Files:   ReportIssueForm.cs, ServiceRequestRepository.cs

U2. Buttons wired in code instead of only in the designer
    Source:  Testing (Part 1)
    Before:  The Report Issues button once stopped responding because its
             Click event had lost its link to the handler in the designer
             file. Nothing warned me that it was dead.
    Change:  The Local Events and Service Status buttons are connected in
             code (btn.Click += handler), and the Events and Service Status
             screens are built entirely in code, so no handler can be lost.
    Files:   MainMenuForm.cs, EventsForm.cs, ServiceStatusForm.cs

U3. One colour scheme, including progress bars
    Source:  Self-review against the "Consistency" design requirement
    Before:  The progress bars used the default Windows green, which did not
             match the #BF5700 / #E09540 / #ED9121 scheme.
    Change:  A ThemedProgressBar (still a standard ProgressBar) draws itself
             in the application's colours.
    Files:   ThemedProgressBar.cs, ServiceStatusForm.cs
             [delete "ReportIssueForm.Designer.cs" from this list if you did
             not switch the Report Issues bar over]

U4. Filter bar wrapping fixed
    Source:  Testing (Service Status screen)
    Before:  When the window was narrow, the "Period:" label wrapped away
             from its dropdown.
    Change:  Each label and dropdown is grouped as a pair, so they wrap
             together.
    Files:   ServiceStatusForm.cs

U5. Disabled button now looks disabled
    Source:  Testing (Service Status screen)
    Before:  "Advance Status (demo)" looked the same whether or not it could
             be used.
    Change:  It turns grey, with a "no entry" cursor, when no request is
             selected or the selected request is already resolved.
    Files:   ServiceStatusForm.cs

U6. Rebuild loops prevented when lists refresh
    Source:  Testing (Events and Service Status screens)
    Before:  Refilling a list in code fires its selection event as if the
             user had clicked, which could trigger another refresh.
    Change:  A flag (isRefreshing) makes the handlers ignore changes made by
             the program itself, and the tree view opens a request only after
             its own event has finished.
    Files:   EventsForm.cs, ServiceStatusForm.cs

U7. README rewritten and expanded
    Source:  Self-review against the Part 3 readme and explanation criteria
    Before:  The README covered compiling, running and using Part 1 and
             Part 2 only.
    Change:  It now covers all three parts and the full application, and
             explains each data structure's role, why it was chosen, an
             example from the app and its efficiency. It also lists the
             limitations I found while testing.
    Files:   README.txt

U8. Data structure claims backed by measurements
    Source:  Self-review
    Change:  Test runs compared the plain BST and the AVL tree on the same
             1,000 request IDs (BST height 1000 and AVL height 10), and
             checked the red-black tree against its own rules
             (IsValid() returned True). The Structures tab now shows live
             heights and step counts, so the efficiency claims can be seen
             in the running application.
    Files:   ServiceRequestRepository.cs, ServiceStatusForm.cs


4. HOW TO USE THE APPLICATION
-----------------------------
MAIN MENU
  Three buttons: Report Issues, Local Events, Service Status. Each opens its
  own window; closing it (or "Back to Main Menu") returns here.

REPORT ISSUES
  1. Type the location (include an area name such as "Ward 7" or "Riverside"
     so the request is placed in the right area).
  2. Choose a category: Sanitation, Roads, Utilities (Water/Electricity),
     Waste Management, Public Safety or Other.
  3. Describe the issue in the description box.
  4. Optional: click "Attach Image/Document" and choose a JPG, PNG, PDF or
     DOCX file. The file name appears beside the button.
  5. The progress bar and message above Submit show how complete the report
     is (25% for each of location, category, description and attachment).
  6. Click Submit. If anything required is missing, a warning explains what.
     On success a message shows the tracking reference, e.g. SR-1014.
  7. "Back" returns to the main menu without submitting.

  Priority is set from the category: Public Safety = High; Roads, Sanitation
  and Utilities = Medium; everything else = Routine.

LOCAL EVENTS
  1. The page opens with every event and announcement in date order.
  2. The bar under the title rotates through announcements every 4 seconds,
     most urgent first.
  3. Choose a Category and/or a Date and click Search. Click Clear to reset.
  4. Click any event to see its full details on the right.
  5. "Recommended for you" suggests upcoming events in the categories you
     searched or opened most (it stays empty until you do either, and never
     suggests an event you already opened).
  6. "Recently viewed" lists the events you opened, newest first.

SERVICE STATUS
  Tracking:
    Type a reference ("SR-1013", "sr-1013" or just "1013") and click Track
    (or press Enter). The request is selected and a message shows how many
    steps the AVL tree and the plain BST each needed to find it.
  The list:
    - Sort by: ID, Date submitted, or Urgency.
    - Status filter and Period filter (all time / last 7 / last 14 days).
    - Open urgent requests are bold; resolved requests are grey.
  Right-hand tabs:
    - Progress: details of the selected request, a progress bar
      (Submitted 0%, Under Review 33%, In Progress 66%, Resolved 100%), the
      four stages, and the status history with times.
      "Advance Status (demo)" moves the request to its next stage. It is a
      demonstration control; in a live system only municipal staff would
      change a status.
    - Departments: all requests as Municipality > Department > Category >
      Request, with open counts. Click a request to open it.
    - Crew Plan: the five most urgent open requests; open requests near the
      selected one; and the shortest road network and visiting order for a
      work crew. Double-click a request in either list to open it.
    - Structures: live statistics for every data structure (heights,
      rotations, step counts).

  Requests submitted through Report Issues appear here automatically with
  status "Submitted". (The 13 requests present at start-up are sample data.)

QUICK TOUR (about two minutes)
  1. Report Issues: submit a report; note its reference.
  2. Service Status: type that reference and click Track.
  3. Open the Structures tab and compare the AVL and BST step counts.
  4. Click "Advance Status (demo)" and watch the bar slide.
  5. Sort by Urgency, then open the Crew Plan tab.
  6. Local Events: search "Roads", open an event, and watch
     "Recommended for you" and "Recently viewed" update.


5. DATA STRUCTURES: ROLE, EFFICIENCY AND EXAMPLES
-------------------------------------------------
n = number of items; h = height of a tree. A tree's height is the number
of nodes on its longest path from the top, and a search never takes more
steps than the height.

5.1 SERVICE REQUEST STATUS
--------------------------

BINARY TREE (BinaryTreeBase.cs)
  What: a tree in which every node has at most two children (left, right).
  Role: the shared foundation of the BST, AVL and red-black trees. It holds
  the node type and the code that every search tree needs: search, height,
  range query and the four traversals (in-order, pre-order, post-order,
  level-order). Writing it once means the three trees below only supply
  their own insert logic.
  Example: in-order traversal of the AVL tree returns requests sorted by ID,
  which is how "Sort by ID" works without ever sorting a list. Level-order
  uses a queue and reads the tree row by row.
  Efficiency: each traversal visits every node once, O(n). A range query
  skips whole branches outside the range, so it costs O(h + k) for k results.

BINARY SEARCH TREE (BinarySearchTree.cs)
  What: a binary tree where smaller keys go left and larger keys go right.
  Role: a deliberately plain, unbalanced baseline holding the same request
  IDs as the AVL tree, so the app can show why balancing matters.
  Example: request IDs arrive in increasing order, so every new node goes to
  the right and the tree becomes a chain. With the 13 sample requests its
  height is 13; in a test with 1,000 requests it was 1,000, and finding the
  last one took 1,000 steps.
  Efficiency: search and insert take O(h): fast for random keys, but O(n) in
  this worst case.

AVL TREE (AvlTree.cs)
  What: a BST that rebalances itself. After each insert, any node whose left
  and right sides differ in height by more than 1 is fixed by a rotation
  (single or double).
  Role: the main lookup for "Track by reference" and the source of the
  "sort by ID" list.
  Example: same 13 requests, same insertion order: AVL height 4 (the best
  possible for 13 items) against the BST's 13. Tracking SR-1013 takes 4
  steps instead of 13. In the 1,000-request test the AVL tree stayed at
  height 10 and found request 1000 in 10 steps, using 990 rotations to stay
  balanced. The Structures tab shows these figures live.
  Efficiency: height is always about log2(n), so search and insert are
  O(log n) in the worst case.

RED-BLACK TREE (RedBlackTree.cs, DateKey.cs)
  What: a BST whose nodes are red or black. Rules (black root, no red node
  with a red child, equal black count on every path) guarantee that the
  longest path is at most twice the shortest. This is the left-leaning
  variant, which needs only rotations and colour flips.
  Role: keeps requests in the order they were submitted. It powers "Sort by
  Date submitted" and the Period filter. The key is a date plus the request
  ID (DateKey), so two requests at the same moment never overwrite each
  other.
  Example: the sample requests are deliberately not in date order, yet an
  in-order walk returns them oldest first. "Last 7 days" asks the tree for a
  date range and skips every branch outside it. IsValid() checks the red-black
  rules on the real tree and reports True. With 1,000 requests the tree had
  height 10 (limit about 20).
  Efficiency: insert and search are O(log n) (height at most about
  2 x log2(n)). Red-black trees usually rebalance less than AVL trees on
  updates, while AVL trees are slightly shallower for lookups. Using both
  shows that trade-off in practice.

GENERAL (BASIC) TREE (HierarchyNode.cs)
  What: a tree where a node may have any number of children.
  Role: the Departments tab. Requests are grouped Municipality > Department
  > Category > Request using DepartmentMap (for example Roads and Utilities
  belong to Infrastructure). Each folder node counts the open requests
  beneath it by asking its children.
  Example: "Infrastructure (4 open of 6)" is calculated by adding up the
  counts of its categories, with no separate counters to keep in step.
  Efficiency: building it is O(n); counting a branch visits only that
  branch's nodes.

HEAP / PRIORITY QUEUE (SimplePriorityQueue.cs, UrgencyHeap.cs)
  What: a binary min-heap stored in a List. The most urgent item is always at
  position 0; adding or removing reshuffles only one path of the tree.
  .NET Framework has no built-in priority queue, so this one is hand-written
  and generic.
  Role: ranks requests by urgency (open before resolved, then priority, then
  longest waiting, then ID). It feeds "Sort by Urgency" and the five most
  urgent open requests. The same heap also orders announcements in Local
  Events and chooses the shortest road in Prim's algorithm.
  Example: with the sample data the top five open requests are SR-1002,
  SR-1003, SR-1005, SR-1007 and SR-1013. SR-1013 is Medium priority but ranks
  fifth because it has waited longest of the Medium requests.
  Efficiency: insert and remove-most-urgent are O(log n). Taking the top
  five costs far less than sorting everything.

GRAPH (LocationGraph.cs)
  What: nodes joined by weighted edges. Here the nodes are nine areas (Town
  Centre, Wards 3, 4, 5, 7, 8 and 12, Industrial Area, Riverside), the edges
  are 14 roads, and the weights are distances in km (50.0 km in total). It is
  stored as an adjacency list: each area maps to the roads leaving it.
  Role: describes how areas connect, so requests can be related by place. It
  also turns a typed location into an area: the text is searched for an area
  name, and the Town Centre depot is used if none matches.
  Example: "Corner of Main St, ward 7" is placed in Ward 7; "Durban" is
  placed at the depot.
  Efficiency: an adjacency list suits a sparse road map (9 areas, 14
  roads): it uses O(V + E) memory, and "which roads leave this area?" is
  instant.

GRAPH TRAVERSAL: BREADTH-FIRST AND DEPTH-FIRST SEARCH
  Breadth-first search (a queue) explores in rings: all areas one road away,
  then two, and so on.
  Role: "Open requests near the selected one". Example from Ward 7:
  Ward 7 (0 roads), then Town Centre, Ward 12, Ward 8 (1 road), then
  Industrial Area, Ward 3, Ward 4, Riverside (2 roads), then Ward 5 (3 roads).
  The list shows open requests in those areas, nearest first.
  Depth-first search (recursion) follows one road as far as it goes, then
  backs up. Role: it walks the spanning tree to produce the crew's visiting
  order.
  Efficiency: both visit each area and road once, O(V + E). Each area is
  remembered once visited, so loops in the road map can never trap the search.

MINIMUM SPANNING TREE (Prim's algorithm, in LocationGraph.cs)
  What: the cheapest set of roads that still connects a group of areas, with
  no loops. Prim's algorithm starts at the depot and repeatedly adds the
  shortest road that reaches a new area, using the heap to pick it. A road
  to an already-connected area is skipped, which is what removes loops.
  Role: the Crew Plan. It connects the depot and every area that has an
  open request, and a depth-first walk of the result gives the visiting
  order.
  Example: with all nine areas needing a visit, the crew needs 8 roads
  totalling 24.0 km instead of all 14 roads (50.0 km), which is 52% less
  road. For Ward 3, Ward 4 and Ward 5 alone, the tree keeps 7.3 km of the
  13.8 km available between them.
  Efficiency: O(E log V) with a heap.

5.2 LOCAL EVENTS
----------------
SORTED DICTIONARY (EventRepository.EventsByDate)
  Maps each date to the events on that date, always in date order.
  Efficient because nothing is ever re-sorted: O(log n) to add or look up.
  Example: the events page lists all 12 events in date order straight from
  this structure.

DICTIONARY / HASH TABLE (EventRepository.EventsByCategory)
  Maps each category to its events. A hash lookup does not scan every event,
  so it is O(1) on average. Example: searching "Roads" finds the two Roads
  events directly. EventService also uses a Dictionary of interest points
  per category for the recommendations.

SORTED SETS (EventRepository.Categories and EventDates)
  A set holds each value once and, sorted, keeps it in order. They fill the
  search dropdowns, so each of the 8 categories and each date appears exactly
  once with no duplicate checking. Add, remove and contains are O(log n).

HASH SET (EventService)
  Remembers the IDs of events the user has opened. "Have I seen this?" is
  O(1), and it is how recommendations avoid suggesting an event already seen.

STACK (EventService, "Recently viewed")
  Last in, first out: the newest viewed event is on top, which is the order
  a history list needs. Push and read are O(1).

QUEUE (EventService, announcement ticker)
  First in, first out. The front announcement is shown and put back at the
  end, so all announcements take turns forever. O(1) per step. The queue is
  filled in urgency order by the priority queue.

PRIORITY QUEUE (SimplePriorityQueue, announcements)
  Orders announcements by priority (1 = most urgent), then by date. Example:
  the Water Supply Interruption (priority 1) is always shown first.

RECOMMENDATION ALGORITHM (EventService)
  Each category search adds 1 point to that category, and opening an event
  adds 2 points (opening shows stronger interest than searching). Upcoming
  events the user has not opened are scored by their category's points and
  listed highest first; the soonest event wins a tie. Nothing is recommended
  until the user has searched or opened something.
  Example: search "Roads" twice and open one Sanitation event. Roads has 2
  points and Sanitation has 2 points, so the suggestions show events from
  both, with the soonest first.

5.3 REPORT ISSUES
-----------------
LIST (IssueRepository.Issues): a List<Issue> storing every submitted issue
in order, adding at the end in O(1). Each submission also creates a service
request, which is inserted into the AVL tree, the BST and the red-black tree
together (ServiceRequestRepository.Add), so the three can never disagree.


6. PROJECT STRUCTURE
--------------------
Screens
  MainMenuForm.cs             Main menu
  ReportIssueForm.cs          Report Issues screen
  EventsForm.cs               Local Events screen
  ServiceStatusForm.cs        Service Status screen (four tabs)
  ThemedProgressBar.cs        Progress bar drawn in the colour scheme
  AppTheme.cs                 Shared colours and fonts

Data models and stores
  Issue.cs, IssueRepository.cs
  EventItem.cs, EventRepository.cs, EventService.cs
  ServiceRequest.cs           Request, its status stages and DepartmentMap
  ServiceRequestRepository.cs Owns the three trees; heap, hierarchy, graph
                              features

Data structures
  BinaryTreeBase.cs           Binary tree foundation and traversals
  BinarySearchTree.cs         Plain BST
  AvlTree.cs                  AVL tree
  RedBlackTree.cs, DateKey.cs Red-black tree and its date key
  HierarchyNode.cs            General tree and its builder
  SimplePriorityQueue.cs      Min-heap
  UrgencyHeap.cs              Request urgency ranking
  LocationGraph.cs            Graph, BFS, DFS and Prim's algorithm


7. LIMITATIONS
--------------
  - Data is held in memory only: issues, requests and history reset when the
    program closes. Events and the first 13 requests are sample data, with
    dates calculated from today so they are always recent or upcoming.
  - Attachments are stored as file paths, not copied. If the file is moved or
    deleted, the path no longer works.
  - A location is matched to an area only if its text contains an area name
    (such as "Ward 7"); otherwise the request is placed at the Town Centre.
  - The spanning tree uses only roads that run directly between areas that
    have open requests. Resolving a request can therefore make the plan
    longer: if Ward 8's only open request is resolved, the total rises from
    24.0 km to 24.5 km, because Ward 8 was a stepping stone between its
    neighbours.
  - The crew's visiting order (depth-first walk of the spanning tree) is a
    sensible route but not guaranteed to be the shortest possible tour.
  - The trees support insert, search, traversal and range queries; requests
    are never deleted, so deletion is not implemented.
  - "Advance Status (demo)" lets any user move a request forward. A real
    system would restrict this to municipal staff.
  - Distances display with your Windows regional decimal separator (for
    example 24,0 km on some systems).