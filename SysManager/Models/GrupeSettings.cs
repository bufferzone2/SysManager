using System.Windows.Media;

namespace SysManager.Models
{
    public class GrupeSettings
    {
        public int Id { get; set; }
        public int Inaltime { get; set; }
        public int Latime { get; set; }
        public int PanouHeight { get; set; }

        // String-uri din DB
        public string BorderColor { get; set; }
        public string ColorNormal { get; set; }
        public string ColorPressed { get; set; }
        public string ColorHover { get; set; }
        public string ColorDisabled { get; set; }

        // ✅ PROPRIETĂȚI CONVERTITE PENTRU WPF
        public Color NormalColor => ParseColor(ColorNormal, Color.FromRgb(0, 102, 204));
        public Color PressedColor => ParseColor(ColorPressed, Color.FromRgb(46, 125, 50)); // Verde pentru selected
        public Color HoverColor => ParseColor(ColorHover, Color.FromRgb(0, 82, 184));
        public Color DisabledColor => ParseColor(ColorDisabled, Color.FromRgb(128, 128, 128));
        public Color BorderColorParsed => ParseColor(BorderColor, Color.FromRgb(46, 92, 138));

        // ✅ METODĂ HELPER PENTRU CONVERSIE
        private Color ParseColor(string hex, Color defaultColor)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hex))
                    return defaultColor;

                hex = hex.Trim().TrimStart('#');

                if (hex.Length != 6)
                    return defaultColor;

                byte r = System.Convert.ToByte(hex.Substring(0, 2), 16);
                byte g = System.Convert.ToByte(hex.Substring(2, 2), 16);
                byte b = System.Convert.ToByte(hex.Substring(4, 2), 16);

                return Color.FromRgb(r, g, b);
            }
            catch
            {
                return defaultColor;
            }
        }
    }
}
