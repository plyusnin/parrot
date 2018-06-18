using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Geographics;

namespace Parrot.Viewer.GallerySources
{
    public interface IGallery
    {
        IList<IPhotoEntity> All();
        IList<IPhotoEntity> WithinInterval(DateTime From, DateTime To);
        bool Contains(string FileName);
        void Add(IPhotoEntity Entity, Stream Thumbnail);
        Stream OpenThumbnail(IPhotoEntity PhotoEntity);
        IList<IPhotoEntity> All(int Offset, int Count);
        IList<GeoStack> GetPhotosOnMap(int Scale, int FromX, int ToX, int FromY, int ToY);
        IList<GeoStack> GetPhotosOnMap(int Scale, EarthArea ForArea);
    }

    public class GeoStack
    {
        public GeoStack(IList<EarthPoint> Points, IPhotoEntity LastPhoto)
        {
            this.Points = Points;
            this.LastPhoto = LastPhoto;
        }

        public IList<EarthPoint> Points { get; }
        public IPhotoEntity LastPhoto { get; }

        public static IEqualityComparer<GeoStack> ValueComparer { get; } = new ValueEqualityComparer();

        private sealed class ValueEqualityComparer : IEqualityComparer<GeoStack>
        {
            public bool Equals(GeoStack x, GeoStack y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                if (x.Points.Count != y.Points.Count) return false;
                if (x.LastPhoto.FileName != y.LastPhoto.FileName) return false;
                return x.Points.All(p => y.Points.Contains(p));
            }

            public int GetHashCode(GeoStack obj)
            {
                unchecked
                {
                    return (obj.Points.Aggregate(0, (code, p) => code ^ p.GetHashCode()) * 397) ^ obj.LastPhoto.FileName.GetHashCode();
                }
            }
        }
    }

    public enum TimeTakingDirection
    {
        OlderThan,
        NewierThan
    }
}
