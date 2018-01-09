using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Geographics;
using MapVisualization.Elements;
using MapVisualization.TileLoaders;
using MapVisualization.TileLoaders.TilePathProvider;

namespace MapVisualization
{
    public class MapView : MapVisualHost
    {
        public static readonly DependencyProperty TileLoaderProperty =
            DependencyProperty.Register("TileLoader",
                                        typeof (ITileLoader),
                                        typeof (MapView),
                                        new PropertyMetadata(new WebTileLoader(OsmTilePathProviders.Retina),
                                            TileLoaderPropertyChangedCallback));

        public static readonly DependencyProperty ElementsSourceProperty =
            DependencyProperty.Register("ElementsSource",
                                        typeof (IEnumerable<MapElement>),
                                        typeof (MapView),
                                        new PropertyMetadata(Enumerable.Empty<MapElement>(), ElementsSourcePropertyChangedCallback));

        static MapView() { DefaultStyleKeyProperty.OverrideMetadata(typeof (MapView), new FrameworkPropertyMetadata(typeof (MapView))); }

        public MapView()
        {
            Projector = ScreenProjector.DefaultProjector;
            Point topLeftScreenCoordinate = ScreenProjector.DefaultProjector.Project(CentralPoint, ZoomLevel);
            _globalTransform = new TranslateTransform(-topLeftScreenCoordinate.X, -topLeftScreenCoordinate.Y);
        }

        public IEnumerable<MapElement> ElementsSource
        {
            get { return (IEnumerable<MapElement>)GetValue(ElementsSourceProperty); }
            set { SetValue(ElementsSourceProperty, value); }
        }

        public ScreenProjector Projector { get; private set; }

        public ITileLoader TileLoader
        {
            get { return (ITileLoader)GetValue(TileLoaderProperty); }
            set { SetValue(TileLoaderProperty, value); }
        }

        private static void TileLoaderPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (MapView)d;
            sender._tiles.ToList().ForEach(sender.RemoveTile);
            sender.RefreshTiles();
        }

        private static void ElementsSourcePropertyChangedCallback(DependencyObject target,
                                                                  DependencyPropertyChangedEventArgs e)
        {
            var map = (MapView)target;
            var newEnumerable = (IEnumerable<MapElement>)e.NewValue;

            foreach (MapElement mapElement in map._elements)
                map.RemoveElement(mapElement);

            foreach (MapElement mapElement in newEnumerable)
                map.AddElement(mapElement);

            var notifyCollection = newEnumerable as INotifyCollectionChanged;
            if (notifyCollection != null) notifyCollection.CollectionChanged += map.ElementsSourceOnCollectionChanged;
        }

        private void ElementsSourceOnCollectionChanged(object Sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (MapElement element in e.NewItems.OfType<MapElement>())
                    AddElement(element);
            }

            if (e.OldItems != null)
            {
                foreach (MapElement element in e.OldItems.OfType<MapElement>())
                    RemoveElement(element);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            var delta = new Vector(sizeInfo.NewSize.Width - sizeInfo.PreviousSize.Width,
                                   sizeInfo.NewSize.Height - sizeInfo.PreviousSize.Height);
            Point oldScreenCentralPoint = Projector.Project(CentralPoint, ZoomLevel);
            Point newScreenCentralPoint = oldScreenCentralPoint + 0.5 * delta;

            CentralPoint = Projector.InverseProject(newScreenCentralPoint, ZoomLevel);
        }

        private void RefreshTiles()
        {
            int x0 = OsmIndexes.GetHorizontalIndex(VisibleArea.MostWesternLongitude, ZoomLevel);
            int y0 = OsmIndexes.GetVerticalIndex(VisibleArea.MostNorthenLatitude, ZoomLevel);

            int w = (int)Math.Ceiling(ActualWidth / 256) + 1;
            int h = (int)Math.Ceiling(ActualHeight / 256) + 1;

            for (int x = x0; x < x0 + w; x++)
            {
                for (int y = y0; y < y0 + h; y++)
                {
                    if (!_tiles.Any(t => t.HorizontalIndex == x && t.VerticalIndex == y))
                    {
                        ITileLoadingContext tileContext = TileLoader.GetTile(x, y, ZoomLevel);
                        var tile = new MapImageTileElement(tileContext, x, y, ZoomLevel);
                        AddTile(tile);
                    }
                }
            }
            _tiles
                .Where(t => t.HorizontalIndex < x0 || t.HorizontalIndex > x0 + w ||
                            t.VerticalIndex < y0 || t.VerticalIndex > y0 + h)
                .ToList()
                .ForEach(RemoveTile);
        }

