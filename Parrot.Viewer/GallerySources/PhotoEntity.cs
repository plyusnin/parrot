using Parrot.Viewer.GallerySources.Exif;

namespace Parrot.Viewer.GallerySources
{
    public class PhotoEntity : IPhotoEntity
    {
        public PhotoEntity(string FileName, ExifInformation Exif)
        {
            this.FileName = FileName;
            this.Exif = Exif;
        }

        public string FileName { get; }
        public ExifInformation Exif { get; }
    }
}
