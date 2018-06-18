using System.Collections.Generic;
using Geographics;
using MapVisualization;

namespace Parrot.Viewer.GallerySources
{
    public interface IGeoIndexer
    {
        IEnumerable<GeotaggingInformation> ListGeotagsFor(EarthPoint Point);
        int GetX(Degree Longitude, int Scale);
        int GetY(Degree Latitude, int Scale);
    }

    public class OsmGeoIndexer : IGeoIndexer
    {
        public IEnumerable<GeotaggingInformation> ListGeotagsFor(EarthPoint Point)
        {
            for (var scale = 1; scale < 19; scale++)
                yield return new GeotaggingInformation(
                    scale,
                    OsmIndexes.GetHorizontalIndex(Point.Longitude, scale),
                    OsmIndexes.GetVerticalIndex(Point.Latitude, scale));
        }

        public int GetX(Degree Longitude, int Scale)
        {
            return OsmIndexes.GetHorizontalIndex(Longitude, Scale);
        }

        public int GetY(Degree Latitude, int Scale)
        {
            return OsmIndexes.GetVerticalIndex(Latitude, Scale);
        }
    }

    public class GeotaggingInformation
    {
        public GeotaggingInformation(int Scale, int X, int Y)
        {
            this.Scale = Scale;
            this.X = X;
            this.Y = Y;
        }

        public int Scale { get; }
        public int X { get; }
        public int Y { get; }

        public override string ToString()
        {
            return $"{nameof(Scale)} {Scale}: {X}-{Y}";
        }
    }
}
