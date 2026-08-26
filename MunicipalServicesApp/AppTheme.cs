using System.Drawing;

namespace MunicipalServicesApp
{
    
    // Place for consistent colour palette and shared styling across the app.
    
    public static class AppTheme
    {
        // Primary brand colours
        public static readonly Color Primary = ColorTranslator.FromHtml("#BF5700"); 
        public static readonly Color Secondary = ColorTranslator.FromHtml("#E09540"); 
        public static readonly Color Accent = ColorTranslator.FromHtml("#ED9121"); 

        public static readonly Color Background = Color.White;
        public static readonly Color TextDark = Color.FromArgb(40, 40, 40);
        public static readonly Color TextLight = Color.White;
        public static readonly Color Disabled = Color.FromArgb(200, 200, 200);

        public static readonly Font HeadingFont = new Font("Segoe UI", 16F, FontStyle.Bold);
        public static readonly Font BodyFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        public static readonly Font ButtonFont = new Font("Segoe UI", 10F, FontStyle.Bold);
    }
}
