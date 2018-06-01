using Parrot.Viewer.GallerySources.Exif;

namespace Parrot.Viewer.GallerySources
{
    public interface IPhotoEntity
    {
        string FileName { get; }
        ExifInformation Exif { get; }
    }
}
