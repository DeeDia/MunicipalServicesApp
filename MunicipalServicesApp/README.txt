MUNICIPAL SERVICES APPLICATION - README
=========================================

DESCRIPTION
-----------
A C# .NET Framework Windows Forms application that allows citizens to report
municipal service issues (e.g. sanitation, roads, utilities). This is Part_01
of the Municipal Services Application PoE, only the "Report Issues" feature
is implemented; "Local Events & Announcements" and "Service Request Status"
are shown but disabled, to be implemented in later parts.

REQUIREMENTS TO RUN
--------------------
- Windows OS
- Visual Studio 2019/2022 (or later) with the ".NET desktop development"
  workload installed
- .NET Framework 4.7.2 or later

HOW TO COMPILE AND RUN
------------------------
1. Open MunicipalServicesApp.sln in Visual Studio.
2. Build the solution: Build > Build Solution (or Ctrl+Shift+B).
3. Run the application: press F5, or Debug > Start Debugging.
4. The Main Menu window will appear.

To use the application, you need to follow these steps:
-----------------------------
1. On the Main Menu select "Report Issues" (the other two options are)
   The term disabled is used here for the following app versions (see disabled in these versions of the app).
   On the Report an Issue form:
 Type in the Location of the problem in the box.
   - Click on the Category drop down (Sanitation, Roads, Utilities,
     (Other public safety, Waste Management).
   - Enter a Description of the issue in the description box.
   You can optionally click on “Attach Image/Document” to attach a supporting image or document.
     A file (JPG, PNG, PDF or DOCX).
2 As you use the program, watch the progress bar and message above the buttons change.
     Fill in each box - this indicates the fullness of your report.
3. Click the "Submit Report" button to save the issue.
   If required fields (Location, Category, Description) are not filled in,
     A warning message will inform you what you need to do.
   A confirmation message with a reference number is displayed in case of success.
4. You can click “Back to Main Menu” anytime to go back without submitting.

PROJECT STRUCTURE
--------------------
- MainMenuForm.cs        - Main menu with navigation to the three features
- ReportIssueForm.cs     - Report Issues screen and its logic
- Issue.cs               - Data model representing one reported issue
- IssueRepository.cs     - Stores all submitted issues (List<Issue>)
- AppTheme.cs            - Shared colour palette and fonts used app-wide

KNOWN LIMITATIONS (Part 1 scope)
-----------------------------------
- Reported issues are stored in memory only and are cleared when the
  application closes (no file/database persistence yet).
- "Local Events & Announcements" and "Service Request Status" are
  placeholders, disabled on the Main Menu, to be implemented in later
  parts of the PoE.