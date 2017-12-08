using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Parrot.Viewer.Views
{
    /// <summary>Логика взаимодействия для SingleView.xaml</summary>
    public partial class SingleView : UserControl
    {
        public static readonly DependencyProperty IsAnimatedRightNowProperty = DependencyProperty.Register(
            "IsAnimatedRightNow", typeof(bool), typeof(SingleView), new PropertyMetadata(default(bool)));

        public SingleView() { InitializeComponent(); }

        public bool IsAnimatedRightNow
        {
            get => (bool)GetValue(IsAnimatedRightNowProperty);
            set => SetValue(IsAnimatedRightNowProperty, value);
        }
    }
}
