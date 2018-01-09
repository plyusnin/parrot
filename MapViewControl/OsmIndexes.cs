using System;
using Geographics;

namespace MapVisualization
{
    public static class OsmIndexes
    {
        public static int TileWidth
        {
            get { return 256; }
        }

        public static int TileHeight
        {
            get { return 256; }
        }

        public static int GetHorizontalIndex(double Longitude, int Zoom)
        {
            return (int)Math.Floor((Longitude + 180) / 360 * (1 << Zoom));
        }

        public static int GetVerticalIndex(double Latitude, int Zoom)
        {
            return
                (int)
                Math.Floor((1
                            - Math.Log(Math.Tan(Math.PI * Latitude / 180) + 1 / Math.Cos(Math.PI * Latitude / 180))
                            / Math.PI) / 2 * (1 << Zoom));
        }

        public static double GetLongitude(int horizontalIndex, int Zoom)
        {
            return horizontalIndex / Math.Pow(2.0, Zoom) * 360.0 - 180;
        }

        public static double GetLatitude(int verticalIndex, int Zoom)
        {
            double n = Math.PI - 2.0 * Math.PI * verticalIndex / Math.Pow(2.0, Zoom);
            return 180.0 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n)));
        }

        public static Uri GetTileUri(int x, int y, int zoom)
        {
            return new Uri(String.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", zoom, x, y));
        }

        public static Uri GetTileUri(double Latitude, double Longitude, int zoom)
        {
            return GetTileUri(GetHorizontalIndex(Longitude, zoom), GetVerticalIndex(Latitude, zoom), zoom);
        }

        public static EarthPoint GetTopLeftPoint(int HorizontalIndex, int VerticalIndex, int Zoom)
        {
            return new EarthPoint(GetLatitude(VerticalIndex, Zoom), GetLongitude(HorizontalIndex, Zoom));
        }
    }
}
