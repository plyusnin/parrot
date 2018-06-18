﻿using System;
using System.Globalization;
using System.IO;
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
        private readonly int _photosCount;
        private readonly Stream _thumbnailStream;
        private readonly double _previewSize = 50;
        private readonly Lazy<ImageSource> _thumbnail;

        public PhotoMapElement(EarthPoint Position, int PhotosCount, Stream ThumbnailStream) : base(Position)
        {
            _photosCount = PhotosCount;
            _thumbnailStream = ThumbnailStream;
            _thumbnail   = new Lazy<ImageSource>(LoadThumbnail);
        }

        protected override int ZIndex => 10;

        private ImageSource LoadThumbnail()
        {
            var thumbnail = new BitmapImage();
            thumbnail.BeginInit();
            thumbnail.StreamSource = _thumbnailStream;
            thumbnail.CacheOption  = BitmapCacheOption.OnDemand;
            thumbnail.EndInit();
            return thumbnail;
        }

        protected override void DrawPointElement(DrawingContext dc, int Zoom)
        {
            var radius = _previewSize / 2;

            if (IsMouseOver) radius *= 2;
            if (Zoom < 14) radius   *= 0.7;

            dc.DrawEllipse(new ImageBrush(_thumbnail.Value) { Stretch = Stretch.UniformToFill },
                           new Pen(new SolidColorBrush(Color.FromArgb(255, 102, 102, 102)), 1.5),
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
