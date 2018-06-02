using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Parrot.Controls.TileView.Visuals
{
    public abstract class TileHostVisual : DrawingVisual
    {
        protected TileHostVisual(int ZIndex) { this.ZIndex = ZIndex; }

        public int ZIndex { get; }

        protected abstract void DrawElement(DrawingContext Context);

        public void Draw()
        {
            using (var context = RenderOpen())
            {
                DrawElement(context);
            }
        }
    }

    public class PictureVisual : TileHostVisual
    {
        private readonly int _index;
        private readonly Size _size;
        private Point _imageOrigin;
        private Size _imageSize;
        private readonly BitmapImage _imageSource;
        private bool _ready;

        public PictureVisual(Stream ThumbnailStream, Size Size, int Index) : base(1)
        {
            _size = Size;
            _index = Index;
            CacheMode = new BitmapCache();

            _imageSource = new BitmapImage();
            _imageSource.BeginInit();
            _imageSource.CacheOption = BitmapCacheOption.OnLoad;
            _imageSource.DownloadCompleted += BmpOnDownloadCompleted;
            _imageSource.DownloadCompleted += (s, e) => { ThumbnailStream.Dispose(); };
            _imageSource.StreamSource = ThumbnailStream;
            _imageSource.EndInit();

            //_imageSource = (BitmapFrame)ThumbnailStream;
            //_imageSource.DownloadCompleted += BmpOnDownloadCompleted;
        }

        private void BmpOnDownloadCompleted(object Sender, EventArgs Args)
        {
            var scale = Math.Max(_size.Width / _imageSource.Width, _size.Height / _imageSource.Height);
            _imageSize = new Size(_imageSource.Width * scale, _imageSource.Height * scale);
            _imageOrigin = new Point(0.5 * (_size.Width - _imageSize.Width),
                                     0.5 * (_size.Height - _imageSize.Height));
            _ready = true;
            Draw();
        }

        protected override void DrawElement(DrawingContext Context)
        {
            if (!_ready)
            {
                Context.DrawRectangle(Brushes.BurlyWood, null, new Rect(_size));
            }
            else
            {
                Context.PushClip(new RectangleGeometry(new Rect(_size)));
                Context.DrawImage(_imageSource, new Rect(_imageOrigin, _imageSize));
                Context.Pop();
            }
            //Context.DrawRectangle(Brushes.BurlyWood, null, new Rect(_size));
            Context.DrawText(new FormattedText((_index + 1).ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                               new Typeface(new FontFamily(), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal), 30, Brushes.Coral),
                             new Point(5, 5));
        }
    }

    public class ShadowVisual : TileHostVisual
    {
        private readonly Size _size;

        public ShadowVisual(Size Size) : base(-1)
        {
            _size = Size;
            Effect = new BlurEffect { Radius = 22 };
            CacheMode = new BitmapCache();
        }

        protected override void DrawElement(DrawingContext Context)
        {
            Context.PushOpacity(0.3);
            Context.DrawRectangle(Brushes.Black, null, new Rect(new Point(3, 3), _size));
            Context.Pop();
        }
    }
}
