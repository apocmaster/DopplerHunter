using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DopplerHunter.Views.Controls
{
    /// <summary>
    /// Interaction logic for PageHeader.xaml
    /// </summary>
    public partial class PageHeader : UserControl
    {
        public PageHeader()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IconStringProperty =
            DependencyProperty.Register(
                nameof(IconString),
                typeof(string),
                typeof(PageHeader),
                new PropertyMetadata("✨"));

        public string IconString
        {
            get { return (string)GetValue(IconStringProperty); }
            set { SetValue(IconStringProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(PageHeader),
                new PropertyMetadata("Page Title"));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
    }
}
