using System.Windows;
using System.Windows.Controls;

namespace SysManager.Controls
{
    public class ActionButton : Button
    {
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(string), typeof(ActionButton), new PropertyMetadata("✓"));

        public static readonly DependencyProperty ButtonTypeProperty =
            DependencyProperty.Register(nameof(ButtonType), typeof(ActionButtonType), typeof(ActionButton),
                new PropertyMetadata(ActionButtonType.Primary));

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public ActionButtonType ButtonType
        {
            get => (ActionButtonType)GetValue(ButtonTypeProperty);
            set => SetValue(ButtonTypeProperty, value);
        }

        static ActionButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ActionButton),
                new FrameworkPropertyMetadata(typeof(ActionButton)));
        }
    }

    public enum ActionButtonType
    {
        Primary,
        Danger,
        Warning,
        Info
    }
}
