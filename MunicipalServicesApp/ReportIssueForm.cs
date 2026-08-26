using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace MunicipalServicesApp
{
    public partial class ReportIssueForm : Form
    {
        // Tracks the full path of the file the user attaches, if any
        private string attachedFilePath = string.Empty;

        public ReportIssueForm()
        {
            InitializeComponent();
            LayoutControls();
            ApplyTheme();
            PopulateCategories();
            WireUpEvents();
            SetupFadeInAnimation();

        }

        // Positions and sizes every control in code. Keeping layout here
        
        private void LayoutControls()
        {
            int leftMargin = 30;
            int fieldWidth = 440;
            int y = 20;

            lblFormTitle.SetBounds(leftMargin, y, fieldWidth, 30);
            y += 45;

            lblLocation.SetBounds(leftMargin, y, fieldWidth, 20);
            y += 22;
            txtLocation.SetBounds(leftMargin, y, fieldWidth, 26);
            y += 40;

            lblCategory.SetBounds(leftMargin, y, fieldWidth, 20);
            y += 22;
            cmbCategory.SetBounds(leftMargin, y, fieldWidth, 26);
            y += 40;

            lblDescription.SetBounds(leftMargin, y, fieldWidth, 20);
            y += 22;
            rtbDescription.SetBounds(leftMargin, y, fieldWidth, 110);
            y += 122;

            btnAttach.SetBounds(leftMargin, y, 220, 32);
            lblAttachedFile.SetBounds(leftMargin + 230, y + 6, fieldWidth - 230, 20);
            y += 45;

            lblEngagement.SetBounds(leftMargin, y, fieldWidth, 20);
            y += 22;
            progressEngagement.SetBounds(leftMargin, y, fieldWidth, 20);
            y += 40;

            btnSubmit.SetBounds(leftMargin, y, fieldWidth, 38);
            y += 48;

            btnBack.SetBounds(leftMargin, y, fieldWidth, 32);
            y += 45;

            this.ClientSize = new Size(fieldWidth + 2 * leftMargin, y);
        }

        // Applies the shared colour palette and fonts, consistent with the Main Menu.
        
        private void ApplyTheme()
        {
            this.BackColor = AppTheme.Background;

            lblFormTitle.Font = AppTheme.HeadingFont;
            lblFormTitle.ForeColor = AppTheme.Primary;

            foreach (var lbl in new[] { lblLocation, lblCategory, lblDescription, lblEngagement })
            {
                lbl.Font = AppTheme.BodyFont;
                lbl.ForeColor = AppTheme.TextDark;
            }

            lblAttachedFile.Font = AppTheme.BodyFont;
            lblAttachedFile.ForeColor = AppTheme.Secondary;
            lblAttachedFile.Text = string.Empty; // nothing attached yet

            lblEngagement.Text = "Fill in the details below to help us respond faster.";

            txtLocation.Font = AppTheme.BodyFont;
            cmbCategory.Font = AppTheme.BodyFont;
            rtbDescription.Font = AppTheme.BodyFont;

            StyleActionButton(btnAttach, AppTheme.Secondary);
            StyleActionButton(btnSubmit, AppTheme.Primary);
            StyleActionButton(btnBack, AppTheme.Disabled, textColor: AppTheme.TextDark);

            progressEngagement.Minimum = 0;
            progressEngagement.Maximum = 100;
            progressEngagement.Value = 0;
        }

        private void StyleActionButton(Button btn, Color backColor, Color? textColor = null)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = AppTheme.ButtonFont;
            btn.BackColor = backColor;
            btn.ForeColor = textColor ?? AppTheme.TextLight;
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.MouseOverBackColor = AppTheme.Accent;
        }

        // Fills the category dropdown. Centralised here so adding/removing
        // a category later means changing one list, not hunting through the designer.
        
        private void PopulateCategories()
        {
            cmbCategory.Items.Clear();
            cmbCategory.Items.AddRange(new object[]
            {
                "Sanitation",
                "Roads",
                "Utilities (Water/Electricity)",
                "Waste Management",
                "Public Safety",
                "Other"
            });
        }


        // Wires up event handlers for all interactive controls.
        
        private void WireUpEvents()
        {
            btnAttach.Click += BtnAttach_Click;
            btnSubmit.Click += BtnSubmit_Click;
            btnBack.Click += BtnBack_Click;

            // Update the engagement progress bar every time the user changes a field
            txtLocation.TextChanged += (s, e) => UpdateEngagementProgress();
            cmbCategory.SelectedIndexChanged += (s, e) => UpdateEngagementProgress();
            rtbDescription.TextChanged += (s, e) => UpdateEngagementProgress();
        }

        // Opens a file dialog so the user can attach an image or document to their report.
        
        private void BtnAttach_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Attach an image or document";
                dialog.Filter = "Supported files (*.jpg;*.jpeg;*.png;*.pdf;*.docx)|*.jpg;*.jpeg;*.png;*.pdf;*.docx|All files (*.*)|*.*";
                dialog.Multiselect = false;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    attachedFilePath = dialog.FileName;
                    lblAttachedFile.Text = "Attached: " + Path.GetFileName(attachedFilePath);
                    FlashLabel(lblAttachedFile);
                }
            }
        }

        // Briefly flashes a label's colour to the accent colour then back,
        // drawing the eye to a change without needing any external animation library.
        
        private void FlashLabel(Label label)
        {
            Color originalColor = label.ForeColor;
            label.ForeColor = AppTheme.Accent;
            label.Font = new Font(label.Font, FontStyle.Bold);

            Timer flashTimer = new Timer();
            flashTimer.Interval = 600;
            flashTimer.Tick += (s, e) =>
            {
                label.ForeColor = originalColor;
                label.Font = new Font(label.Font, FontStyle.Regular);
                flashTimer.Stop();
                flashTimer.Dispose();
            };
            flashTimer.Start();
        }

        // Recalculates how "complete" the report feels and updates the progress bar
        // and encouragement message accordingly.
        private void UpdateEngagementProgress()
        {
            int score = 0;
            int stepValue = 25; // 4 meaningful fields = 25% each

            if (!string.IsNullOrWhiteSpace(txtLocation.Text)) score += stepValue;
            if (cmbCategory.SelectedItem != null) score += stepValue;
            if (!string.IsNullOrWhiteSpace(rtbDescription.Text)) score += stepValue;
            if (!string.IsNullOrEmpty(attachedFilePath)) score += stepValue;

            AnimateProgressTo(score);

            if (score == 0)
                lblEngagement.Text = "Fill in the details below to help us respond faster.";
            else if (score < 100)
                lblEngagement.Text = "Great progress! A few more details will help us act quickly.";
            else
                lblEngagement.Text = "Your report is complete and ready to submit. Thank you!";
        }

        // Smoothly animates the progress bar to a target value instead of jumping instantly,
        // giving the same "polished" feel React animation would, using a native WinForms Timer.
        
        private void AnimateProgressTo(int targetValue)
        {
            Timer progressTimer = new Timer();
            progressTimer.Interval = 10;
            progressTimer.Tick += (s, e) =>
            {
                if (progressEngagement.Value < targetValue)
                {
                    progressEngagement.Value += 1;
                }
                else if (progressEngagement.Value > targetValue)
                {
                    progressEngagement.Value -= 1;
                }
                else
                {
                    progressTimer.Stop();
                    progressTimer.Dispose();
                }
            };
            progressTimer.Start();
        }

        // Validates input, builds an Issue object, stores it, and gives the user
        // clear success/error feedback.
        
        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLocation.Text))
            {
                MessageBox.Show("Please enter the location of the issue.", "Missing Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLocation.Focus();
                return;
            }

            if (cmbCategory.SelectedItem == null)
            {
                MessageBox.Show("Please select a category for the issue.", "Missing Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCategory.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(rtbDescription.Text))
            {
                MessageBox.Show("Please provide a description of the issue.", "Missing Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                rtbDescription.Focus();
                return;
            }

            Issue newIssue = new Issue
            {
                Location = txtLocation.Text.Trim(),
                Category = cmbCategory.SelectedItem.ToString(),
                Description = rtbDescription.Text.Trim()
            };

            if (!string.IsNullOrEmpty(attachedFilePath))
            {
                newIssue.AttachmentPaths.Add(attachedFilePath);
            }

            IssueRepository.Issues.Add(newIssue);

            MessageBox.Show(
                "Thank you! Your issue has been reported successfully.\n\n" +
                "Reference: Issue #" + IssueRepository.Issues.Count,
                "Report Submitted",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            this.Close();
        }

        // Returns the user to the Main Menu without saving anything.
        
        private void BtnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private Timer fadeInTimer;

        private void SetupFadeInAnimation()
        {
            this.Opacity = 0;
            fadeInTimer = new Timer();
            fadeInTimer.Interval = 15; // ms between ticks - smaller = smoother
            fadeInTimer.Tick += (s, e) =>
            {
                if (this.Opacity < 1.0)
                {
                    this.Opacity += 0.06;
                }
                else
                {
                    this.Opacity = 1.0;
                    fadeInTimer.Stop();
                }
            };
            fadeInTimer.Start();
   
        }
    }
}