        private void RefreshObjectsVisuals()
        {
            EarthArea vArea = VisibleArea;
            foreach (var element in _elements)
            {
                bool visibility = element.TestVisual(vArea);
                CheckVisual(element, visibility);
            }
        }

        /// <summary>Получает координаты точки, соответствующей точке с заданными экранными координатами</summary>
        /// <param name="screenPoint">Координаты точки относительно элемента управления карты</param>
        /// <returns>Координаты точки на поверхности Земли, соответствующие точке на карте</returns>
        public EarthPoint PointAt(Point screenPoint)
        {
            Point globalScreenCenter = Projector.Project(CentralPoint, ZoomLevel);
            Point globalScreenPoint = globalScreenCenter + (Vector)screenPoint
                                      - new Vector(ActualWidth / 2, ActualHeight / 2);
            return Projector.InverseProject(globalScreenPoint, ZoomLevel);
        }

        #region Работа со списком элементов

        private readonly List<MapElement> _elements = new List<MapElement>();

        private readonly List<MapTileElement> _tiles = new List<MapTileElement>();

        private readonly Dictionary<MapTileElement, MapVisual> _tilesToVisuals =
            new Dictionary<MapTileElement, MapVisual>();

        public void AddTile(MapTileElement Tile)
        {
            _tiles.Add(Tile);
            MapVisual visual = Tile.GetVisual(ZoomLevel);
            visual.Transform = _globalTransform;
            AddVisual(visual);
            _tilesToVisuals.Add(Tile, visual);
            Tile.ChangeVisualRequested += TileChangeRequested;
        }

        private void RemoveTile(MapTileElement Tile)
        {
            _tiles.Remove(Tile);
            if (_tilesToVisuals.ContainsKey(Tile))
            {
                var visual = _tilesToVisuals[Tile];
                visual.Transform = null;
                DeleteVisual(visual);
                _tilesToVisuals.Remove(Tile);
            }
            Tile.ChangeVisualRequested -= TileChangeRequested;
            Tile.Dispose();
        }

        private void TileChangeRequested(object Sender, EventArgs E)
        {
            Dispatcher.BeginInvoke((Action<MapTileElement>)
                                   (tile =>
                                    {
                                        MapVisual oldVisual;
                                        if (_tilesToVisuals.TryGetValue(tile, out oldVisual))
                                        {
                                            DeleteVisual(oldVisual);
                                            _tilesToVisuals.Remove(tile);
                                            MapVisual newVisual = tile.GetVisual(ZoomLevel);
                                            newVisual.Transform = _globalTransform;
                                            AddVisual(newVisual);
                                            _tilesToVisuals.Add(tile, newVisual);
                                        }
                                    }), Sender);

            //var tile = (MapTileElement)Sender;
        }

        protected void AddElement(MapElement Element)
        {
            _elements.Add(Element);
            CheckVisual(Element);
        }

        protected void RemoveElement(MapElement Element)
        {
            CheckVisual(Element, false);
            _elements.Remove(Element);
        }

        /// <summary>Проверяет, и при необходимости отрисовывает или скрывает объект с карты</summary>
        /// <param name="Element">Проверяемый объект</param>
        private void CheckVisual(MapElement Element) { CheckVisual(Element, VisibleArea); }

        /// <summary>Проверяет, и при необходимости отрисовывает или скрывает объект с карты</summary>
        /// <param name="Element">Проверяемый объект</param>
        /// <param name="OnArea">Видимая в область карты</param>
        private void CheckVisual(MapElement Element, EarthArea OnArea) { CheckVisual(Element, Element.TestVisual(OnArea)); }

        /// <summary>Проверяет, и при необходимости отрисовывает или скрывает объект с карты</summary>
        /// <param name="Element">Проверяемый объект</param>
        /// <param name="IsElementVisible">Видим ли объект на карте в данный момент</param>
        private void CheckVisual(MapElement Element, bool IsElementVisible)
        {
            if (IsElementVisible && Element.ZoomRestriction <= ZoomLevel)
            {
                if (Element.AttachedVisual == null) VisualizeElement(Element);
            }
            else if (Element.AttachedVisual != null) HideElement(Element);
        }

        /// <summary>Выводит визуальное представление элемента на карту</summary>
        /// <param name="Element">Элемент для визуализации</param>
        private void VisualizeElement(MapElement Element)
        {
            Element.AttachedVisual = Element.GetVisual(ZoomLevel);
            Element.AttachedVisual.Transform = _globalTransform;
            Element.ChangeVisualRequested += OnMapElementChangeVisualRequested;
            AddVisual(Element.AttachedVisual);
        }

