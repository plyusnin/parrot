using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Parrot.Controls.TileView.Visuals
{
    public abstract class TileHostVisual : DrawingVisual
    {
        protected TileHostVisual(int ZIndex)
        {
            this.ZIndex = ZIndex;
        }

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
        private Point _imageOrigin;
        private Size _imageSize;
        private readonly BitmapImage _imageSource;
        private readonly Size _size;

        public PictureVisual(ImageSource ImageSource, Size Size) : base(1)
        {
            _size        = Size;


            _imageSource = (BitmapImage)ImageSource;
            _imageSource.DownloadCompleted += BmpOnDownloadCompleted;
        }

        private void BmpOnDownloadCompleted(object Sender, EventArgs Args)
        {
            var scale = Math.Max(_size.Width / _imageSource.Width, _size.Height / _imageSource.Height);
            _imageSize = new Size(_imageSource.Width * scale, _imageSource.Height * scale);
            _imageOrigin = new Point(0.5 * (_size.Width - _imageSize.Width),
                                     0.5 * (_size.Height - _imageSize.Height));
            
            Draw();
        }

        protected override void DrawElement(DrawingContext Context)
        {
            if (_imageSource.IsDownloading)
            {
                Context.DrawRectangle(Brushes.BurlyWood, null, new Rect(_size));
            }
            else
            {
                Context.PushClip(new RectangleGeometry(new Rect(_size)));
                Context.DrawImage(_imageSource, new Rect(_imageOrigin, _imageSize));
                Context.Pop();
            }
        }
    }

    public class ShadowVisual : TileHostVisual
    {
        private readonly Size _size;

        public ShadowVisual(Size Size) : base(-1)
        {
            _size     = Size;
            Effect    = new BlurEffect { Radius = 22 };
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
