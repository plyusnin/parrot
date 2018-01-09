using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MapVisualization.Elements;

namespace MapVisualization
{
    /// <summary>Панель - хост для визуальных элементов</summary>
    public abstract class MapVisualHost : Panel
    {
        /// <summary>Визуальные элементы карты</summary>
        private readonly VisualCollection _visuals;

        private MouseButton? _clickButton;
        private MapElement _mouseDownOnElement;

        private MapElement _mouseMoveOnElement;

        protected MapVisualHost() { _visuals = new VisualCollection(this); }

        protected override int VisualChildrenCount
        {
            get { return _visuals.Count; }
        }

        protected override Visual GetVisualChild(int index) { return _visuals[index]; }

        /// <summary>Добавляет визуальный элемент на карту</summary>
        /// <param name="v">Визуальный элемент</param>
        protected void AddVisual(MapVisual v)
        {
            int index;
            for (index = _visuals.Count; index > 0; index--)
                if (((MapVisual)_visuals[index - 1]).ZIndex <= v.ZIndex) break;

            _visuals.Insert(index, v);
        }

        /// <summary>Удаляет визуальный элемент с карты</summary>
        /// <param name="v"></param>
        protected void DeleteVisual(MapVisual v) { _visuals.Remove(v); }

        /// <summary>Проверяет попадание мыши по элементу карты</summary>
        public MapVisual HitVisual(Point point) { return VisualTreeHelper.HitTest(this, point).VisualHit as MapVisual; }

        private MapElement safeGetElement(MapVisual Visual) { return Visual != null ? Visual.Element : null; }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var newMouseMoveOnElement = safeGetElement(HitVisual(e.GetPosition(this)));
            if (!Equals(_mouseMoveOnElement, newMouseMoveOnElement))
            {
                if (_mouseMoveOnElement != null) _mouseMoveOnElement.OnMouseLeave(e);
                if (newMouseMoveOnElement != null) newMouseMoveOnElement.OnMouseEnter(e);
            }
            if (_mouseMoveOnElement != null) _mouseMoveOnElement.OnMouseMove(e);
            _mouseMoveOnElement = newMouseMoveOnElement;
            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            _clickButton = e.ChangedButton;
            _mouseDownOnElement = safeGetElement(HitVisual(e.GetPosition(this)));
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (_clickButton == e.ChangedButton)
            {
                var mouseUpOnElement = safeGetElement(HitVisual(e.GetPosition(this)));
                if (mouseUpOnElement != null && Equals(_mouseDownOnElement, mouseUpOnElement))
                    mouseUpOnElement.OnMouseClick(e);
            }
            _clickButton = null;
            base.OnMouseUp(e);
        }
    }
}
