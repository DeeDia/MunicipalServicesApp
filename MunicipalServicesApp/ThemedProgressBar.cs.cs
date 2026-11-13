using System.Drawing;
using System.Windows.Forms;

namespace MunicipalServicesApp
{
    /// A ProgressBar that draws itself in the application's colour scheme
    /// instead of the default Windows green.
 
    public class ThemedProgressBar : ProgressBar
    {
        public ThemedProgressBar()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.FromArgb(235, 235, 235));

            double fraction = Maximum > Minimum
                ? (double)(Value - Minimum) / (Maximum - Minimum)
                : 0;
            int filledWidth = (int)(Width * fraction);

            if (filledWidth > 0)
            {
                using (SolidBrush brush = new SolidBrush(AppTheme.Primary))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, filledWidth, Height);
                }
            }
        }
    }
}