using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Geographics;
using MapVisualization.Elements;
using Parrot.Viewer.GallerySources;

namespace Parrot.Viewer.ViewModels.Map
{
    public class PhotoMapElement : MapPointElement
    {
        private static readonly Typeface _typeface = new Typeface("Tahoma");
        private readonly IPhotoEntity _photo;
        private readonly int _photosCount;
        private readonly double _previewSize = 50;
        private readonly Lazy<ImageSource> _thumbnail;

        public PhotoMapElement(EarthPoint Position, IPhotoEntity Photo, int PhotosCount) : base(Position)
        {
            _photo       = Photo;
            _photosCount = PhotosCount;
            _thumbnail   = new Lazy<ImageSource>(LoadThumbnail);
        }

        private ImageSource LoadThumbnail()
        {
            var thumbnail = new BitmapImage();
            thumbnail.BeginInit();
            thumbnail.StreamSource = _photo.OpenThumbnail();
            thumbnail.CacheOption  = BitmapCacheOption.OnDemand;
            thumbnail.EndInit();
            return thumbnail;
        }

        protected override int ZIndex => 10;

        protected override void DrawPointElement(DrawingContext dc, int Zoom)
        {
            var radius = _previewSize / 2;

            if (IsMouseOver) radius *= 2;
            if (Zoom < 12) radius   /= 2;

            dc.DrawEllipse(new ImageBrush(_thumbnail.Value) { Stretch = Stretch.UniformToFill },
                           new Pen(Brushes.DarkSlateGray, 1.5),
                           new Point(),
                           radius, radius);

            if (_photosCount > 1)
            {
                dc.DrawEllipse(Brushes.DodgerBlue,
                               new Pen(Brushes.White, 0.5),
                               new Point(radius * 0.8, -radius * 0.8),
                               10, 10);

                var ft = new FormattedText(_photosCount.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _typeface, 12, Brushes.White);
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
