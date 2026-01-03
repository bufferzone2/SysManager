//Controls/POSButton.cs
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SysManager.Controls
{
    public class POSButton : Button
    {
        public static readonly DependencyProperty ProductNameProperty =
            DependencyProperty.Register(nameof(ProductName), typeof(string), typeof(POSButton), new PropertyMetadata("Produs"));

        public static readonly DependencyProperty PriceProperty =
            DependencyProperty.Register(nameof(Price), typeof(string), typeof(POSButton), new PropertyMetadata("0.00 RON"));

        public static readonly DependencyProperty ImagePathProperty =
            DependencyProperty.Register(nameof(ImagePath), typeof(ImageSource), typeof(POSButton), new PropertyMetadata(null));

        public static readonly DependencyProperty ProductColorProperty =
            DependencyProperty.Register(nameof(ProductColor), typeof(Brush), typeof(POSButton),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(67, 160, 71))));

        // ✅ PROPRIETATE PENTRU NUMELE GESTIUNII
        public static readonly DependencyProperty GestiuneNameProperty =
            DependencyProperty.Register(nameof(GestiuneName), typeof(string), typeof(POSButton), new PropertyMetadata(string.Empty));

        public string ProductName
        {
            get => (string)GetValue(ProductNameProperty);
            set => SetValue(ProductNameProperty, value);
        }

        public string Price
        {
            get => (string)GetValue(PriceProperty);
            set => SetValue(PriceProperty, value);
        }

        public ImageSource ImagePath
        {
            get => (ImageSource)GetValue(ImagePathProperty);
            set => SetValue(ImagePathProperty, value);
        }

        public Brush ProductColor
        {
            get => (Brush)GetValue(ProductColorProperty);
            set => SetValue(ProductColorProperty, value);
        }

        // ✅ NUME GESTIUNE (afișat în partea stângă)
        public string GestiuneName
        {
            get => (string)GetValue(GestiuneNameProperty);
            set => SetValue(GestiuneNameProperty, value);
        }

        static POSButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(POSButton),
                new FrameworkPropertyMetadata(typeof(POSButton)));
        }
    }
}
