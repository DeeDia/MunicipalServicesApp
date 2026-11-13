using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MunicipalServicesApp
{
    public partial class ServiceStatusForm : Form
    {
        // Choices for the three dropdowns
        private const string SortById = "ID (AVL tree)";
        private const string SortByDate = "Date submitted (red-black tree)";
        private const string SortByUrgency = "Urgency (heap)";

        private const string AllStatuses = "All statuses";
        private const string AllTime = "All time";
        private const string Last7Days = "Last 7 days";
        private const string Last14Days = "Last 14 days";

        // Controls we need to reach after construction
        private TextBox txtTrack;
        private Label lblTrackResult;
        private Label lblListHeading;
        private ComboBox cmbSort;
        private ComboBox cmbStatus;
        private ComboBox cmbPeriod;
        private ListView lvRequests;
        private RichTextBox rtbDetails;
        private ProgressBar progressStatus;
        private Label[] stageLabels;
        private ListBox lstHistory;
        private Button btnAdvance;
        private TreeView tvHierarchy;
        private ListBox lstUrgent;
        private ListBox lstNearby;
        private Label lblNearbyHeading;
        private RichTextBox rtbCrewPlan;
        private RichTextBox rtbStructures;

        // Step counts from the most recent "Track", shown on the Structures tab
        private int lastAvlSteps;
        private int lastBstSteps;

        private Timer fadeInTimer;
        private Timer progressTimer;

        private readonly Font boldFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        private readonly Font titleFont = new Font("Segoe UI", 13F, FontStyle.Bold);

        // True while we change controls in code, so their events don't
        // treat our changes as if the user had made them
        private bool isRefreshing;

        // The request currently shown in the details panel
        private ServiceRequest selected;

        public ServiceStatusForm()
        {
            InitializeComponent();
            BuildUI();
            LoadFilters();
            RefreshList();
            ShowSelected();
            SetupFadeInAnimation();
        }

        // BUILDING THE INTERFACE

        private void BuildUI()
        {
            this.Text = "Service Request Status";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(1100, 700);
            this.MinimumSize = new Size(980, 660);
            this.BackColor = AppTheme.Background;

            // One column, four rows: header / tracker / main area / footer.
            // The main area takes the leftover space, so it grows with the window.
            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 4;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 74F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));

            root.Controls.Add(BuildHeader(), 0, 0);
            root.Controls.Add(BuildTrackerBar(), 0, 1);
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
            title.Text = "Service Request Status";
            title.Dock = DockStyle.Fill;
            title.Font = AppTheme.HeadingFont;
            title.ForeColor = AppTheme.TextLight;
            title.TextAlign = ContentAlignment.MiddleLeft;
            title.Padding = new Padding(20, 0, 0, 0);

            panel.Controls.Add(title);
            return panel;
        }

        private Control BuildTrackerBar()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.Margin = Padding.Empty;
            panel.Padding = new Padding(15, 6, 15, 0);
            panel.BackColor = Color.FromArgb(253, 246, 238); // a soft tint of the orange palette

            TableLayoutPanel layout = NewColumnLayout(2, Padding.Empty);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel row = new FlowLayoutPanel();
            row.Dock = DockStyle.Fill;
            row.Margin = Padding.Empty;
            row.WrapContents = false;

            row.Controls.Add(NewFieldLabel("Track a request by its reference:"));

            txtTrack = new TextBox();
            txtTrack.Width = 170;
            txtTrack.Font = AppTheme.BodyFont;
            txtTrack.Margin = new Padding(0, 5, 10, 0);
            txtTrack.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true; // stops the "ding" sound
                    Track();
                }
            };
            row.Controls.Add(txtTrack);

            Button track = NewButton("Track", AppTheme.Primary, AppTheme.TextLight, 90);
            track.Click += (s, e) => Track();
            row.Controls.Add(track);

            lblTrackResult = new Label();
            lblTrackResult.Dock = DockStyle.Fill;
            lblTrackResult.Margin = Padding.Empty;
            lblTrackResult.Font = AppTheme.BodyFont;
            lblTrackResult.ForeColor = AppTheme.TextDark;
            lblTrackResult.TextAlign = ContentAlignment.MiddleLeft;
            lblTrackResult.AutoEllipsis = true;
            lblTrackResult.UseMnemonic = false;
            lblTrackResult.Text = "Enter a reference such as SR-1013 and click Track.";

            layout.Controls.Add(row, 0, 0);
            layout.Controls.Add(lblTrackResult, 0, 1);
            panel.Controls.Add(layout);
            return panel;
        }

        private Control BuildMainArea()
        {
            // Two columns: the request list (55%) and the tabs (45%)
            TableLayoutPanel main = new TableLayoutPanel();
            main.Dock = DockStyle.Fill;
            main.Margin = Padding.Empty;
            main.Padding = new Padding(15);
            main.ColumnCount = 2;
            main.RowCount = 1;
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            main.Controls.Add(BuildLeftColumn(), 0, 0);
            main.Controls.Add(BuildRightColumn(), 1, 0);
            return main;
        }

        private Control BuildLeftColumn()
        {
            TableLayoutPanel left = NewColumnLayout(3, new Padding(0, 0, 10, 0));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));  // filters
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));  // heading
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // list

            left.Controls.Add(BuildFilterBar(), 0, 0);

            lblListHeading = NewSectionLabel("Service requests");
            left.Controls.Add(lblListHeading, 0, 1);

            lvRequests = new ListView();
            lvRequests.Dock = DockStyle.Fill;
            lvRequests.Margin = new Padding(0, 0, 0, 5);
            lvRequests.View = View.Details;
            lvRequests.FullRowSelect = true;
            lvRequests.HideSelection = false; // keep the highlight when the list loses focus
            lvRequests.MultiSelect = false;
            lvRequests.Font = AppTheme.BodyFont;
            lvRequests.ForeColor = AppTheme.TextDark;
            lvRequests.BorderStyle = BorderStyle.FixedSingle;
            lvRequests.Columns.Add("Reference", 72);
            lvRequests.Columns.Add("Status", 88);
            lvRequests.Columns.Add("Category", 150);
            lvRequests.Columns.Add("Area", 95);
            lvRequests.Columns.Add("Priority", 62);
            lvRequests.Columns.Add("Submitted", 88);
            lvRequests.SelectedIndexChanged += List_SelectedIndexChanged;
            left.Controls.Add(lvRequests, 0, 2);

            return left;
        }

        private Control BuildFilterBar()
        {
            // Each caption and dropdown is grouped as a pair, so wrapping keeps them together
            FlowLayoutPanel bar = new FlowLayoutPanel();
            bar.Dock = DockStyle.Fill;
            bar.Margin = Padding.Empty;
            bar.WrapContents = true;

            cmbSort = NewComboBox(215);
            cmbSort.SelectedIndexChanged += Filter_Changed;

            cmbStatus = NewComboBox(125);
            cmbStatus.SelectedIndexChanged += Filter_Changed;

            cmbPeriod = NewComboBox(115);
            cmbPeriod.SelectedIndexChanged += Filter_Changed;

            bar.Controls.Add(NewFilterPair("Sort by:", cmbSort));
            bar.Controls.Add(NewFilterPair("Status:", cmbStatus));
            bar.Controls.Add(NewFilterPair("Period:", cmbPeriod));

            return bar;
        }

        private Control BuildRightColumn()
        {
            TabControl tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            tabs.Margin = new Padding(10, 0, 0, 0);
            tabs.Font = AppTheme.BodyFont;

            tabs.TabPages.Add(BuildProgressTab());
            BuildExtraTabs(tabs); // Departments, Crew Plan and Structures

            return tabs;
        }

        private TabPage BuildProgressTab()
        {
            TabPage page = NewTabPage("Progress");

            TableLayoutPanel layout = NewColumnLayout(5, Padding.Empty);
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));   // details
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));  // progress bar and stages
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));  // "history" heading
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));   // history list
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));  // advance button

            rtbDetails = NewReadOnlyBox();
            layout.Controls.Add(rtbDetails, 0, 0);

            layout.Controls.Add(BuildProgressArea(), 0, 1);
            layout.Controls.Add(NewSectionLabel("Status history"), 0, 2);

            lstHistory = NewListBox();
            layout.Controls.Add(lstHistory, 0, 3);

            layout.Controls.Add(BuildAdvanceRow(), 0, 4);

            page.Controls.Add(layout);
            return page;
        }

        private Control BuildProgressArea()
        {
            TableLayoutPanel layout = NewColumnLayout(2, Padding.Empty);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            progressStatus = new ProgressBar();
            progressStatus.Dock = DockStyle.Fill;
            progressStatus.Margin = new Padding(0, 4, 0, 4);
            progressStatus.Minimum = 0;
            progressStatus.Maximum = 100;
            progressStatus.Style = ProgressBarStyle.Continuous;
            layout.Controls.Add(progressStatus, 0, 0);

            // Four labels, one per stage, under the bar
            TableLayoutPanel stages = new TableLayoutPanel();
            stages.Dock = DockStyle.Fill;
            stages.Margin = Padding.Empty;
            stages.ColumnCount = 4;
            stages.RowCount = 1;
            for (int i = 0; i < 4; i++)
                stages.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            stages.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            string[] names = { "Submitted", "Under Review", "In Progress", "Resolved" };
            stageLabels = new Label[4];
            for (int i = 0; i < 4; i++)
            {
                Label label = new Label();
                label.Text = names[i];
                label.Dock = DockStyle.Fill;
                label.Margin = Padding.Empty;
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.UseMnemonic = false;
                label.Font = AppTheme.BodyFont;
                label.ForeColor = Color.FromArgb(150, 150, 150);
                stageLabels[i] = label;
                stages.Controls.Add(label, i, 0);
            }
            layout.Controls.Add(stages, 0, 1);

            return layout;
        }

        private Control BuildAdvanceRow()
        {
            TableLayoutPanel row = new TableLayoutPanel();
            row.Dock = DockStyle.Fill;
            row.Margin = Padding.Empty;
            row.ColumnCount = 2;
            row.RowCount = 1;
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            row.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            btnAdvance = NewButton("Advance Status (demo)", AppTheme.Secondary, AppTheme.TextDark, 190);
            btnAdvance.Dock = DockStyle.Fill;
            btnAdvance.Margin = new Padding(0, 6, 0, 4);
            btnAdvance.Enabled = false;
            btnAdvance.Click += BtnAdvance_Click;

            Label note = new Label();
            note.Text = "Demo control. In a live system only municipal staff would update a request's status.";
            note.Dock = DockStyle.Fill;
            note.Margin = Padding.Empty;
            note.Padding = new Padding(10, 0, 0, 0);
            note.Font = AppTheme.BodyFont;
            note.ForeColor = Color.FromArgb(120, 120, 120);
            note.TextAlign = ContentAlignment.MiddleLeft;
            note.UseMnemonic = false;

            row.Controls.Add(btnAdvance, 0, 0);
            row.Controls.Add(note, 1, 0);
            return row;
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

        private TabPage NewTabPage(string title)
        {
            TabPage page = new TabPage(title);
            page.BackColor = AppTheme.Background;
            page.UseVisualStyleBackColor = false;
            page.Padding = new Padding(8);
            return page;
        }

        private RichTextBox NewReadOnlyBox()
        {
            RichTextBox box = new RichTextBox();
            box.Dock = DockStyle.Fill;
            box.ReadOnly = true;
            box.BorderStyle = BorderStyle.None;
            box.BackColor = Color.FromArgb(253, 246, 238);
            box.Font = AppTheme.BodyFont;
            box.Margin = new Padding(0, 0, 0, 5);
            return box;
        }

        private Control NewFilterPair(string caption, ComboBox combo)
        {
            FlowLayoutPanel pair = new FlowLayoutPanel();
            pair.AutoSize = true;
            pair.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pair.WrapContents = false;
            pair.Margin = Padding.Empty;
            pair.Controls.Add(NewFieldLabel(caption));
            pair.Controls.Add(combo);
            return pair;
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
            return list;
        }

        private void WriteLine(RichTextBox box, string text, Font font, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionFont = font;
            box.SelectionColor = color;
            box.AppendText(text + Environment.NewLine);
        }

        private void SetAdvanceEnabled(bool enabled)
        {
            btnAdvance.Enabled = enabled;
            btnAdvance.BackColor = enabled ? AppTheme.Secondary : AppTheme.Disabled;
            btnAdvance.ForeColor = enabled ? AppTheme.TextDark : Color.FromArgb(120, 120, 120);
            btnAdvance.Cursor = enabled ? Cursors.Hand : Cursors.No;
        }

        // FILLING THE LIST FROM THE TREES AND THE HEAP

        private void LoadFilters()
        {
            isRefreshing = true;

            cmbSort.Items.AddRange(new object[] { SortById, SortByDate, SortByUrgency });
            cmbSort.SelectedIndex = 0;

            cmbStatus.Items.AddRange(new object[] { AllStatuses, "Submitted", "Under Review", "In Progress", "Resolved" });
            cmbStatus.SelectedIndex = 0;

            cmbPeriod.Items.AddRange(new object[] { AllTime, Last7Days, Last14Days });
            cmbPeriod.SelectedIndex = 0;

            isRefreshing = false;
        }

        /// Each sort order comes from a different structure, so the list is
        /// never re-sorted in memory.

        private List<ServiceRequest> GetSortedSource()
        {
            string sort = cmbSort.SelectedItem.ToString();

            if (sort == SortByDate)
                return ServiceRequestRepository.GetByDate();     // red-black tree, in-order

            if (sort == SortByUrgency)
                return ServiceRequestRepository.GetByUrgency();  // min-heap

            return ServiceRequestRepository.GetAll();            // AVL tree, in-order
        }

        private void RefreshList()
        {
            List<ServiceRequest> source = GetSortedSource();

            string status = cmbStatus.SelectedItem.ToString();
            if (status != AllStatuses)
                source = source.Where(r => r.StatusText == status).ToList();

            // The period filter asks the red-black tree for a date range, which
            // skips whole branches outside it, then keeps only those requests.
            string period = cmbPeriod.SelectedItem.ToString();
            if (period != AllTime)
            {
                int days = period == Last7Days ? 7 : 14;
                var inRange = new HashSet<int>(
                    ServiceRequestRepository
                        .GetBetween(DateTime.Today.AddDays(-days), DateTime.Today)
                        .Select(r => r.Id));
                source = source.Where(r => inRange.Contains(r.Id)).ToList();
            }

            int? keepId = selected != null ? (int?)selected.Id : null;
            ListViewItem toShow = null;

            isRefreshing = true;
            lvRequests.BeginUpdate();
            lvRequests.Items.Clear();
            foreach (ServiceRequest request in source)
            {
                ListViewItem item = MakeItem(request);
                lvRequests.Items.Add(item);

                if (keepId.HasValue && request.Id == keepId.Value)
                {
                    item.Selected = true; // keep the user's selection through a refresh
                    toShow = item;
                }
            }
            lvRequests.EndUpdate();
            isRefreshing = false;

            if (toShow != null)
                toShow.EnsureVisible();

            lblListHeading.Text = "Service requests (" + source.Count + " shown of " +
                                  ServiceRequestRepository.Count + ")";

            RefreshExtraTabs();
        }

        private ListViewItem MakeItem(ServiceRequest request)
        {
            ListViewItem item = new ListViewItem(request.Reference);
            item.SubItems.Add(request.StatusText);
            item.SubItems.Add(request.Category);
            item.SubItems.Add(request.Area);
            item.SubItems.Add(PriorityText(request.Priority));
            item.SubItems.Add(request.DateSubmitted.ToString("dd MMM yyyy"));
            item.Tag = request; // so we can get the request back when the row is clicked

            if (!request.IsOpen)
                item.ForeColor = Color.FromArgb(130, 130, 130);  // resolved: muted
            else if (request.Priority == 1)
                item.Font = boldFont;                            // urgent and open: bold

            return item;
        }

        private static string PriorityText(int priority)
        {
            switch (priority)
            {
                case 1: return "High";
                case 2: return "Medium";
                default: return "Routine";
            }
        }

        private static string StatusName(RequestStatus status)
        {
            switch (status)
            {
                case RequestStatus.UnderReview: return "Under Review";
                case RequestStatus.InProgress: return "In Progress";
                default: return status.ToString();
            }
        }

        // EVENT HANDLERS

        private void Filter_Changed(object sender, EventArgs e)
        {
            if (isRefreshing)
                return;

            RefreshList();
        }

        private void List_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isRefreshing)
                return;

            // A ListView fires this twice per click (deselect, then select);
            // the first time nothing is selected, so we ignore it
            if (lvRequests.SelectedItems.Count == 0)
                return;

            selected = (ServiceRequest)lvRequests.SelectedItems[0].Tag;
            ShowSelected();
        }

        /// Looks a request up by its reference in the AVL tree, and reports how many
        /// steps that took compared with the plain BST holding the same IDs.

        private void Track()
        {
            ServiceRequest found;
            int avlSteps;
            int bstSteps;

            if (!ServiceRequestRepository.TryFindByReference(txtTrack.Text, out found, out avlSteps, out bstSteps))
            {
                lblTrackResult.ForeColor = Color.Firebrick;
                lblTrackResult.Text = string.IsNullOrWhiteSpace(txtTrack.Text)
                    ? "Please enter a reference such as SR-1013."
                    : "No request found for \"" + txtTrack.Text.Trim() + "\". Check the reference and try again.";
                return;
            }

            lblTrackResult.ForeColor = AppTheme.TextDark;
            lblTrackResult.Text = found.Reference + " found: " + found.StatusText + " (" + found.Category +
                                  ", " + found.Area + ").   Steps to find it:  AVL tree " + avlSteps +
                                  "  |  plain BST " + bstSteps;

            lastAvlSteps = avlSteps;
            lastBstSteps = bstSteps;

            SelectRequest(found);
        }

        private void SelectRequest(ServiceRequest request)
        {
            selected = request;
            RefreshList();

            // If the current filters hide the request, clear them so it can be shown
            bool visible = lvRequests.Items.Cast<ListViewItem>()
                .Any(item => ((ServiceRequest)item.Tag).Id == request.Id);

            if (!visible)
            {
                isRefreshing = true;
                cmbStatus.SelectedIndex = 0;
                cmbPeriod.SelectedIndex = 0;
                isRefreshing = false;
                RefreshList();
            }

            ShowSelected();
        }

        private void BtnAdvance_Click(object sender, EventArgs e)
        {
            if (selected == null)
                return;

            if (!selected.Advance())
            {
                MessageBox.Show("This request has already been resolved.", "Request Resolved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            RefreshList();
            ShowSelected();
        }

        // SHOWING THE SELECTED REQUEST

        private void ShowSelected()
        {
            if (selected == null)
            {
                rtbDetails.Text = "Select a request from the list, or track one by its reference above.";
                lstHistory.Items.Clear();
                UpdateStages(null);
                AnimateProgressTo(0);
                SetAdvanceEnabled(false);
                RefreshExtraTabs();
                return;
            }

            rtbDetails.Clear();
            AppendLine(selected.Title, titleFont, AppTheme.Primary);
            AppendLine(selected.Reference + "  |  " + selected.StatusText, AppTheme.ButtonFont, AppTheme.Secondary);
            AppendLine("", AppTheme.BodyFont, AppTheme.TextDark);
            AppendLine("Category: " + selected.Category + "  (" + DepartmentMap.DepartmentFor(selected.Category) + ")", AppTheme.BodyFont, AppTheme.TextDark);
            AppendLine("Area: " + selected.Area, AppTheme.BodyFont, AppTheme.TextDark);
            AppendLine("Location: " + selected.Location, AppTheme.BodyFont, AppTheme.TextDark);
            AppendLine("Priority: " + PriorityText(selected.Priority), AppTheme.BodyFont, AppTheme.TextDark);
            AppendLine("Submitted: " + selected.DateSubmitted.ToString("dddd, dd MMMM yyyy"), AppTheme.BodyFont, AppTheme.TextDark);
            if (selected.AttachmentPaths.Count > 0)
                AppendLine("Attachments: " + selected.AttachmentPaths.Count, AppTheme.BodyFont, AppTheme.TextDark);
            AppendLine("", AppTheme.BodyFont, AppTheme.TextDark);
            AppendLine(selected.Description, AppTheme.BodyFont, AppTheme.TextDark);

            lstHistory.Items.Clear();
            foreach (StatusChange change in selected.History)
                lstHistory.Items.Add(change.When.ToString("dd MMM yyyy  HH:mm") + "   -   " + StatusName(change.Status));

            UpdateStages(selected.Status);
            AnimateProgressTo(selected.ProgressPercent);
            SetAdvanceEnabled(selected.IsOpen);

            RefreshExtraTabs();
        }

        private void AppendLine(string text, Font font, Color color)
        {
            WriteLine(rtbDetails, text, font, color);
        }

        /// Highlights the stages the request has reached. Passing null greys them all out.

        private void UpdateStages(RequestStatus? status)
        {
            for (int i = 0; i < stageLabels.Length; i++)
            {
                bool reached = status.HasValue && i <= (int)status.Value;
                stageLabels[i].ForeColor = reached ? AppTheme.Primary : Color.FromArgb(150, 150, 150);
                stageLabels[i].Font = reached ? AppTheme.ButtonFont : AppTheme.BodyFont;
            }
        }

        /// Slides the progress bar to its new value instead of jumping.
        
        private void AnimateProgressTo(int target)
        {
            if (progressTimer != null)
            {
                progressTimer.Stop();
                progressTimer.Dispose();
            }

            progressTimer = new Timer();
            progressTimer.Interval = 8;
            progressTimer.Tick += (s, e) =>
            {
                if (progressStatus.Value < target)
                    progressStatus.Value = Math.Min(progressStatus.Value + 2, target);
                else if (progressStatus.Value > target)
                    progressStatus.Value = Math.Max(progressStatus.Value - 2, target);
                else
                    progressTimer.Stop();
            };
            progressTimer.Start();
        }

        // EXTRA TABS
        
        /// A ListBox entry that remembers which request it stands for, so a
        /// double-click can open the right one.

        private class RequestEntry
        {
            public ServiceRequest Request { get; private set; }
            private readonly string text;

            public RequestEntry(ServiceRequest request, string text)
            {
                Request = request;
                this.text = text;
            }

            public override string ToString()
            {
                return text;
            }
        }

        private void BuildExtraTabs(TabControl tabControl)
        {
            tabControl.TabPages.Add(BuildDepartmentTab());
            tabControl.TabPages.Add(BuildCrewTab());
            tabControl.TabPages.Add(BuildStructuresTab());
        }

        private void RefreshExtraTabs()
        {
            RefreshHierarchy();
            RefreshCrewPlan();
            RefreshStructures();
        }

        //Departments tab (general tree)

        private TabPage BuildDepartmentTab()
        {
            TabPage page = NewTabPage("Departments");

            TableLayoutPanel layout = NewColumnLayout(2, Padding.Empty);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            layout.Controls.Add(NewSectionLabel("Municipality > Department > Category > Request"), 0, 0);

            tvHierarchy = new TreeView();
            tvHierarchy.Dock = DockStyle.Fill;
            tvHierarchy.Margin = new Padding(0, 0, 0, 5);
            tvHierarchy.Font = AppTheme.BodyFont;
            tvHierarchy.ForeColor = AppTheme.TextDark;
            tvHierarchy.BorderStyle = BorderStyle.FixedSingle;
            tvHierarchy.HideSelection = false;
            tvHierarchy.AfterSelect += Hierarchy_AfterSelect;
            layout.Controls.Add(tvHierarchy, 0, 1);

            page.Controls.Add(layout);
            return page;
        }

        private void Hierarchy_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (isRefreshing)
                return;

            HierarchyNode node = e.Node.Tag as HierarchyNode;
            if (node == null || node.Request == null)
                return; // a department or category: nothing to open

            ServiceRequest request = node.Request;

            // Open it after this event finishes, because opening it rebuilds the tree
            this.BeginInvoke(new Action(() => SelectRequest(request)));
        }

        private void RefreshHierarchy()
        {
            HierarchyNode root = ServiceRequestRepository.BuildHierarchy();
            TreeNode selectedNode = null;

            isRefreshing = true;
            tvHierarchy.BeginUpdate();
            tvHierarchy.Nodes.Clear();
            tvHierarchy.Nodes.Add(ToTreeNode(root, ref selectedNode));
            tvHierarchy.ExpandAll();

            if (selectedNode != null)
            {
                tvHierarchy.SelectedNode = selectedNode;
                selectedNode.EnsureVisible();
            }
            tvHierarchy.EndUpdate();
            isRefreshing = false;
        }

        private TreeNode ToTreeNode(HierarchyNode node, ref TreeNode selectedNode)
        {
            TreeNode treeNode = new TreeNode(node.DisplayText);
            treeNode.Tag = node;

            if (node.Request != null)
            {
                if (!node.Request.IsOpen)
                    treeNode.ForeColor = Color.FromArgb(130, 130, 130);

                if (selected != null && node.Request.Id == selected.Id)
                    selectedNode = treeNode;
            }

            foreach (HierarchyNode child in node.Children)
                treeNode.Nodes.Add(ToTreeNode(child, ref selectedNode));

            return treeNode;
        }

        //Crew Plan tab (heap, BFS, MST + DFS)

        private TabPage BuildCrewTab()
        {
            TabPage page = NewTabPage("Crew Plan");

            TableLayoutPanel layout = NewColumnLayout(6, Padding.Empty);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 24F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 24F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));

            layout.Controls.Add(NewSectionLabel("Most urgent open requests (heap) - double-click to open"), 0, 0);
            lstUrgent = NewListBox();
            lstUrgent.DoubleClick += RequestList_DoubleClick;
            layout.Controls.Add(lstUrgent, 0, 1);

            lblNearbyHeading = NewSectionLabel("Open requests near the selected one (BFS)");
            layout.Controls.Add(lblNearbyHeading, 0, 2);
            lstNearby = NewListBox();
            lstNearby.DoubleClick += RequestList_DoubleClick;
            layout.Controls.Add(lstNearby, 0, 3);

            layout.Controls.Add(NewSectionLabel("Crew route (spanning tree + DFS)"), 0, 4);
            rtbCrewPlan = NewReadOnlyBox();
            layout.Controls.Add(rtbCrewPlan, 0, 5);

            page.Controls.Add(layout);
            return page;
        }

        private void RequestList_DoubleClick(object sender, EventArgs e)
        {
            ListBox list = (ListBox)sender;
            RequestEntry entry = list.SelectedItem as RequestEntry; // null for placeholder text
            if (entry != null)
                SelectRequest(entry.Request);
        }

        private void RefreshCrewPlan()
        {
            // 1. Heap: the most urgent open requests
            lstUrgent.BeginUpdate();
            lstUrgent.Items.Clear();
            foreach (ServiceRequest r in ServiceRequestRepository.MostUrgentOpen(5))
            {
                lstUrgent.Items.Add(new RequestEntry(r,
                    r.Reference + "  |  " + PriorityText(r.Priority) + "  |  " + r.Area + "  |  " + r.StatusText));
            }
            if (lstUrgent.Items.Count == 0)
                lstUrgent.Items.Add("No open requests.");
            lstUrgent.EndUpdate();

            // 2. BFS: open requests in the selected request's area or within two roads of it
            lstNearby.BeginUpdate();
            lstNearby.Items.Clear();
            if (selected == null)
            {
                lblNearbyHeading.Text = "Open requests near the selected one (BFS)";
                lstNearby.Items.Add("Select a request to see open requests near it.");
            }
            else
            {
                lblNearbyHeading.Text = "Near " + selected.Reference + " in " + selected.Area + " (BFS, within 2 roads)";

                List<NearbyRequest> nearby = ServiceRequestRepository.FindNearbyOpen(selected, 2);
                foreach (NearbyRequest n in nearby)
                {
                    string distance = n.Hops == 0
                        ? "same area"
                        : n.Hops + (n.Hops == 1 ? " road away" : " roads away");

                    lstNearby.Items.Add(new RequestEntry(n.Request,
                        n.Request.Reference + "  |  " + n.Request.Area + "  |  " + distance +
                        "  |  " + PriorityText(n.Request.Priority)));
                }
                if (nearby.Count == 0)
                    lstNearby.Items.Add("No other open requests within 2 roads.");
            }
            lstNearby.EndUpdate();

            // 3. Spanning tree + DFS: the least road that connects every area with an open request
            rtbCrewPlan.Clear();

            int openAreas = ServiceRequestRepository.GetOpenAreas().Count;
            if (openAreas == 0)
            {
                WriteLine(rtbCrewPlan, "No open requests, so no crew trip is needed.", AppTheme.BodyFont, AppTheme.TextDark);
                return;
            }

            MstResult plan = ServiceRequestRepository.PlanCrewRoute();
            List<string> route = LocationGraph.Default.RouteThroughTree(plan);
            double fullKm = LocationGraph.Default.TotalRoadKm();

            WriteLine(rtbCrewPlan,
                openAreas + " area(s) have open requests. The crew starts from the depot (" + LocationGraph.Depot + ").",
                AppTheme.BodyFont, AppTheme.TextDark);
            WriteLine(rtbCrewPlan, "", AppTheme.BodyFont, AppTheme.TextDark);

            if (plan.Edges.Count == 0)
            {
                WriteLine(rtbCrewPlan, "No travel needed: every open request is at the depot.", AppTheme.BodyFont, AppTheme.TextDark);
                return;
            }

            WriteLine(rtbCrewPlan, "Roads the crew needs (minimum spanning tree):", AppTheme.ButtonFont, AppTheme.Primary);
            foreach (RoadEdge road in plan.Edges)
            {
                WriteLine(rtbCrewPlan,
                    "   " + road.From + " - " + road.To + "   " + road.Km.ToString("F1") + " km",
                    AppTheme.BodyFont, AppTheme.TextDark);
            }

            double lessRoad = fullKm > 0 ? (1 - plan.TotalKm / fullKm) * 100 : 0;
            WriteLine(rtbCrewPlan,
                "Total: " + plan.TotalKm.ToString("F1") + " km of the network's " + fullKm.ToString("F1") +
                " km (" + lessRoad.ToString("F0") + "% less road)",
                AppTheme.ButtonFont, AppTheme.TextDark);
            WriteLine(rtbCrewPlan, "", AppTheme.BodyFont, AppTheme.TextDark);

            WriteLine(rtbCrewPlan, "Visiting order (depth-first walk of that tree):", AppTheme.ButtonFont, AppTheme.Primary);
            WriteLine(rtbCrewPlan, string.Join(" > ", route), AppTheme.BodyFont, AppTheme.TextDark);

            if (plan.Components > 1)
            {
                WriteLine(rtbCrewPlan, "", AppTheme.BodyFont, AppTheme.TextDark);
                WriteLine(rtbCrewPlan,
                    "Note: some of these areas have no direct road between them, so the plan is " +
                    plan.Components + " separate groups.",
                    AppTheme.BodyFont, AppTheme.TextDark);
            }
        }

        //Structures tab (live statistics for every structure)

        private TabPage BuildStructuresTab()
        {
            TabPage page = NewTabPage("Structures");

            TableLayoutPanel layout = NewColumnLayout(2, Padding.Empty);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            layout.Controls.Add(NewSectionLabel("How each data structure is performing right now"), 0, 0);
            rtbStructures = NewReadOnlyBox();
            layout.Controls.Add(rtbStructures, 0, 1);

            page.Controls.Add(layout);
            return page;
        }

        private void AddStructure(string title, string detail)
        {
            WriteLine(rtbStructures, title, AppTheme.ButtonFont, AppTheme.Primary);
            WriteLine(rtbStructures, detail, AppTheme.BodyFont, AppTheme.TextDark);
            WriteLine(rtbStructures, "", AppTheme.BodyFont, AppTheme.TextDark);
        }

        private void RefreshStructures()
        {
            int count = ServiceRequestRepository.Count;
            int bestHeight = count == 0 ? 0 : (int)Math.Ceiling(Math.Log(count + 1, 2));

            HierarchyNode root = ServiceRequestRepository.BuildHierarchy();
            MstResult plan = ServiceRequestRepository.PlanCrewRoute();
            double fullKm = LocationGraph.Default.TotalRoadKm();

            rtbStructures.Clear();

            if (lastAvlSteps > 0)
            {
                AddStructure("Last lookup",
                    "Tracking found the request in " + lastAvlSteps + " steps using the AVL tree. " +
                    "The plain BST needed " + lastBstSteps + " steps for the same ID.");
            }
            else
            {
                AddStructure("Last lookup", "Track a request by its reference to compare the two trees.");
            }

            AddStructure("AVL tree (keyed by ID)",
                count + " requests, height " + ServiceRequestRepository.ById.Height() +
                " (the best possible is " + bestHeight + "). " +
                ServiceRequestRepository.ById.RotationCount + " rotations kept it balanced. " +
                "Used for: tracking by reference and sorting by ID.");

            AddStructure("Plain binary search tree (keyed by ID)",
                "Height " + ServiceRequestRepository.BaselineById.Height() + " for the same " + count +
                " requests. IDs arrive in order, so it has become a chain and a lookup can need that many steps. " +
                "Kept only as the baseline for comparison.");

            AddStructure("Red-black tree (keyed by date submitted)",
                count + " requests, height " + ServiceRequestRepository.ByDate.Height() + ", " +
                ServiceRequestRepository.ByDate.RotationCount + " rotations and " +
                ServiceRequestRepository.ByDate.ColourFlipCount + " colour flips. Valid red-black tree: " +
                ServiceRequestRepository.ByDate.IsValid() + ". Used for: sorting by date and the Period filter.");

            AddStructure("Min-heap (urgency)",
                "Ranks all " + count + " requests: open before resolved, then priority, then the longest waiting. " +
                "Used for: sorting by urgency and the five most urgent open requests.");

            AddStructure("General tree (departments)",
                root.PreOrder().Count() + " nodes, " + root.Children.Count + " departments, height " + root.Height() +
                ". Used for: browsing requests by department and category.");

            AddStructure("Graph (road network)",
                LocationGraph.Default.AreaCount + " areas, " + LocationGraph.Default.RoadCount + " roads, " +
                fullKm.ToString("F1") + " km of road. Used for: matching a typed location to an area, " +
                "finding nearby requests (BFS) and planning the crew's trip.");

            AddStructure("Minimum spanning tree (Prim's algorithm)",
                plan.Edges.Count + " roads, " + plan.TotalKm.ToString("F1") + " km out of the full " +
                fullKm.ToString("F1") + " km. Built with the heap. The visiting order is a depth-first walk of this tree.");
        }

        // ANIMATION AND CLEAN-UP


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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (fadeInTimer != null)
            {
                fadeInTimer.Stop();
                fadeInTimer.Dispose();
            }
            if (progressTimer != null)
            {
                progressTimer.Stop();
                progressTimer.Dispose();
            }
            boldFont.Dispose();
            titleFont.Dispose();
            base.OnFormClosed(e);
        }
    }
}