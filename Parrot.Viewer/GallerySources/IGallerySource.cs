using System.IO;
using ReactiveUI;

namespace Parrot.Viewer.GallerySources
{
    public interface IGallerySource
    {
        IReactiveList<IPhotoEntity> Photos { get; }
    }

    public interface IPhotoEntity
    {
        string FileName { get; }
        Stream Thumbnail { get; }
        ExifInformation Exif { get; }
        Stream Open();
    }

    public class PhotoEntity : IPhotoEntity
    {
        private readonly byte[] _thumbnail;

        public PhotoEntity(string FileName, byte[] Thumbnail, ExifInformation Exif)
        {
            _thumbnail = Thumbnail;
            this.FileName = FileName;
            this.Exif = Exif;
        }

        public ExifInformation Exif { get; }
        public string FileName { get; }
        public Stream Open() { return File.OpenRead(FileName); }
        public Stream Thumbnail => new MemoryStream(_thumbnail);
    }
}
