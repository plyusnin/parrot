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
            "IsAnimatedRightNow", typeof(bool), typeof(SingleView), new PropertyMetadata(default(bool), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject O, DependencyPropertyChangedEventArgs args)
        {
            //((SingleView)O).Picture.SetValue(RenderOptions.BitmapScalingModeProperty, args.NewValue ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.Fant);
        }

        public SingleView() { InitializeComponent(); }

        public bool IsAnimatedRightNow
        {
            get => (bool)GetValue(IsAnimatedRightNowProperty);
            set => SetValue(IsAnimatedRightNowProperty, value);
        }
    }
}
