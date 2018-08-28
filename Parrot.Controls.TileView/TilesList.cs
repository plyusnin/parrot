using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Parrot.Controls.TileView.Visuals;
using ReactiveUI;

namespace Parrot.Controls.TileView
{
    public class TilesList : Canvas
    {
        public static readonly DependencyProperty VisibleHeightProperty = DependencyProperty.Register(
            "VisibleHeight", typeof(double), typeof(TilesList), new PropertyMetadata(default(double), VisibleHeightChangedCallback));

        public static readonly DependencyProperty TilesSourceProperty = DependencyProperty.Register(
            "TilesSource", typeof(ITilesSource), typeof(TilesList),
            new PropertyMetadata(default(ITilesSource), OnTilesSourceChangedCallback));

        public static readonly DependencyProperty TileSizeProperty = DependencyProperty.Register(
            "TileSize", typeof(Size), typeof(TilesList), new PropertyMetadata(new Size(100, 100)));

        public static readonly DependencyProperty TileSpaceProperty = DependencyProperty.Register(
            "TileSpace", typeof(double), typeof(TilesList), new PropertyMetadata(15.0));

        public static readonly DependencyProperty ScrollingOffsetProperty = DependencyProperty.Register(
            "ScrollingOffset", typeof(double), typeof(TilesList), new PropertyMetadata(0.0, OnScrollingOffsetChanged));

        private readonly Subject<double> _targetOffset = new Subject<double>();

        private readonly ReactiveList<ITileViewModel> _tileViewModels;

        internal readonly TranslateTransform GlobalTransform = new TranslateTransform();
        private int _columns;

        private double _lastTargetOffset;

        private Bounds _loadedBounds;
        private int _rows;

        private int _topmostRow;

        private CancellationTokenSource _updateCancellation;

        public TilesList()
        {
            Background = Brushes.Transparent;
            _tileViewModels = new ReactiveList<ITileViewModel>();
            Tiles = _tileViewModels.CreateDerivedCollection(CreateTile, RemoveTilesPack, scheduler: DispatcherScheduler.Current);
            //ClipToBounds = true;

            //_targetOffset.Throttle(TimeSpan.FromMilliseconds(30))
            //             .ObserveOnDispatcher()
            //             .Subscribe(ScrollTo);
            //_targetOffset.ObserveOnDispatcher()
            //             .Subscribe(offset => ScrollingOffset = offset);
        }

        public double VisibleHeight
        {
            get => (double)GetValue(VisibleHeightProperty);
            set => SetValue(VisibleHeightProperty, value);
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

        private IReactiveDerivedList<Tile> Tiles { get; }

        private static void VisibleHeightChangedCallback(DependencyObject O, DependencyPropertyChangedEventArgs PropertyChangedEventArgs)
        {
            ((TilesList)O).OnVisibleHeightChanged((double)PropertyChangedEventArgs.NewValue);
        }

        private void OnVisibleHeightChanged(double NewHeight)
        {
            var rowHeight = TileSize.Height + TileSpace;
            var newRows = (int)Math.Ceiling((NewHeight + TileSpace) / rowHeight) + 1;

            if (newRows != _rows)
            {
                _rows = newRows;
                UpdateGridContent();
            }
        }

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
            UpdateGridContent();
        }

        private void UpdateGridContent()
        {
            var newTopmostRow = (int)Math.Floor(ScrollingOffset / (TileSize.Height + TileSpace));
            _topmostRow = newTopmostRow;

            if (TilesSource == null)
                return;

            _updateCancellation?.Cancel();
            _updateCancellation = new CancellationTokenSource();
            var cancel = _updateCancellation.Token;

            var newBounds = new Bounds { Min = _topmostRow * _columns, Max = (_topmostRow + _rows) * _columns };
            if (!_loadedBounds.Equals(newBounds))
            {
                var maxExistingIndex = _tileViewModels.Select(t => t.Index)
                                                      .DefaultIfEmpty(-1)
                                                      .Max();
                var minExistingIndex = _tileViewModels.Select(t => t.Index)
                                                      .DefaultIfEmpty(-1)
                                                      .Min();

                var tilesSource = TilesSource;
                Task.Run(() =>
                         {
                             var newTiles = new List<ITileViewModel>();
                             if (newBounds.Max > maxExistingIndex)
                             {
                                 var from = Math.Max(newBounds.Min, maxExistingIndex + 1);
                                 newTiles.AddRange(tilesSource.GetTiles(from, newBounds.Max - from - 1));
                             }
                             if (newBounds.Min < minExistingIndex)
                             {
                                 var to = Math.Min(minExistingIndex, newBounds.Max);
                                 newTiles.AddRange(tilesSource.GetTiles(newBounds.Min, to - newBounds.Min));
                             }

                             if (!cancel.IsCancellationRequested)
                                 Dispatcher.BeginInvoke((Action<List<ITileViewModel>, Bounds, CancellationToken>)SubmitNew,
                                                        newTiles, newBounds, cancel);
                         }, cancel);

            }
        }

