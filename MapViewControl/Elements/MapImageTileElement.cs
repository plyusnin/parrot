using System.Windows;
using System.Windows.Media;
using MapVisualization.TileLoaders;

namespace MapVisualization.Elements
{
    public class MapImageTileElement : MapTileElement
    {
        private readonly ITileLoadingContext _tileImage;

        public MapImageTileElement(ITileLoadingContext TileImage, int HorizontalIndex, int VerticalIndex, int Zoom)
            : base(HorizontalIndex, VerticalIndex, Zoom)
        {
            _tileImage = TileImage;
            _tileImage.Ready += (Sender, Args) => RequestChangeVisual();
        }

        public override void Dispose() { _tileImage.Abort(); }

        protected override void DrawTile(DrawingContext dc, Rect TileRect)
        {
            if (!_tileImage.IsReady)
                dc.DrawRectangle(Brushes.LemonChiffon, null, TileRect);
            else
                dc.DrawImage(_tileImage.Image, TileRect);
        }
    }
}
