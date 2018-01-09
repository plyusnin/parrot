using System;
using System.Collections.Generic;
using System.Windows;
using Geographics;

namespace MapVisualization
{
    public class ScreenProjector
    {
        private static readonly ScreenProjector _defaultProjector = new ScreenProjector();

        /// <summary>Таблица масштабов</summary>
        /// <remarks>Задаёт масштаб (в метрах на пиксел) для разного уровня масштабирования</remarks>
        private static readonly Dictionary<int, double> Scales =
            new Dictionary<int, double>
            {
                { 18, 0.597164 },
                { 17, 1.194329 },
                { 16, 2.388657 },
                { 15, 4.777314 },
                { 14, 9.554629 },
                { 13, 19.109257 },
                { 12, 38.218514 },
                { 11, 76.437028 },
                { 10, 152.874057 },
                { 9, 305.748113 },
                { 8, 611.496226 },
                { 7, 1222.992453 },
                { 6, 2445.984905 },
                { 5, 4891.969810 },
                { 4, 9783.939621 },
                { 3, 19567.879241 },
                { 2, 39135.758482 }
            };

        public static ScreenProjector DefaultProjector
        {
            get { return _defaultProjector; }
        }

        public Point Project(EarthPoint p, int Zoom)
        {
            var surfacePoint = (SurfacePoint)p;
            double mpp = Scales[Zoom];
            return new Point(Math.Round(surfacePoint.X / mpp), Math.Round(-surfacePoint.Y / mpp));
        }

        public EarthPoint InverseProject(Point Point, int Zoom)
        {
            double mpp = Scales[Zoom];
            var surfacePoint = new SurfacePoint(Point.X * mpp, -Point.Y * mpp);
            return (EarthPoint)surfacePoint;
        }
    }
}
