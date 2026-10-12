using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace MunicipalServicesApp
{
    public partial class EventsForm : Form
    {
        private const string DateFormat = "dd MMM yyyy";
        private const string ResultsHeading = "Upcoming events and announcements";

        // Controls to reach after construction
        private Label lblTicker;
        private Label lblResults;
        private Label lblRecommendedHeader;
        private ComboBox cmbCategory;
        private ComboBox cmbDate;
        private ListBox lstResults;
        private ListBox lstRecommended;
        private ListBox lstRecent;
        private RichTextBox rtbDetails;

        private Timer tickerTimer;
        private Timer fadeInTimer;

        // True while we are refilling a list in code, so the list's
        // "selection changed" event doesn't treat it as a user click
        private bool isRefreshing;

        private readonly Font titleFont = new Font("Segoe UI", 13F, FontStyle.Bold);

        public EventsForm()
        {
            InitializeComponent();
            BuildUI();
            LoadFilters();
            ShowResults(EventService.Search(null, null)); // start by showing everything
            RefreshSidePanels();
            StartTicker();
            SetupFadeInAnimation();
        }

        // BUILDING THE INTERFACE
        private void BuildUI()
        {
            this.Text = "Local Events and Announcements";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(1000, 680);
            this.MinimumSize = new Size(900, 640); // stops the layout being crushed
            this.BackColor = AppTheme.Background;

            // One column, four rows: header / ticker / main area / footer.
            // The main area row takes whatever space is left, so it grows with the window.
            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 4;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));

            root.Controls.Add(BuildHeader(), 0, 0);
            root.Controls.Add(BuildTicker(), 0, 1);
            root.Controls.Add(BuildMainArea(), 0, 2);
            root.Controls.Add(BuildFooter(), 0, 3);

            this.Controls.Add(root);
        }

        private Control BuildHeader()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.Margin = Padding.Empty;
            panel.BackColor = AppTheme.Primary;

            Label title = new Label();
            title.Text = "Local Events and Announcements";
            title.Dock = DockStyle.Fill;
            title.Font = AppTheme.HeadingFont;
            title.ForeColor = AppTheme.TextLight;
            title.TextAlign = ContentAlignment.MiddleLeft;
            title.Padding = new Padding(20, 0, 0, 0);

            panel.Controls.Add(title);
            return panel;
        }

        private Control BuildTicker()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.Margin = Padding.Empty;
            panel.BackColor = AppTheme.Secondary;

            lblTicker = new Label();
            lblTicker.Dock = DockStyle.Fill;
            lblTicker.Font = AppTheme.ButtonFont;
            lblTicker.ForeColor = AppTheme.TextDark; // dark text stays readable on the orange
            lblTicker.TextAlign = ContentAlignment.MiddleLeft;
            lblTicker.Padding = new Padding(20, 0, 0, 0);
            lblTicker.AutoEllipsis = true;
            lblTicker.UseMnemonic = false;

            panel.Controls.Add(lblTicker);
            return panel;
        }

        private Control BuildMainArea()
        {
            // Two columns: results on the left (58%), details and suggestions on the right (42%)
            TableLayoutPanel main = new TableLayoutPanel();
            main.Dock = DockStyle.Fill;
            main.Margin = Padding.Empty;
            main.Padding = new Padding(15);
            main.ColumnCount = 2;
            main.RowCount = 1;
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            main.Controls.Add(BuildLeftColumn(), 0, 0);
            main.Controls.Add(BuildRightColumn(), 1, 0);
            return main;
        }

        private Control BuildLeftColumn()
        {
            TableLayoutPanel left = NewColumnLayout(3, new Padding(0, 0, 10, 0));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));  // search bar
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));  // "results" heading
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // results list

            left.Controls.Add(BuildSearchBar(), 0, 0);

            lblResults = NewSectionLabel(ResultsHeading);
            left.Controls.Add(lblResults, 0, 1);

            lstResults = NewListBox();
            left.Controls.Add(lstResults, 0, 2);
            return left;
        }

        private Control BuildSearchBar()
        {
            // A FlowLayoutPanel wraps its contents onto a second line when the window is narrow
            FlowLayoutPanel bar = new FlowLayoutPanel();
            bar.Dock = DockStyle.Fill;
            bar.Margin = Padding.Empty;
            bar.WrapContents = true;

            bar.Controls.Add(NewFieldLabel("Category:"));
            cmbCategory = NewComboBox(190);
            bar.Controls.Add(cmbCategory);

            bar.Controls.Add(NewFieldLabel("Date:"));
            cmbDate = NewComboBox(140);
            bar.Controls.Add(cmbDate);

            Button search = NewButton("Search", AppTheme.Primary, AppTheme.TextLight, 100);
            search.Click += BtnSearch_Click;
            bar.Controls.Add(search);

            Button clear = NewButton("Clear", AppTheme.Secondary, AppTheme.TextDark, 90);
            clear.Click += BtnClear_Click;
            bar.Controls.Add(clear);

            return bar;
        }

        private Control BuildRightColumn()
        {
            TableLayoutPanel right = NewColumnLayout(6, new Padding(10, 0, 0, 0));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 29F));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 29F));

            right.Controls.Add(NewSectionLabel("Event details"), 0, 0);

            rtbDetails = new RichTextBox();
            rtbDetails.Dock = DockStyle.Fill;
            rtbDetails.ReadOnly = true;
            rtbDetails.BorderStyle = BorderStyle.None;
            rtbDetails.BackColor = Color.FromArgb(253, 246, 238); // a soft tint of the orange palette
            rtbDetails.Font = AppTheme.BodyFont;
            rtbDetails.Margin = new Padding(0, 0, 0, 5);
            rtbDetails.Text = "Select an event from the list to see its details here.";
            right.Controls.Add(rtbDetails, 0, 1);

            lblRecommendedHeader = NewSectionLabel("Recommended for you");
            right.Controls.Add(lblRecommendedHeader, 0, 2);
            lstRecommended = NewListBox();
            right.Controls.Add(lstRecommended, 0, 3);

            right.Controls.Add(NewSectionLabel("Recently viewed"), 0, 4);
            lstRecent = NewListBox();
            right.Controls.Add(lstRecent, 0, 5);

            return right;
        }

        private Control BuildFooter()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.Margin = Padding.Empty;
            panel.Padding = new Padding(15, 10, 15, 10);

            Button back = NewButton("Back to Main Menu", AppTheme.Disabled, AppTheme.TextDark, 220);
            back.Dock = DockStyle.Left;
            back.Margin = Padding.Empty;
            back.Click += (s, e) => this.Close();

            panel.Controls.Add(back);
            return panel;
        }

        // SMALL HELPERS FOR CREATING CONSISTENTLY STYLED CONTROLS
       
        private TableLayoutPanel NewColumnLayout(int rows, Padding margin)
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Margin = margin;
            layout.ColumnCount = 1;
            layout.RowCount = rows;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            return layout;
        }

        private Label NewSectionLabel(string text)
        {
            Label label = new Label();
            label.Text = text;
            label.Dock = DockStyle.Fill;
            label.Margin = Padding.Empty;
            label.Font = AppTheme.ButtonFont;
            label.ForeColor = AppTheme.Primary;
            label.TextAlign = ContentAlignment.BottomLeft;
            label.AutoEllipsis = true;
            label.UseMnemonic = false;
            return label;
        }

        private Label NewFieldLabel(string text)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = true;
            label.Font = AppTheme.BodyFont;
            label.ForeColor = AppTheme.TextDark;
            label.Margin = new Padding(0, 9, 6, 0);
            return label;
        }

        private ComboBox NewComboBox(int width)
        {
            ComboBox combo = new ComboBox();
            combo.Width = width;
            combo.DropDownStyle = ComboBoxStyle.DropDownList; // pick from the list, no typing
            combo.Font = AppTheme.BodyFont;
            combo.Margin = new Padding(0, 4, 15, 0);
            return combo;
        }

        private Button NewButton(string text, Color back, Color fore, int width)
        {
            Button button = new Button();
            button.Text = text;
            button.Width = width;
            button.Height = 32;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = AppTheme.Accent;
            button.Font = AppTheme.ButtonFont;
            button.BackColor = back;
            button.ForeColor = fore;
            button.Cursor = Cursors.Hand;
            button.Margin = new Padding(0, 2, 8, 0);
            button.UseMnemonic = false;
            return button;
        }

        private ListBox NewListBox()
        {
            ListBox list = new ListBox();
            list.Dock = DockStyle.Fill;
            list.Margin = new Padding(0, 0, 0, 5);
            list.Font = AppTheme.BodyFont;
            list.ForeColor = AppTheme.TextDark;
            list.BorderStyle = BorderStyle.FixedSingle;
            list.IntegralHeight = false;
            list.HorizontalScrollbar = true;
            list.SelectedIndexChanged += EventList_SelectedIndexChanged;
            return list;
        }

        // FILLING THE CONTROLS FROM OUR DATA STRUCTURES
        // The dropdowns are filled from the two SortedSets, so every category and date appears exactly once, already in order.
        private void LoadFilters()
        {
            cmbCategory.Items.Add(EventService.AllCategories);
            foreach (string category in EventRepository.Categories)
                cmbCategory.Items.Add(category);
            cmbCategory.SelectedIndex = 0;

            cmbDate.Items.Add(EventService.AnyDate);
            foreach (DateTime date in EventRepository.EventDates)
                cmbDate.Items.Add(date.ToString(DateFormat, CultureInfo.InvariantCulture));
            cmbDate.SelectedIndex = 0;
        }

        private void ShowResults(List<EventItem> results)
        {
            isRefreshing = true;
            lstResults.BeginUpdate();
            lstResults.Items.Clear();
            foreach (EventItem item in results)
                lstResults.Items.Add(item);
            lstResults.EndUpdate();
            isRefreshing = false;

            lblResults.Text = ResultsHeading + " (" + results.Count + " found)";

            if (results.Count == 0)
            {
                rtbDetails.Text = "No events match your search. Try a different category or date, " +
                                  "or click Clear to see everything.";
            }
        }

        private void RefreshSidePanels()
        {
            isRefreshing = true;
            FillList(lstRecommended, EventService.GetRecommendations(5),
                     "Search or open events to get suggestions.");
            FillList(lstRecent, EventService.GetRecentlyViewed(5),
                     "Events you open will appear here.");
            isRefreshing = false;

            List<string> top = EventService.GetTopInterests(2);
            lblRecommendedHeader.Text = top.Count == 0
                ? "Recommended for you"
                 : "Recommended (based on: " + string.Join(", ", top) + ")";
        }

        private void FillList(ListBox list, List<EventItem> items, string emptyMessage)
        {
            list.BeginUpdate();
            list.Items.Clear();

            if (items.Count == 0)
            {
                list.Items.Add(emptyMessage); // a plain string, not an EventItem
            }
            else
            {
                foreach (EventItem item in items)
                    list.Items.Add(item);
            }
            list.EndUpdate();
        }

        // EVENT HANDLERS
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string category = cmbCategory.SelectedItem.ToString();

            DateTime? date = null;
            string chosenDate = cmbDate.SelectedItem.ToString();
            if (chosenDate != EventService.AnyDate)
                date = DateTime.ParseExact(chosenDate, DateFormat, CultureInfo.InvariantCulture);

            EventService.RecordSearch(category);   // feeds the recommendation engine
            ShowResults(EventService.Search(category, date));
            RefreshSidePanels();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            cmbCategory.SelectedIndex = 0;
            cmbDate.SelectedIndex = 0;
            ShowResults(EventService.Search(null, null));
        }

        /// One handler shared by all three lists. 'sender' tells us which list was clicked.
        private void EventList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isRefreshing)
                return;

            ListBox list = (ListBox)sender;
            EventItem item = list.SelectedItem as EventItem; 
            if (item == null)
                return;

            ShowDetails(item);

            // Re-opening something from "Recently viewed" shouldn't boost its score again
            if (list != lstRecent)
            {
                EventService.RecordView(item);
                RefreshSidePanels();
            }
        }

        private void ShowDetails(EventItem item)
        {
            rtbDetails.Clear();

            AppendLine(item.Title, titleFont, AppTheme.Primary);
            AppendLine(item.IsAnnouncement
                           ? "Announcement (priority " + item.Priority + ")"
                           : "Community event",
                       AppTheme.ButtonFont, AppTheme.Secondary);
            AppendLine("", AppTheme.BodyFont, AppTheme.TextDark);
            AppendLine("Date: " + item.Date.ToString("dddd, dd MMMM yyyy"), AppTheme.BodyFont, AppTheme.TextDark);
            AppendLine("Location: " + item.Location, AppTheme.BodyFont, AppTheme.TextDark);
            AppendLine("Category: " + item.Category, AppTheme.BodyFont, AppTheme.TextDark);
            AppendLine("", AppTheme.BodyFont, AppTheme.TextDark);
            AppendLine(item.Description, AppTheme.BodyFont, AppTheme.TextDark);
        }

        private void AppendLine(string text, Font font, Color color)
        {
            rtbDetails.SelectionStart = rtbDetails.TextLength;
            rtbDetails.SelectionLength = 0;
            rtbDetails.SelectionFont = font;
            rtbDetails.SelectionColor = color;
            rtbDetails.AppendText(text + Environment.NewLine);
        }

        // ANIMATION AND TIMERS
        /// Rotates through the announcements every 4 seconds using the queue.
        private void StartTicker()
        {
            ShowNextTickerItem();

            tickerTimer = new Timer();
            tickerTimer.Interval = 4000;
            tickerTimer.Tick += (s, e) => ShowNextTickerItem();
            tickerTimer.Start();
        }

        private void ShowNextTickerItem()
        {
            EventItem next = EventService.GetNextTickerItem();
            lblTicker.Text = next == null
                ? "No announcements at the moment."
                : "Announcement:  " + next.Title + "  -  " + next.Date.ToString("dd MMM") + "  -  " + next.Location;
        }

        private void SetupFadeInAnimation()
        {
            this.Opacity = 0;
            fadeInTimer = new Timer();
            fadeInTimer.Interval = 15;
            fadeInTimer.Tick += (s, e) =>
            {
                if (this.Opacity < 1.0)
                {
                    this.Opacity += 0.08;
                }
                else
                {
                    this.Opacity = 1.0;
                    fadeInTimer.Stop();
                }
            };
            fadeInTimer.Start();
        }

        /// Stops the timers when the form closes so they don't keep running in the background.
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (tickerTimer != null)
            {
                tickerTimer.Stop();
                tickerTimer.Dispose();
            }
            if (fadeInTimer != null)
            {
                fadeInTimer.Stop();
                fadeInTimer.Dispose();
            }
            base.OnFormClosed(e);
        }
    }
}