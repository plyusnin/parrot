using System;
using System.Collections.Generic;
using System.IO;

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
    }

    public enum TimeTakingDirection
    {
        OlderThan,
        NewierThan
    }
}
