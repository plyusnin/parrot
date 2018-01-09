using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media;
using Geographics;

namespace MapVisualization.Elements
{
    /// <summary>Элемент карты</summary>
    [ZoomRestriction(0)]
    public abstract class MapElement
    {
        protected static readonly GuidelineSet ScreenGuidelineSet;
        private static readonly Dictionary<Type, int> _zoomRestrictions = new Dictionary<Type, int>();
        private readonly Lazy<int> _lazyZoomRestriction;

        static MapElement()
        {
            ScreenGuidelineSet = new GuidelineSet(
                Enumerable.Range(0, 40000).Select(x => (double)x).ToArray(),
                Enumerable.Range(0, 40000).Select(y => (double)y).ToArray());
        }

        protected MapElement() { _lazyZoomRestriction = new Lazy<int>(GetZoomRestriction); }

        /// <summary>Z-индекс элемента на карте</summary>
        /// <remarks>Меньшее значения индекса соответствуют нижним слоям на карте</remarks>
        protected virtual int ZIndex
        {
            get { return 0; }
        }

        /// <summary>Проектор географических координат в экранные</summary>
        protected ScreenProjector Projector
        {
            get { return ScreenProjector.DefaultProjector; }
        }

        /// <summary>Визуальный элемент, изображающий данный элемент карты</summary>
        internal MapVisual AttachedVisual { get; set; }

        public Boolean IsMouseOver { get; private set; }

        public int ZoomRestriction
        {
            get { return _lazyZoomRestriction.Value; }
        }

        /// <summary>Событие, сигнализирующее о том, что элемент запросил изменение своего визуального отображения</summary>
        internal event EventHandler ChangeVisualRequested;

        /// <summary>Отправляет запрос на смену своего визуального отображения</summary>
        protected void RequestChangeVisual()
        {
            EventHandler handler = ChangeVisualRequested;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>Отрисовывает объект в указанном контексте рисования</summary>
        /// <param name="dc">Контекст рисования</param>
        /// <param name="Zoom">Индекс масштаба рисования</param>
        protected abstract void Draw(DrawingContext dc, int Zoom);

        /// <summary>Получает визуальный элемент для этого элемента карты</summary>
        /// <param name="Zoom">Индекс масштаба отображения</param>
        public MapVisual GetVisual(int Zoom)
        {
            var res = new MapVisual(this, ZIndex);
            using (DrawingContext dc = res.RenderOpen())
            {
                Draw(dc, Zoom);
            }
            return res;
        }

        /// <summary>Перерисовывает визуальный элемент для этого элемента карты</summary>
        /// <param name="Zoom">Индекс масштаба отображения</param>
        public void RedrawVisual(int Zoom)
        {
            using (DrawingContext dc = AttachedVisual.RenderOpen())
            {
                Draw(dc, Zoom);
            }
        }

        /// <summary>Проверяет, попадает ли этот элемент в указанную области видимости</summary>
        /// <param name="VisibleArea">Область видимости</param>
        /// <returns>True, если объект может оказаться виден в указанной области</returns>
        public abstract bool TestVisual(EarthArea VisibleArea);

        public virtual void OnMouseClick(MouseButtonEventArgs ChangedButton) { }
        public virtual void OnMouseMove(MouseEventArgs MouseEventArgs) { }
        public virtual void OnMouseEnter(MouseEventArgs MouseEventArgs) { IsMouseOver = true; }
        public virtual void OnMouseLeave(MouseEventArgs MouseEventArgs) { IsMouseOver = false; }

        private int GetZoomRestriction()
        {
            Type type = GetType();
            int value;
            if (_zoomRestrictions.TryGetValue(type, out value))
                return value;
            int restriction = type.GetCustomAttribute<ZoomRestrictionAttribute>(true).MinZoomLevel;
            _zoomRestrictions.Add(type, restriction);
            return restriction;
        }
    }
}
