using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DynamicData;
using DynamicData.Binding;

namespace Parrot.Controls.TileView
{
    public class TilesList : Canvas
    {
        public static readonly DependencyProperty VisibleHeightProperty = DependencyProperty.Register(
            "VisibleHeight", typeof(double), typeof(TilesList), new PropertyMetadata(default(double), VisibleHeightChangedCallback));

        public static readonly DependencyProperty TilesSourceProperty = DependencyProperty.Register(
            "TilesSource", typeof(ITilesSource), typeof(TilesList),
            new FrameworkPropertyMetadata(default(ITilesSource),
                                          FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                                          OnTilesSourceChangedCallback));

        public static readonly DependencyProperty TileSizeProperty = DependencyProperty.Register(
            "TileSize", typeof(Size), typeof(TilesList),
            new FrameworkPropertyMetadata(new Size(100, 100),
                                          FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty TileSpaceProperty = DependencyProperty.Register(
            "TileSpace", typeof(double), typeof(TilesList),
            new FrameworkPropertyMetadata(15.0,
                                          FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty ScrollingOffsetProperty = DependencyProperty.Register(
            "ScrollingOffset", typeof(double), typeof(TilesList),
            new FrameworkPropertyMetadata(0.0,
                                          FrameworkPropertyMetadataOptions.AffectsRender,
                                          OnScrollingOffsetChanged));

        public static readonly DependencyProperty TileTemplateProperty = DependencyProperty.Register(
            "TileTemplate", typeof(DataTemplate), typeof(TilesList), new PropertyMetadata(default(DataTemplate)));

        private readonly Subject<double> _targetOffset = new Subject<double>();

        private readonly TileViewModelFactory _tileViewModelFactory = new TileViewModelFactory();

        private readonly SourceCache<ITileElement, int> _tileViewModels;

        private int _columns;

        private double _lastTargetOffset;

        private Bounds _loadedBounds;
        private int _rows;
        private readonly ReadOnlyObservableCollection<Tile> _tiles;

        private int _topmostRow;

        private CancellationTokenSource _updateCancellation;

        public TilesList()
        {
            Background = Brushes.Transparent;
            _tileViewModels = new SourceCache<ITileElement, int>(x => x.Index);



            _tileViewModels.Connect()
                           .ObserveOn(TaskPoolScheduler.Default)
                           .Transform(i => _tileViewModelFactory.CreateInstance(i))
                           .ObserveOnDispatcher()
                           .Transform(CreateTile)
                           .OnItemRemoved(RemoveTile)
                           .Bind(out _tiles)
                           .Subscribe();

            //ClipToBounds = true;

            //_targetOffset.Throttle(TimeSpan.FromMilliseconds(30))
            //             .ObserveOnDispatcher()
            //             .Subscribe(ScrollTo);
            //_targetOffset.ObserveOnDispatcher()
            //             .Subscribe(offset => ScrollingOffset = offset);
        }

        public DataTemplate TileTemplate
        {
            get => (DataTemplate)GetValue(TileTemplateProperty);
            set => SetValue(TileTemplateProperty, value);
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

        private ReadOnlyObservableCollection<Tile> Tiles => _tiles;

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
                var maxExistingIndex = _tileViewModels.Items.Select(t => t.Index)
                                                      .DefaultIfEmpty(-1)
                                                      .Max();
                var minExistingIndex = _tileViewModels.Items.Select(t => t.Index)
                                                      .DefaultIfEmpty(-1)
                                                      .Min();

                var tilesSource = TilesSource;
                Task.Run(() =>
                         {
                             var newTiles = new List<ITileElement>();
                             if (newBounds.Max > maxExistingIndex)
                             {
                                 var from = Math.Max(newBounds.Min, maxExistingIndex + 1);
                                 newTiles.AddRange(tilesSource.GetTiles(from, newBounds.Max - from));
                             }
                             if (newBounds.Min < minExistingIndex)
                             {
                                 var to = Math.Min(minExistingIndex, newBounds.Max);
                                 newTiles.AddRange(tilesSource.GetTiles(newBounds.Min, to - newBounds.Min));
                             }

                             if (!cancel.IsCancellationRequested)
                                 Dispatcher.BeginInvoke((Action<List<ITileElement>, Bounds, CancellationToken>)SubmitNew,
                                                        newTiles, newBounds, cancel);
                         }, cancel);
            }
        }

        private void SubmitNew(List<ITileElement> Items, Bounds Bounds, CancellationToken cancel)
        {
            if (!cancel.IsCancellationRequested)
            {
                _tileViewModels.Edit(list =>
                                     {
                                         list.Remove(list.Items.Where(t => t.Index < Bounds.Min || t.Index >= Bounds.Max).ToList());
                                         list.AddOrUpdate(Items);
                                     });
                _loadedBounds = Bounds;
            }
        }

        private static void OnTilesSourceChangedCallback(DependencyObject O, DependencyPropertyChangedEventArgs PropertyChangedEventArgs)
        {
            ((TilesList)O).OnTilesSourceChanged();
        }

        private void OnTilesSourceChanged()
        {
            _tileViewModels.Edit(
                list =>
                {
                    list.Clear();
                    list.AddOrUpdate(TilesSource.GetTiles(0, _rows * _columns));
                });

            Rearrange();
            UpdateGridContent();
        }

        private void RemoveTile(Tile Tile)
        {
            Children.Remove(Tile.Image);
        }

        private Tile CreateTile(TileViewModel ViewModel)
        {
            var placeholder = new ContentControl
            {
                Content = ViewModel,
                ContentTemplate = TileTemplate,
                Width = TileSize.Width,
                Height = TileSize.Height
            };

            Children.Add(placeholder);
            var tile = new Tile(ViewModel.Index, placeholder);
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

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var arrange = base.ArrangeOverride(arrangeSize);

            var rowHeight = TileSize.Height + TileSpace;
            var newColumns = (int)Math.Floor((arrangeSize.Width + TileSpace) / (TileSize.Width + TileSpace));

            if (newColumns != _columns)
            {
                var centerPhotoIndex = (int)Math.Round(_topmostRow * _columns + 0.5 * _rows * _columns + 0.5 * _columns);
                var offsetFromTheTopmostRow = ScrollingOffset - rowHeight * _topmostRow;

                _columns = newColumns;

                //SetCurrentValue(ScrollingOffsetProperty,
                //                Math.Max(
                //                    Math.Round((centerPhotoIndex - 0.5 * _rows * _columns - 0.5 * _columns) / _columns) * rowHeight + offsetFromTheTopmostRow,
                //                    0));
                _lastTargetOffset = ScrollingOffset;
                _targetOffset.OnNext(ScrollingOffset);

                Rearrange();
                UpdateGridContent();
            }

            return arrange;
        }

        private void Rearrange()
        {
            if (Tiles == null)
                return;

            foreach (var tile in Tiles) RefreshTilePosition(tile);
        }

        private void RefreshTilePosition(Tile tile)
        {
            var newPosition = new GridPosition(tile.Index % _columns,
                                               tile.Index / _columns);
            if (tile.Position != newPosition)
            {
                tile.Position =
                    new GridPosition(tile.Index % _columns,
                                     tile.Index / _columns);

                var left = tile.Position.X * (TileSize.Width + TileSpace);
                var top = tile.Position.Y * (TileSize.Height + TileSpace);

                var easing = new PowerEase { EasingMode = EasingMode.EaseOut };
                Duration dur = TimeSpan.FromMilliseconds(400);

                tile.Image.BeginAnimation(LeftProperty, new DoubleAnimation { To = left, EasingFunction = easing, Duration = dur });
                tile.Image.BeginAnimation(TopProperty, new DoubleAnimation { To = top, EasingFunction = easing, Duration = dur });

                SetLeft(tile.Image, tile.Position.X * (TileSize.Width + TileSpace));
                SetTop(tile.Image, tile.Position.Y * (TileSize.Height + TileSpace));
            }
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
        int Count { get; }
        IList<ITileElement> GetTiles(int StartIndex, int Count);
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

        protected bool Equals(GridPosition other)
        {
            return X == other.X
                   && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GridPosition)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public static bool operator ==(GridPosition left, GridPosition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GridPosition left, GridPosition right)
        {
            return !Equals(left, right);
        }
    }
}
