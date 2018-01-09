using System;
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
        private readonly IPhotoEntity _photo;
        private readonly double _previewSize = 50;
        private readonly Lazy<ImageSource> _thumbnail;

        public PhotoMapElement(IPhotoEntity Photo) : base((EarthPoint)Photo.Exif.Gps)
        {
            _photo     = Photo;
            _thumbnail = new Lazy<ImageSource>(LoadThumbnail);
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

        protected override void DrawPointElement(DrawingContext dc, int Zoom)
        {
            var radius = _previewSize / 2;

            if (IsMouseOver) radius *= 2;
            if (Zoom < 12) radius   /= 2;

            dc.DrawEllipse(new ImageBrush(_thumbnail.Value),
                           new Pen(Brushes.DarkSlateGray, 1.5),
                           new Point(),
                           radius, radius);
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
