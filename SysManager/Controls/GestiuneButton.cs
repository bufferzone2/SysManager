using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SysManager.Controls
{
    public class GestiuneButton : Button
    {
        static GestiuneButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GestiuneButton),
                new FrameworkPropertyMetadata(typeof(GestiuneButton)));
        }

        // ✅ NUME GESTIUNE (Afișat pe buton)
        public static readonly DependencyProperty GestiuneNameProperty =
            DependencyProperty.Register("GestiuneName", typeof(string), typeof(GestiuneButton),
                new PropertyMetadata(string.Empty));

        public string GestiuneName
        {
            get { return (string)GetValue(GestiuneNameProperty); }
            set { SetValue(GestiuneNameProperty, value); }
        }

        // ✅ CULOARE GESTIUNE
        public static readonly DependencyProperty GestiuneColorProperty =
            DependencyProperty.Register("GestiuneColor", typeof(Brush), typeof(GestiuneButton),
                new PropertyMetadata(Brushes.DarkOrange));

        public Brush GestiuneColor
        {
            get { return (Brush)GetValue(GestiuneColorProperty); }
            set { SetValue(GestiuneColorProperty, value); }
        }

        // ✅ SELECȚIE
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(GestiuneButton),
                new PropertyMetadata(false));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // ✅ ID GESTIUNE
        public static readonly DependencyProperty GestiuneIdProperty =
            DependencyProperty.Register("GestiuneId", typeof(int), typeof(GestiuneButton),
                new PropertyMetadata(0));

        public int GestiuneId
        {
            get { return (int)GetValue(GestiuneIdProperty); }
            set { SetValue(GestiuneIdProperty, value); }
        }
    }
}
