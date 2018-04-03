using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Parrot.Controls.TileView.Visuals;
using ReactiveUI;

namespace Parrot.Controls.TileView
{
    public class TilesList : Panel
    {
        public static readonly DependencyProperty TilesSourceProperty = DependencyProperty.Register(
            "TilesSource", typeof(ITilesSource), typeof(TilesList),
            new PropertyMetadata(default(ITilesSource), OnTilesSourceChangedCallback));

        public static readonly DependencyProperty TileSizeProperty = DependencyProperty.Register(
            "TileSize", typeof(Size), typeof(TilesList), new PropertyMetadata(new Size(100, 100)));

        public static readonly DependencyProperty TileSpaceProperty = DependencyProperty.Register(
            "TileSpace", typeof(double), typeof(TilesList), new PropertyMetadata(15.0));

        public static readonly DependencyProperty ScrollingOffsetProperty = DependencyProperty.Register(
            "ScrollingOffset", typeof(double), typeof(TilesList), new PropertyMetadata(0.0, OnScrollingOffsetChanged));

        private readonly ReactiveList<ITileViewModel> _tileViewModels;

        private readonly VisualCollection _visuals;

        internal readonly TranslateTransform GlobalTransform = new TranslateTransform();
        private int _columns;
        private int _rows;
        private int _topmostRow;

        public TilesList()
        {
            Background      = Brushes.Transparent;
            _visuals        = new VisualCollection(this);
            _tileViewModels = new ReactiveList<ITileViewModel>();
            Tiles           = _tileViewModels.CreateDerivedCollection(CreateTiles, RemoveTilesPack);
        }

        public double ScrollingOffset
        {
            get => (double)GetValue(ScrollingOffsetProperty);
            set => SetValue(ScrollingOffsetProperty, value);
        }

        public double TileSpace
        {
            get => (double)GetValue(TileSpaceProperty);
            set => SetValue(TileSpaceProperty, value);
        }

        public Size TileSize
        {
            get => (Size)GetValue(TileSizeProperty);
            set => SetValue(TileSizeProperty, value);
        }

        public ITilesSource TilesSource
        {
            get => (ITilesSource)GetValue(TilesSourceProperty);
            set => SetValue(TilesSourceProperty, value);
        }

        protected override int VisualChildrenCount => _visuals.Count;

        private IReactiveDerivedList<TilesPack> Tiles { get; }

        private static void OnScrollingOffsetChanged(DependencyObject O, DependencyPropertyChangedEventArgs PropertyChangedEventArgs)
        {
            var c = (TilesList)O;
            c.ScrollChanged();
        }

        private void ScrollChanged()
        {
            GlobalTransform.Y = -ScrollingOffset;

            var newTopmostRow = (int)Math.Floor(ScrollingOffset / (TileSize.Height + TileSpace));
            if (newTopmostRow != _topmostRow)
            {
                _topmostRow = newTopmostRow;

                var minIndex = _topmostRow * _columns;
                var maxIndex = (_topmostRow + _rows) * _columns;

                _tileViewModels.RemoveAll(_tileViewModels.Where(t => t.Index <= minIndex || t.Index >= maxIndex).ToList());

                var maxExistingIndex = _tileViewModels.Select(t => t.Index).DefaultIfEmpty().Max();
                var minExistingIndex = _tileViewModels.Select(t => t.Index).DefaultIfEmpty().Min();

                if (maxExistingIndex != maxIndex)
                    _tileViewModels.AddRange(TilesSource.GetTiles(maxExistingIndex + 1, maxIndex - maxExistingIndex - 1));
                if (minExistingIndex != minIndex)
                    _tileViewModels.AddRange(TilesSource.GetTiles(minIndex, minExistingIndex - minIndex));
            }
        }

        private static void OnTilesSourceChangedCallback(DependencyObject O, DependencyPropertyChangedEventArgs PropertyChangedEventArgs)
        {
            ((TilesList)O).OnTilesSourceChanged();
        }

        private void OnTilesSourceChanged()
        {
            _tileViewModels.Clear();
            _tileViewModels.AddRange(TilesSource.GetTiles(0, _rows * _columns));
            Rearrange();
        }

        private void RemoveTilesPack(TilesPack Tiles)
        {
            foreach (var visual in Tiles.EnumerateVisuals()) DeleteVisual(visual);
        }

        private TilesPack CreateTiles(ITileViewModel ViewModel)
        {
            var pack = new TilesPack(this, ViewModel.Index, ViewModel.ImageSource);
            RefreshPosition(pack);
            foreach (var visual in pack.EnumerateVisuals()) AddVisual(visual);
            return pack;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _visuals[index];
        }

        protected void AddVisual(TileHostVisual v)
        {
            int index;
            for (index = _visuals.Count; index > 0; index--)
                if (((TileHostVisual)_visuals[index - 1]).ZIndex <= v.ZIndex)
                    break;

            _visuals.Insert(index, v);
            v.Draw();
        }

        protected void DeleteVisual(TileHostVisual v)
        {
            _visuals.Remove(v);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            _columns          = (int)Math.Floor((ActualWidth + TileSpace) / (TileSize.Width + TileSpace));
            _rows             = (int)Math.Ceiling((ActualHeight + TileSpace) / (TileSize.Height + TileSpace)) + 1;
            GlobalTransform.X = 0.5 * (ActualWidth - TileSpace * (_columns - 1) - TileSize.Width * _columns);
            Rearrange();
        }

        private void Rearrange()
        {
            if (Tiles == null)
                return;

            foreach (var tile in Tiles) RefreshPosition(tile);
        }

        private void RefreshPosition(TilesPack tile)
        {
            tile.Position =
                new GridPosition(tile.Index % _columns,
                                 tile.Index / _columns);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            ScrollingOffset = Math.Max(0, ScrollingOffset - e.Delta * 0.2);
            base.OnMouseWheel(e);
        }
    }

    public interface ITilesSource
    {
        IList<ITileViewModel> GetTiles(int StartIndex, int Count);
    }

    internal class TilesPack
    {
        private readonly TranslateTransform _gridTransform = new TranslateTransform();
        private readonly TilesList _parent;
        private GridPosition _position;

        public TilesPack(TilesList Parent, int Index, ImageSource Image)
        {
            _parent    = Parent;
            this.Index = Index;

            var transform = new TransformGroup();
            transform.Children.Add(_gridTransform);
            transform.Children.Add(_parent.GlobalTransform);

            PictureVisual = new PictureVisual(Image, _parent.TileSize) { Transform = transform };
            ShadowVisual  = new ShadowVisual(_parent.TileSize) { Transform         = transform };
        }

        public PictureVisual PictureVisual { get; }
        public ShadowVisual  ShadowVisual  { get; }
        public int           Index         { get; }

        public GridPosition Position
        {
            get => _position;
            set
            {
                _position        = value;
                _gridTransform.X = (_parent.TileSize.Width + _parent.TileSpace) * _position.X;
                _gridTransform.Y = (_parent.TileSize.Height + _parent.TileSpace) * _position.Y;
            }
        }

        public IEnumerable<TileHostVisual> EnumerateVisuals()
        {
            yield return PictureVisual;
            yield return ShadowVisual;
        }
    }

    internal class GridPosition
    {
        public GridPosition(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public int X { get; }
        public int Y { get; }

        public override string ToString()
        {
            return $"{X}; {Y}";
        }
    }
}