        /// <summary>Скрывает визуальное представление с карты</summary>
        /// <param name="Element">Элемент для сокрытия</param>
        private void HideElement(MapElement Element)
        {
            if (Element.AttachedVisual == null) return;

            Element.AttachedVisual.Transform = null;
            DeleteVisual(Element.AttachedVisual);
            Element.ChangeVisualRequested -= OnMapElementChangeVisualRequested;
            Element.AttachedVisual = null;
        }

        private void RedrawElement(MapElement Element)
        {
            if (Element.TestVisual(VisibleArea))
                if (Element.AttachedVisual != null)
                    Element.RedrawVisual(ZoomLevel);
                else
                    VisualizeElement(Element);
            else
                HideElement(Element);
        }

        /// <summary>Выполняет действия по перерисовке визуального отображения элемента карты</summary>
        private void OnMapElementChangeVisualRequested(object Sender, EventArgs Args)
        {
            var element = (MapElement)Sender;
            if (element.AttachedVisual != null)
                HideElement(element);
            if (element.TestVisual(VisibleArea))
                VisualizeElement(element);
        }

        #endregion

        #region Скроллинг и позиционирование карты

        #region CentralPoint DependencyProperty

        public static readonly DependencyProperty CentralPointProperty =
            DependencyProperty.Register("CentralPoint", typeof (EarthPoint), typeof (MapView),
                                        new FrameworkPropertyMetadata(new EarthPoint(56.8302, 60.4928),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                      CentralPointPropertyChangedCallback));

        public EarthPoint CentralPoint
        {
            get { return (EarthPoint)GetValue(CentralPointProperty); }
            set { SetValue(CentralPointProperty, value); }
        }

        private static void CentralPointPropertyChangedCallback(DependencyObject Obj,
                                                                DependencyPropertyChangedEventArgs e)
        {
            var newPoint = (EarthPoint)e.NewValue;
            var map = (MapView)Obj;
            map.OnCentralPointChanged(newPoint);
        }

        #endregion

        #region VisibleArea DependencyProperty

        public static readonly DependencyPropertyKey VisibleAreaPropertyKey = DependencyProperty
            .RegisterReadOnly("VisibleArea", typeof (EarthArea), typeof (MapView),
                              new PropertyMetadata(default(EarthArea), VisibleAreaPropertyChangedCallback));

        public static readonly DependencyProperty VisibleAreaProperty =
            VisibleAreaPropertyKey.DependencyProperty;

        public EarthArea VisibleArea
        {
            get { return (EarthArea)GetValue(VisibleAreaProperty); }
            protected set { SetValue(VisibleAreaPropertyKey, value); }
        }

        private static void VisibleAreaPropertyChangedCallback(DependencyObject Obj,
                                                               DependencyPropertyChangedEventArgs e)
        {
            var map = (MapView)Obj;
            var newVisibleArea = (EarthArea)e.NewValue;
            map.OnVisibleAreaChanged(newVisibleArea);
        }

        #endregion

