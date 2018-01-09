using System.IO;
using Parrot.Viewer.GallerySources.Exif;
using ReactiveUI;

namespace Parrot.Viewer.GallerySources
{
    public interface IGallerySource
    {
        IReactiveDerivedList<IPhotoEntity> Photos { get; }
    }

    public interface IPhotoEntity
    {
        string FileName { get; }
        ExifInformation Exif { get; }
        Stream OpenThumbnail();
    }
}
