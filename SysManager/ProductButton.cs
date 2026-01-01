using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace SysManager
{
    public class ProductButton : Button  // Schimbă de la Control la Button
    {
        public static readonly DependencyProperty ProductNameProperty =
            DependencyProperty.Register("ProductName", typeof(string), typeof(ProductButton));

        public static readonly DependencyProperty PriceProperty =
            DependencyProperty.Register("Price", typeof(string), typeof(ProductButton));

        public static readonly DependencyProperty StockProperty =
            DependencyProperty.Register("Stock", typeof(string), typeof(ProductButton));

        public static readonly DependencyProperty ImagePathProperty =
            DependencyProperty.Register("ImagePath", typeof(string), typeof(ProductButton));

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

        public string Stock
        {
            get => (string)GetValue(StockProperty);
            set => SetValue(StockProperty, value);
        }

        public string ImagePath
        {
            get => (string)GetValue(ImagePathProperty);
            set => SetValue(ImagePathProperty, value);
        }

        static ProductButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProductButton),
                new FrameworkPropertyMetadata(typeof(ProductButton)));
        }
    }
}

