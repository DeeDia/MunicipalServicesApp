using System;
using System.Drawing;
using System.Windows.Forms;

namespace MunicipalServicesApp
{
    public partial class MainMenuForm : Form
    {
        // Timer used to fade the form in smoothly on load, instead of it just "popping" into view
        private Timer fadeInTimer;

        public MainMenuForm()
        {
            InitializeComponent();
            ApplyTheme();
            SetupFadeInAnimation();
        }

        // Applies the shared colour scheme/fonts to every control on this form.
                
        private void ApplyTheme()
        {
            this.BackColor = AppTheme.Background;

            lblTitle.Font = AppTheme.HeadingFont;
            lblTitle.ForeColor = AppTheme.Primary;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            lblSubtitle.Font = AppTheme.BodyFont;
            lblSubtitle.ForeColor = AppTheme.TextDark;
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;

            StyleMenuButton(btnReportIssues, enabled: true);
            StyleMenuButton(btnLocalEvents, enabled: false);
            StyleMenuButton(btnServiceStatus, enabled: false);
        }

        // Styles a main-menu button.
        
        private void StyleMenuButton(Button btn, bool enabled)
        {
            btn.Enabled = enabled;
            btn.Font = AppTheme.ButtonFont;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Height = 45;
            btn.Cursor = enabled ? Cursors.Hand : Cursors.No;

            if (enabled)
            {
                btn.BackColor = AppTheme.Primary;
                btn.ForeColor = AppTheme.TextLight;
                btn.FlatAppearance.MouseOverBackColor = AppTheme.Accent;
            }
            else
            {
                btn.BackColor = AppTheme.Disabled;
                btn.ForeColor = Color.FromArgb(120, 120, 120);
            }
        }

        // Fades the form in from transparent to fully opaque on load.
                
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

        private void btnReportIssues_Click(object sender, EventArgs e)
        {
            var reportForm = new ReportIssueForm();
            reportForm.ShowDialog(); // modal - blocks this window until the report form closes
        }
    }
}