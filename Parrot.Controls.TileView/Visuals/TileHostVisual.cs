using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

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
        private readonly Size _imageSize;
        private readonly ImageSource _imageSource;
        private readonly Size _size;
        private readonly Point _imageOrigin;

        public PictureVisual(ImageSource ImageSource, Size Size) : base(1)
        {
            _imageSource = ImageSource;
            _size        = Size;

            var scale = Math.Max(_size.Width / ImageSource.Width, _size.Height / ImageSource.Height);
            _imageSize = new Size(ImageSource.Width * scale, ImageSource.Height * scale);
            _imageOrigin = new Point(0.5 * (_size.Width - _imageSize.Width),
                                     0.5 * (_size.Height - _imageSize.Height));
        }

        protected override void DrawElement(DrawingContext Context)
        {
            Context.PushClip(new RectangleGeometry(new Rect(_size)));
            Context.DrawImage(_imageSource, new Rect(_imageOrigin, _imageSize));
            Context.Pop();
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
