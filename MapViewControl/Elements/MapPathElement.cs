using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Geographics;

namespace MapVisualization.Elements
{
    /// <summary>Элемент карты, состоящий из нескольких точек</summary>
    public abstract class MapPathElement : MapElement
    {
        private readonly double _screenStepSquared;

        /// <summary>Создаёт новый многоточечный объект на карте</summary>
        /// <param name="Points">Точки, входящие в состав объекта</param>
        /// <param name="ScreenStep">Минимальная длинна сегмента для отрисовки на экране</param>
        public MapPathElement(IList<EarthPoint> Points, double ScreenStep = 5)
        {
            this.Points = Points;
            _screenStepSquared = Math.Pow(ScreenStep, 2);
            ElementArea = new EarthArea(Points.ToArray());
        }

        protected virtual EarthArea ElementArea { get; }

        /// <summary>Точки, входящие в состав объекта</summary>
        public IList<EarthPoint> Points { get; }

        protected IEnumerable<Point> GetScreenPoints(int Zoom)
        {
            Point? previousPoint = null;
            Point? tailPoint = null;
            foreach (var point in Points)
            {
                tailPoint = Projector.Project(point, Zoom);
                if (previousPoint == null || (previousPoint.Value - tailPoint.Value).LengthSquared >= _screenStepSquared)
                {
                    previousPoint = tailPoint;
                    yield return tailPoint.Value;
                }
            }
            if (previousPoint != null &&
                tailPoint != previousPoint.Value)
            {
                yield return tailPoint.Value;
            }
        }

        /// <summary>Проверяет, попадает ли этот элемент в указанную области видимости</summary>
        /// <param name="VisibleArea">Область видимости</param>
        /// <returns>True, если объект может оказаться виден в указанной области</returns>
        public override bool TestVisual(EarthArea VisibleArea) { return ElementArea.IsIntersects(VisibleArea); }
    }
}