        private void SubmitNew(List<ITileViewModel> Items, Bounds Bounds, CancellationToken cancel)
        {
            if (!cancel.IsCancellationRequested)
            {
                var tilessss = Items;

                _tileViewModels.RemoveAll(_tileViewModels.Where(t => t.Index < Bounds.Min || t.Index >= Bounds.Max).ToList());
                _tileViewModels.AddRange(tilessss);

                Console.WriteLine($"Bounds: {_loadedBounds} -> {Bounds}    ({tilessss.Count} added)");
                _loadedBounds = Bounds;
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

        private void RemoveTilesPack(Tile Tile)
        {
            Children.Remove(Tile.Image);
        }

        private Tile CreateTile(ITileViewModel ViewModel)
        {
            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = ViewModel.ThumbnailStream;
            imageSource.EndInit();

            var image = new Image
            {
                Source = imageSource,
                Width = TileSize.Width,
                Height = TileSize.Height
            };

            //var image = new Grid
            //{
            //    Background = Brushes.Brown,
            //    Width = TileSize.Width,
            //    Height = TileSize.Height
            //};
            //image.Children.Add(new TextBlock
            //{
            //    Text = ViewModel.Index.ToString(),
            //    VerticalAlignment = VerticalAlignment.Center,
            //    HorizontalAlignment = HorizontalAlignment.Center,
            //    Foreground = Brushes.White,
            //    FontSize = 16
            //});

            Children.Add(image);
            var tile = new Tile(ViewModel.Index, image);
            RefreshTilePosition(tile);
            return tile;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var b = base.MeasureOverride(constraint);
            if (TilesSource == null)
                return new Size();

            var cols = (int)Math.Floor((constraint.Width + TileSpace) / (TileSize.Width + TileSpace));
            var rows = TilesSource.Count / cols;
            var res = new Size((TileSize.Width + TileSpace) * cols - TileSpace,
                               (TileSize.Height + TileSpace) * rows - TileSpace);
            return res;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            var rowHeight = TileSize.Height + TileSpace;

            var newColumns = (int)Math.Floor((ActualWidth + TileSpace) / (TileSize.Width + TileSpace));

            if (newColumns != _columns)
            {
                var centerPhotoIndex = (int)Math.Round(_topmostRow * _columns + 0.5 * _rows * _columns + 0.5 * _columns);
                var offsetFromTheTopmostRow = ScrollingOffset - rowHeight * _topmostRow;

                _columns = newColumns;

                SetCurrentValue(ScrollingOffsetProperty,
                                Math.Max(
                                    Math.Round((centerPhotoIndex - 0.5 * _rows * _columns - 0.5 * _columns) / _columns) * rowHeight + offsetFromTheTopmostRow,
                                    0));
                _lastTargetOffset = ScrollingOffset;
                _targetOffset.OnNext(ScrollingOffset);

                Rearrange();
                UpdateGridContent();
            }

            GlobalTransform.X = 0.5 * (ActualWidth - TileSpace * (_columns - 1) - TileSize.Width * _columns);
        }

        private void Rearrange()
        {
            if (Tiles == null)
                return;

            foreach (var tile in Tiles) RefreshTilePosition(tile);
        }

        private void RefreshTilePosition(Tile tile)
        {
            tile.Position =
                new GridPosition(tile.Index % _columns,
                                 tile.Index / _columns);
            SetLeft(tile.Image, tile.Position.X * (TileSize.Width + TileSpace));
            SetTop(tile.Image, tile.Position.Y * (TileSize.Height + TileSpace));
        }

        //protected override void OnMouseWheel(MouseWheelEventArgs e)
        //{
        //    _targetOffset.OnNext(_lastTargetOffset = Math.Max(0, _lastTargetOffset - e.Delta * 0.4));
        //    //ScrollingOffset = _targetOffset;
        //    base.OnMouseWheel(e);
        //}

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
        int Count { get; }
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

    internal class Tile
    {
        public Tile(int Index, FrameworkElement Image)
        {
            this.Index = Index;
            this.Image = Image;
        }

        public int Index { get; }
        public FrameworkElement Image { get; }
        public GridPosition Position { get; set; }
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
