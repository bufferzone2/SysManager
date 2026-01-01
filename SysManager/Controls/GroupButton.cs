using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SysManager.Controls
{
    public class GroupButton : Button
    {
        static GroupButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GroupButton),
                new FrameworkPropertyMetadata(typeof(GroupButton)));
        }

        // ✅ PROPRIETATE PENTRU NUME GRUPĂ
        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register("GroupName", typeof(string), typeof(GroupButton),
                new PropertyMetadata(string.Empty));

        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        // ✅ MODIFICAT: ProductCount este acum STRING în loc de INT
        public static readonly DependencyProperty ProductCountProperty =
            DependencyProperty.Register("ProductCount", typeof(string), typeof(GroupButton),
                new PropertyMetadata(string.Empty)); // ❌ Era: typeof(int), 0

        public string ProductCount // ❌ Era: public int ProductCount
        {
            get { return (string)GetValue(ProductCountProperty); }
            set { SetValue(ProductCountProperty, value); }
        }

        // ✅ PROPRIETATE PENTRU CULOARE
        public static readonly DependencyProperty GroupColorProperty =
            DependencyProperty.Register("GroupColor", typeof(Brush), typeof(GroupButton),
                new PropertyMetadata(Brushes.Blue));

        public Brush GroupColor
        {
            get { return (Brush)GetValue(GroupColorProperty); }
            set { SetValue(GroupColorProperty, value); }
        }

        // ✅ NOU: PROPRIETATE PENTRU SELECȚIE
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(GroupButton),
                new PropertyMetadata(false));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // ✅ NOU: ID GRUPĂ (pentru identificare)
        public static readonly DependencyProperty GroupIdProperty =
            DependencyProperty.Register("GroupId", typeof(int), typeof(GroupButton),
                new PropertyMetadata(0));

        public int GroupId
        {
            get { return (int)GetValue(GroupIdProperty); }
            set { SetValue(GroupIdProperty, value); }
        }
    }
}
