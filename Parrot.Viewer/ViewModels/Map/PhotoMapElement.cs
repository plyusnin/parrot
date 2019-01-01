using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Geographics;
using MapVisualization.Elements;

namespace Parrot.Viewer.ViewModels.Map
{
    public class PhotoMapElement : MapPointElement
    {
        private static readonly Typeface _typeface = new Typeface("Tahoma");
        private readonly int _photosCount;
        private readonly double _previewSize = 50;
        private readonly Lazy<ImageSource> _thumbnail;
        private readonly Stream _thumbnailStream;

        public PhotoMapElement(EarthPoint Position, int PhotosCount, Stream ThumbnailStream) : base(Position)
        {
            _photosCount = PhotosCount;
            _thumbnailStream = ThumbnailStream;
            _thumbnail = new Lazy<ImageSource>(LoadThumbnail);
        }

        protected override int ZIndex => 10;

        private ImageSource LoadThumbnail()
        {
            var thumbnail = new BitmapImage();
            thumbnail.BeginInit();
            thumbnail.StreamSource = _thumbnailStream;
            thumbnail.CacheOption = BitmapCacheOption.OnDemand;
            thumbnail.EndInit();
            return thumbnail;
        }

        protected override void DrawPointElement(DrawingContext dc, int Zoom)
        {
            var radius = _previewSize / 2;

            if (IsMouseOver) radius *= 2;
            if (Zoom < 14) radius *= 0.7;

            //var photoCitclePen = new Pen(new SolidColorBrush(Color.FromArgb(255, 102, 102, 102)), 1.5);
            var photoCitclePen = new Pen(new SolidColorBrush(Colors.White), 2);
            for (var i = Math.Min(_photosCount, 5); i > 1; i--)
            {
                dc.DrawEllipse(new SolidColorBrush(Colors.Black) { Opacity = 0.3}, 
                               null,
                               new Point(0, (i - 1) * 3),
                               radius + 1.8, radius + 1.8);
                dc.DrawEllipse(Brushes.WhiteSmoke,
                               photoCitclePen,
                               new Point(0, (i - 1) * 3),
                               radius, radius);
            }

            dc.DrawEllipse(new SolidColorBrush(Colors.Black) { Opacity = 0.3 },
                           null,
                           new Point(),
                           radius + 1.8, radius + 1.8);
            dc.DrawEllipse(new ImageBrush(_thumbnail.Value) { Stretch = Stretch.UniformToFill },
                           photoCitclePen,
                           new Point(),
                           radius, radius);

            if (_photosCount > 1)
            {
                dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(255, 102, 163, 230)),
                               null,
                               new Point(radius * 0.8, -radius * 0.8),
                               10, 10);

                var ft = new FormattedText(_photosCount.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _typeface, 12,
                                           Brushes.White);
                dc.DrawText(ft, new Point(radius * 0.8 - ft.Width * 0.5, -radius * 0.8 - ft.Height * 0.5));
            }
        }

        public override void OnMouseEnter(MouseEventArgs MouseEventArgs)
        {
            base.OnMouseEnter(MouseEventArgs);
            RequestChangeVisual();
        }

        public override void OnMouseLeave(MouseEventArgs MouseEventArgs)
        {
            base.OnMouseLeave(MouseEventArgs);
            RequestChangeVisual();
        }
    }
}
