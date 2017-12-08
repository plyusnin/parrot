using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ReactiveUI;

namespace Parrot.Viewer.Views.Controls
{
    /// <summary>Логика взаимодействия для ImagePointer.xaml</summary>
    public partial class ImagePointer : UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(ImageSource), typeof(ImagePointer), new PropertyMetadata(default(ImageSource)));

        private static readonly Duration _zoomDuration = new Duration(TimeSpan.FromMilliseconds(400));

        private readonly Subject<Point> _mousePoint = new Subject<Point>();

        public ImagePointer()
        {
            InitializeComponent();

            var relativePoint = _mousePoint.Select(p => new Point(p.X / ActualWidth,
                                                                  p.Y / ActualHeight))
                                           .Select(p => new Point(Math.Max(0, Math.Min(1, p.X)),
                                                                  Math.Max(0, Math.Min(1, p.Y))))
                                           .StartWith(new Point(0.5, 0.5));

            relativePoint.BindTo(MyImage, x => x.RenderTransformOrigin);
        }

        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (Source == null)
                return;
            CaptureMouse();
            var scale = Math.Min(Source.Width / MyImage.ActualWidth,
                                 Source.Height / MyImage.ActualHeight);
            scale = Math.Max(1, scale);
            ImageTransform.BeginAnimation(ScaleTransform.ScaleXProperty, ZoomAnimation(scale));
            ImageTransform.BeginAnimation(ScaleTransform.ScaleYProperty, ZoomAnimation(scale));
            _mousePoint.OnNext(e.GetPosition(this));
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
                ImageTransform.BeginAnimation(ScaleTransform.ScaleXProperty, ZoomAnimation());
                ImageTransform.BeginAnimation(ScaleTransform.ScaleYProperty, ZoomAnimation());
                _mousePoint.OnNext(e.GetPosition(this));
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (IsMouseCaptured)
                _mousePoint.OnNext(e.GetPosition(this));
        }

        private DoubleAnimation ZoomAnimation(double To)
        {
            return new DoubleAnimation(To, _zoomDuration)
                   {
                       EasingFunction = new PowerEase { EasingMode = EasingMode.EaseOut },
                       FillBehavior = FillBehavior.HoldEnd
                   };
        }

        private DoubleAnimation ZoomAnimation()
        {
            return new DoubleAnimation
                   {
                       Duration = _zoomDuration,
                       EasingFunction = new PowerEase { EasingMode = EasingMode.EaseOut },
                       FillBehavior = FillBehavior.HoldEnd
                   };
        }
    }
}
