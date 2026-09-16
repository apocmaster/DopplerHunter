using System.Windows;
using System.Windows.Controls;

namespace DopplerHunter.Views.Controls
{
    public class StatCard : Control
    {
        #region Static Constructor

        static StatCard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(StatCard),
                new FrameworkPropertyMetadata(typeof(StatCard)));
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(StatCard),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(object),
                typeof(StatCard),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ValueStyleProperty =
            DependencyProperty.Register(
                nameof(ValueStyle),
                typeof(Style),
                typeof(StatCard),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TitleStyleProperty =
            DependencyProperty.Register(
                nameof(TitleStyle),
                typeof(Style),
                typeof(StatCard),
                new PropertyMetadata(null));

        #endregion

        #region Properties

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public object? Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public Style? ValueStyle
        {
            get => (Style?)GetValue(ValueStyleProperty);
            set => SetValue(ValueStyleProperty, value);
        }

        public Style? TitleStyle
        {
            get => (Style?)GetValue(TitleStyleProperty);
            set => SetValue(TitleStyleProperty, value);
        }

        #endregion
    }
}
