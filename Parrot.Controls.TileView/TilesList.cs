using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
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

        private double _lastTargetOffset;

        private Bounds _loadedBounds;
        private int _rows;
        private readonly Subject<double> _targetOffset = new Subject<double>();

        private int _topmostRow;

        public TilesList()
        {
            Background = Brushes.Transparent;
            _visuals = new VisualCollection(this);
            _tileViewModels = new ReactiveList<ITileViewModel>();
            Tiles = _tileViewModels.CreateDerivedCollection(CreateTiles, RemoveTilesPack);
            ClipToBounds = true;

            _targetOffset.Throttle(TimeSpan.FromMilliseconds(30))
                         .ObserveOnDispatcher()
                         .Subscribe(ScrollTo);
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

        private void ScrollTo(double Offset)
        {
            var duration = TimeSpan.FromMilliseconds(1.5 * Math.Abs(Offset - (double)GetValue(ScrollingOffsetProperty)));
            BeginAnimation(ScrollingOffsetProperty,
                           new DoubleAnimation(Offset,
                                               new Duration(duration))
                           {
                               EasingFunction = new PowerEase { EasingMode = EasingMode.EaseOut, Power = 3.0 }
                           });
        }

        private static void OnScrollingOffsetChanged(DependencyObject O, DependencyPropertyChangedEventArgs PropertyChangedEventArgs)
        {
            var c = (TilesList)O;
            c.ScrollChanged();
        }

        private void ScrollChanged()
        {
            GlobalTransform.Y = -ScrollingOffset;
            UpdateGridContent();
        }

        private void UpdateGridContent()
        {
            var newTopmostRow = (int)Math.Floor(ScrollingOffset / (TileSize.Height + TileSpace));
            _topmostRow = newTopmostRow;

            if (TilesSource == null)
                return;

            var newBounds = new Bounds { Min = _topmostRow * _columns, Max = (_topmostRow + _rows) * _columns };
            if (!_loadedBounds.Equals(newBounds))
            {
                _tileViewModels.RemoveAll(_tileViewModels.Where(t => t.Index < newBounds.Min || t.Index >= newBounds.Max).ToList());

                var maxExistingIndex = Math.Max(_tileViewModels.Select(t => t.Index).DefaultIfEmpty().Max(), newBounds.Min - 1);
                var minExistingIndex = Math.Min(_tileViewModels.Select(t => t.Index).DefaultIfEmpty().Min(), newBounds.Max + 1);

                if (maxExistingIndex < newBounds.Max)
                    _tileViewModels.AddRange(TilesSource.GetTiles(maxExistingIndex + 1, newBounds.Max - maxExistingIndex - 1));
                if (minExistingIndex > newBounds.Min)
                    _tileViewModels.AddRange(TilesSource.GetTiles(newBounds.Min, minExistingIndex - newBounds.Min));

                _loadedBounds = newBounds;
                Console.WriteLine($"New bounds: {_loadedBounds}");
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
            var pack = new TilesPack(this, ViewModel.Index, ViewModel.ThumbnailStream);
            RefreshTilePosition(pack);
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
            _columns = (int)Math.Floor((ActualWidth + TileSpace) / (TileSize.Width + TileSpace));
            _rows = (int)Math.Ceiling((ActualHeight + TileSpace) / (TileSize.Height + TileSpace)) + 1;
            GlobalTransform.X = 0.5 * (ActualWidth - TileSpace * (_columns - 1) - TileSize.Width * _columns);
            Rearrange();
            UpdateGridContent();
        }

        private void Rearrange()
        {
            if (Tiles == null)
                return;

            foreach (var tile in Tiles) RefreshTilePosition(tile);
        }

        private void RefreshTilePosition(TilesPack tile)
        {
            tile.Position =
                new GridPosition(tile.Index % _columns,
                                 tile.Index / _columns);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            _targetOffset.OnNext(_lastTargetOffset = Math.Max(0, _lastTargetOffset - e.Delta * 0.4));
            //ScrollingOffset = _targetOffset;
            base.OnMouseWheel(e);
        }

        private struct Bounds
        {
            public int Min;
            public int Max;

            public override string ToString()
            {
                return $"[{Min} => {Max}]";
            }
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

        public TilesPack(TilesList Parent, int Index, Stream ThumbnailStream)
        {
            _parent = Parent;
            this.Index = Index;

            var transform = new TransformGroup();
            transform.Children.Add(_gridTransform);
            transform.Children.Add(_parent.GlobalTransform);

            PictureVisual = new PictureVisual(ThumbnailStream, _parent.TileSize, Index) { Transform = transform };
            ShadowVisual = new ShadowVisual(_parent.TileSize) { Transform = transform };
        }

        public PictureVisual PictureVisual { get; }
        public ShadowVisual ShadowVisual { get; }
        public int Index { get; }

        public GridPosition Position
        {
            get => _position;
            set
            {
                _position = value;
                _gridTransform.X = (_parent.TileSize.Width + _parent.TileSpace) * _position.X;
                _gridTransform.Y = (_parent.TileSize.Height + _parent.TileSpace) * _position.Y;
            }
        }

        public IEnumerable<TileHostVisual> EnumerateVisuals()
        {
            yield return PictureVisual;
            //yield return ShadowVisual;
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
