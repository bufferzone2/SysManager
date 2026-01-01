using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SysManager.Controls
{
    public class DarkerColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return Color.FromRgb(
                    (byte)(color.R * 0.8),
                    (byte)(color.G * 0.8),
                    (byte)(color.B * 0.8));
            }
            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LighterColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return Color.FromRgb(
                    (byte)Math.Min(255, color.R * 1.2),
                    (byte)Math.Min(255, color.G * 1.2),
                    (byte)Math.Min(255, color.B * 1.2));
            }
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