        public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
            "ZoomLevel", typeof (int), typeof (MapView), new PropertyMetadata(13, ZoomChanged, CoerceZoom));

        private static object CoerceZoom(DependencyObject D, object Basevalue)
        {
            var z = (int)Basevalue;
            return Math.Max(2, Math.Min(18, z));
        }

        private readonly TranslateTransform _globalTransform;
        private Point? _dragStartPoint;
        private double _isMapWasMovedDisstance;

        public int ZoomLevel
        {
            get { return (int)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }
        }

        private static void ZoomChanged(DependencyObject Sender, DependencyPropertyChangedEventArgs Args) { ((MapView)Sender).ZoomChanged(Args); }

        private void ZoomChanged(DependencyPropertyChangedEventArgs e)
        {
            OnCentralPointChanged(CentralPoint);
            _tiles.ToList().ForEach(RemoveTile);
            _elements.ForEach(RedrawElement);
            RefreshTiles();
        }

        protected virtual void OnCentralPointChanged(EarthPoint newCentralPoint)
        {
            Point screenCentralPoint = Projector.Project(newCentralPoint, ZoomLevel);
            _globalTransform.X = Math.Round(-screenCentralPoint.X + ActualWidth / 2);
            _globalTransform.Y = Math.Round(-screenCentralPoint.Y + ActualHeight / 2);

            VisibleArea = new EarthArea(
                // Top Left
                Projector.InverseProject(screenCentralPoint + new Vector(-ActualWidth / 2, -ActualHeight / 2), ZoomLevel),
                // Bottom Left
                Projector.InverseProject(screenCentralPoint + new Vector(-ActualWidth / 2, +ActualHeight / 2), ZoomLevel),
                // Top Right
                Projector.InverseProject(screenCentralPoint + new Vector(+ActualWidth / 2, -ActualHeight / 2), ZoomLevel),
                // BottomRight
                Projector.InverseProject(screenCentralPoint + new Vector(+ActualWidth / 2, +ActualHeight / 2), ZoomLevel)
                );

            RefreshTiles();
        }

        protected virtual void OnVisibleAreaChanged(EarthArea NewVisibleArea) { RefreshObjectsVisuals(); }

        public void Move(Vector delta)
        {
            Point p0 = ScreenProjector.DefaultProjector.Project(CentralPoint, ZoomLevel);
            Point p = p0 - delta;
            CentralPoint = ScreenProjector.DefaultProjector.InverseProject(p, ZoomLevel);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            EarthPoint pointedPoint = PointAt(e.GetPosition(this));
            if (e.Delta < 0)
            {
                ZoomLevel--;
                CentralPoint = new EarthPoint(CentralPoint.Latitude - 1.0 * (pointedPoint.Latitude - CentralPoint.Latitude),
                                              CentralPoint.Longitude - 1.0 * (pointedPoint.Longitude - CentralPoint.Longitude));
            }
            if (e.Delta > 0)
            {
                ZoomLevel++;
                CentralPoint = new EarthPoint(CentralPoint.Latitude + 0.5 * (pointedPoint.Latitude - CentralPoint.Latitude),
                                              CentralPoint.Longitude + 0.5 * (pointedPoint.Longitude - CentralPoint.Longitude));
            }
            base.OnMouseWheel(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(this);
            _isMapWasMovedDisstance = 0;
            _isMouseGestureWasStartedOnMap = true;
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            _isMouseGestureWasStartedOnMap = false;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _dragStartPoint != null)
            {
                Point dragCurrentPoint = e.GetPosition(this);
                _isMapWasMovedDisstance += (dragCurrentPoint - _dragStartPoint).Value.Length;
                if (_dragStartPoint != null) Move(dragCurrentPoint - _dragStartPoint.Value);
                _dragStartPoint = dragCurrentPoint;
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (_isMapWasMovedDisstance < 10 && _isMouseGestureWasStartedOnMap)
            {
                var act = MouseAction.None;
                if (e.ChangedButton == MouseButton.Left)
                    act = e.ClickCount == 1 ? MouseAction.LeftClick : MouseAction.LeftDoubleClick;
                else if (e.ChangedButton == MouseButton.Right)
                    act = e.ClickCount == 1 ? MouseAction.RightClick : MouseAction.RightDoubleClick;
                else if (e.ChangedButton == MouseButton.Middle)
                    act = MouseAction.WheelClick;

                if (act != MouseAction.None) OnClick(act, e.GetPosition(this));
            }
            _dragStartPoint = null;
            base.OnMouseUp(e);
        }

        #region Работа с мышью

        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register("ClickCommand", typeof (ICommand), typeof (MapView),
                                        new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty ClickCommandParameterProperty =
            DependencyProperty.Register("ClickCommandParameter", typeof (Object), typeof (MapView),
                                        new PropertyMetadata(default(Object)));

        private bool _isMouseGestureWasStartedOnMap;

        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        public Object ClickCommandParameter
        {
            get { return GetValue(ClickCommandParameterProperty); }
            set { SetValue(ClickCommandParameterProperty, value); }
        }

        public event EventHandler<MapMouseActionEventArgs> Click;

        private void OnClick(MouseAction ActionKind, Point ScreenPoint)
        {
            var eventArgs = new MapMouseActionEventArgs(PointAt(ScreenPoint), ActionKind);
            EventHandler<MapMouseActionEventArgs> handler = Click;
            if (handler != null)
                handler(this, eventArgs);

            if (ClickCommand != null)
                ClickCommand.Execute(eventArgs);
        }

        #endregion

        #endregion
    }

    /// <summary>Представляет данные для событий, связанных с нажатием мышью на карте</summary>
    public class MapMouseActionEventArgs : EventArgs
    {
        public MapMouseActionEventArgs(EarthPoint Point, MouseAction Action)
        {
            this.Action = Action;
            this.Point = Point;
        }

        /// <summary>Вид нажатия</summary>
        public MouseAction Action { get; private set; }

        /// <summary>Точка на карте, в которой совершено нажатие</summary>
        public EarthPoint Point { get; private set; }
    }
}
